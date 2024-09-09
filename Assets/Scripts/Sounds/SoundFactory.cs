using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Sound Factory", fileName = "SoundFactory")]
public class SoundFactory : ScriptableObject    
{
    public List<Music_SFX> musicList = new List<Music_SFX>();
    public List<Sound_SFX> sfxList = new List<Sound_SFX>();

    [System.Serializable]
    public class Sound_SFX
    {
        public SoundManager.SFX sfx;
        public float timer;
        public float timeToDespawn;
        public AudioClip audioClip;
    }

    [System.Serializable]
    public class Music_SFX
    {
        public SoundManager.Music music;
        public AudioClip audioClip;
    }
}
