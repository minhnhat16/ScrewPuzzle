using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum Music
    {
        NULL,
        MainScreenMusic,
        GamplayMusic
    }

    public enum SFX
    {
        NULL,

        ScrewClicked,
        ScrewFaild,

        BoxClose,
        BuyConfirm,
        BuyCancel,
        ButtonClick,

        Star_1,
        Star_2,
        Star_3,

        Win,
        Lose,
        GiftBoxOpen,
        GiftItemAppear,

        AddBox,
        Drill,
        Breaker,
        Magnet,


        MissionComplete,

        Button,
        Close,
        Slide,
        Swoosh,
        Dialog_Appear,
        Dialog_Swipe,
        UI_Toggle,
        UI_Normal,
        Shop_Purchase_Fail,
        Shop_Purchase_Success,
        GoldCollect,
        TicketCollect,
    }

    [SerializeField] public SoundFactory soundFactory;

    // cooldown values (from SoundFactory.Sound_SFX.timer)
    private Dictionary<SFX, float> sfxTimerDictionary;

    // time-to-despawn values (from SoundFactory.Sound_SFX.timeToDespawn)
    public Dictionary<SFX, float> sfxTimerDespawnDictionary;

    // last time a given SFX was played (tracked at runtime)
    private Dictionary<SFX, float> sfxLastPlayed;

    public MusicGameObject musicObject;

    public bool musicSetting;
    public bool sfxSetting;

    public void Init(Action callback)
    {
        // Load factory (guard against missing config)
        soundFactory = ConfigFileManager.Instance.GetConfig<SoundFactory>();
        if (soundFactory == null)
        {
            Debug.LogError("[SoundManager] SoundFactory config not found!");
            // still call callback so boot won't hang
            callback?.Invoke();
            return;
        }

        // Prepare dictionaries
        sfxTimerDictionary = new Dictionary<SFX, float>();
        sfxTimerDespawnDictionary = new Dictionary<SFX, float>();
        sfxLastPlayed = new Dictionary<SFX, float>();

        // Fill dictionaries from ScriptableObject safely
        for (int i = 0; i < soundFactory.sfxList.Count; i++)
        {
            var entry = soundFactory.sfxList[i];
            if (entry == null) continue;

            // store cooldown (timer) and despawn values
            sfxTimerDictionary[entry.sfx] = entry.timer;
            sfxTimerDespawnDictionary[entry.sfx] = entry.timeToDespawn;

            // initialize last-played time so the SFX is immediately playable
            sfxLastPlayed[entry.sfx] = -Mathf.Infinity;
        }


        // Ensure a MusicGameObject exists with an AudioSource
        if (musicObject == null)
        {
            musicObject = FindAnyObjectByType<MusicGameObject>();
            if (musicObject == null)
            {
                var go = new GameObject("MusicGameObject");
                musicObject = go.AddComponent<MusicGameObject>();
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = true;
            }
        }
        else
        {
            // ensure AudioSource exists
            var srcCheck = musicObject.GetComponent<AudioSource>();
            if (srcCheck == null)
            {
                var src = musicObject.gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = true;
            }
        }

        // Apply initial volumes/mute according to settings
        SettingMusicVolume(musicSetting);
        SettingSFXVolume(sfxSetting);

        // Invoke callback to signal completion
        callback?.Invoke();
    }
    public void VolumeSetting(bool musicSetting, bool sfxSetting)
    {
        this.musicSetting = musicSetting;
        this.sfxSetting = sfxSetting;
        SettingMusicVolume(this.musicSetting);
        SettingSFXVolume(this.sfxSetting);
    }

    // Optimized, data-driven cooldown check.
    // Uses sfxTimerDictionary as cooldown (seconds) and sfxLastPlayed to track last play time.
    private bool CanPlaySFX(SFX sfx)
    {
        if (!sfxSetting)
            return false;

        // get configured cooldown (default 0 = no throttle)
        float cooldown = 0f;
        sfxTimerDictionary?.TryGetValue(sfx, out cooldown);

        // get last played timestamp (default -inf => immediately playable)
        float lastPlayed = -Mathf.Infinity;
        if (sfxLastPlayed != null && sfxLastPlayed.TryGetValue(sfx, out var lp))
            lastPlayed = lp;

        // if no cooldown configured or cooldown <= 0 → always allow and record play time
        if (cooldown <= 0f)
        {
            if (sfxLastPlayed != null) sfxLastPlayed[sfx] = Time.time;
            return true;
        }

        // allow only if enough time has passed since last play
        if (Time.time - lastPlayed >= cooldown)
        {
            if (sfxLastPlayed != null) sfxLastPlayed[sfx] = Time.time;
            return true;
        }

        return false;
    }

    public void PlayMusic(Music music)
    {
        musicObject.music = music;
        AudioSource audioSource = musicObject.GetComponent<AudioSource>();
        audioSource.clip = GetMusicAudioClip(music);
        audioSource.Play();
        //Debug.Log("Music " + music + " played!");
    }

    public void PlaySFX(SFX sfx)
    {
        if (CanPlaySFX(sfx))
        {
            SFXGameObj soundGameObj = SoundGameObjPool.instance.pool.SpawnNonGravity();
            soundGameObj.AutoDespawnSFX(sfx);
            soundGameObj.sfx = sfx;
            AudioSource audioSource = soundGameObj.gameObject.GetComponent<AudioSource>();
            SettingSFXVolume(sfxSetting);
            audioSource.PlayOneShot(GetSFXAudioClip(sfx));
        }
    }
    public void PlaySFXWithVolume(SFX sfx, float value)
    {
        if (CanPlaySFX(sfx))
        {
            SFXGameObj soundGameObj = SoundGameObjPool.instance.pool.SpawnNonGravity();
            soundGameObj.sfx = sfx;
            AudioSource audioSource = soundGameObj.gameObject.GetComponent<AudioSource>();
            //Debug.Log(soundGameObj.name.ToString());
            SettingSFXVolumeWithValue(sfxSetting, value);
            audioSource.PlayOneShot(GetSFXAudioClip(sfx));
            //Debug.Log("SFX " + sfx + " played!");
        }
    }
    public void StopMusic()
    {
        AudioSource audioSource = musicObject.GetComponent<AudioSource>();
        audioSource.Stop();
    }

    public void StopSFX(SFX sfx)
    {
        foreach (SFXGameObj obj in SoundGameObjPool.instance.pool.list)
        {
            if (obj.sfx == sfx && obj.gameObject.activeSelf)
            {
                //Debug.Log("Stop " + sfx + " sfx");
                SoundGameObjPool.instance.pool.DeSpawnNonGravity(obj);
            }
        }
    }

    public void SettingMusicVolume(bool valid)
    {
        AudioSource audioSource = musicObject.GetComponent<AudioSource>();

        if (valid)
        {
            audioSource.volume = 1f;
            audioSource.Play();
        }
        else
        {
            //Debug.Log("mute music");
            audioSource.volume = 0f;
            audioSource.Pause();
        }
    }

    public void SettingSFXVolume(bool valid)
    {
       this.sfxSetting = valid;
        foreach (SFXGameObj obj in SoundGameObjPool.instance.pool.list)
        {
            if (obj.sfx != SFX.NULL && obj.gameObject.activeSelf)
            {
                if (valid)
                {
                    //Debug.Log("UnMute SFX");
                    obj.GetComponent<AudioSource>().volume = 1f;
                }
                else
                {
                    //Debug.Log("Mute SFX");
                    obj.GetComponent<AudioSource>().volume = 0f;
                }
            }
        }
    }
    public void SettingSFXVolumeWithValue(bool valid, float value)
    {
        foreach (SFXGameObj obj in SoundGameObjPool.instance.pool.list)
        {
            if (obj.sfx != SFX.NULL && obj.gameObject.activeSelf)
            {
                if (valid)
                {
                    //Debug.Log("UnMute SFX");
                    obj.GetComponent<AudioSource>().volume = value;
                }
                else
                {
                    //Debug.Log("Mute SFX");
                    obj.GetComponent<AudioSource>().volume = 0f;
                }
            }
        }
    }
    public AudioClip GetMusicAudioClip(Music music)
    {
        foreach (SoundFactory.Music_SFX item in soundFactory.musicList)
        {
            if (item.music == music)
            {
                return item.audioClip;
            }
        }
        //Debug.LogError("Music " + music + " not found!");
        return null;
    }

    public AudioClip GetSFXAudioClip(SFX sfx)
    {
        foreach (SoundFactory.Sound_SFX item in soundFactory.sfxList)
        {
            if (item.sfx == sfx)
            {
                return item.audioClip;
            }
        }
        //Debug.LogError("SFX " + sfx + " not found!");
        return null;
    }
}