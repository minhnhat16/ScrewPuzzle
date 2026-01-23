using UnityEngine;
using SFX = SoundManager.SFX;
using Music = SoundManager.Music;

public static class SoundHelper
{
    /// <summary>
    /// Play an SFX via SoundManager (safe null-check).
    /// </summary>
    public static void PlaySFX(SFX sfx)
    {
        var sm = SoundManager.instance;
        if (sm == null)
        {
            Debug.LogWarning("[SoundHelper] SoundManager.instance is null. Cannot PlaySFX.");
            return;
        }

        sm.PlaySFX(sfx);
    }

    /// <summary>
    /// Play an SFX with explicit volume (0..1). Respects SoundManager's CanPlaySFX logic.
    /// </summary>
    public static void PlaySFX(SFX sfx, float volume)
    {
        var sm = SoundManager.instance;
        if (sm == null)
        {
            Debug.LogWarning("[SoundHelper] SoundManager.instance is null. Cannot PlaySFXWithVolume.");
            return;
        }

        sm.PlaySFXWithVolume(sfx, Mathf.Clamp01(volume));
    }

    /// <summary>
    /// Play background music (safe).
    /// </summary>
    public static void PlayMusic(Music music)
    {
        var sm = SoundManager.instance;
        if (sm == null)
        {
            Debug.LogWarning("[SoundHelper] SoundManager.instance is null. Cannot PlayMusic.");
            return;
        }

        sm.PlayMusic(music);
    }

    /// <summary>
    /// Stop background music.
    /// </summary>
    public static void StopMusic()
    {
        SoundManager.instance?.StopMusic();
    }

    /// <summary>
    /// Stop a specific SFX (despawn pooled SFX instances).
    /// </summary>
    public static void StopSFX(SFX sfx)
    {
        SoundManager.instance?.StopSFX(sfx);
    }

    /// <summary>
    /// Update music / sfx enabled settings on SoundManager.
    /// </summary>
    public static void SetEnabled(bool musicEnabled, bool sfxEnabled)
    {
        var sm = SoundManager.instance;
        if (sm == null)
        {
            Debug.LogWarning("[SoundHelper] SoundManager.instance is null. Cannot SetEnabled.");
            return;
        }

        sm.VolumeSetting(musicEnabled, sfxEnabled);
    }
}