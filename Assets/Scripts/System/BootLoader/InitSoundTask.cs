using System.Collections;
using System.DataBase;
using UnityEngine;

public class InitSoundTask : IBootTask
{
    public string Name => "InitSound";

    public IEnumerator Execute()
    {
        Debug.Log("[BOOT] InitSound...");
        bool done = false;

        bool musicData = DataAPIController.instance.GetMusicSetting();
        bool sfxData = DataAPIController.instance.GetSoundSetting();
        SoundHelper.SetEnabled(musicData, sfxData);

        SoundManager.instance.Init(() => done = true);
        SoundHelper.PlayMusic(SoundManager.Music.MainScreenMusic);

        yield return new WaitUntil(() => done);
    }
}