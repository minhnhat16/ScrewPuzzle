using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXGameObj : MonoBehaviour
{
    public SoundManager.SFX sfx;

    public void AutoDespawnSFX(SoundManager.SFX sfx)
    {
        if (SoundManager.instance.sfxTimerDespawnDictionary.TryGetValue(sfx, out float time))
        {
            StartCoroutine(DespawnSFX(time));
        }
    }

    IEnumerator DespawnSFX(float time)
    {
        yield return new WaitForSeconds(time);
        SoundGameObjPool.instance.pool.DeSpawnNonGravity(this);
    }
}
