using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundGameObjPool : MonoBehaviour
{
    [System.NonSerialized]
    public BY_Local_Pool<SFXGameObj> pool;
    public SFXGameObj prefab;
    public static SoundGameObjPool instance;
    private void Awake()
    {
        instance = this;
        pool = new BY_Local_Pool<SFXGameObj>(prefab, 50, this.transform);
    }
}
