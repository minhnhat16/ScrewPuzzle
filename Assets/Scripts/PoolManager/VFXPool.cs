using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPool : MonoBehaviour
{
    [System.NonSerialized]
    public BY_Local_Pool<SplashVfx> pool;
    public SplashVfx prefab;
    public static VFXPool Instance;
    public ColorConfig config;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<SplashVfx>(prefab, 40, this.transform);
    }
     IEnumerator Start()
    {
        yield return new WaitUntil(() => ConfigFileManager.Instance.ColorConfig != null);
        config = ConfigFileManager.Instance.ColorConfig;
    }
    public Color GetColor(CardColorPallet color)
    {
        var c = config.GetRecordByKeySearch(color);
        if(c== null ) return Color.clear;
        return c.Color;
    }
    public void PlayParticleAt(ParticleSystem p, Vector3 pos)
    {
        p.transform.SetLocalPositionAndRotation(pos, Quaternion.identity);
        p.Play();
    }
   
}
