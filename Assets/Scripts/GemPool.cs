using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemPool : MonoBehaviour
{
    public static GemPool Instance;
    public BY_Local_Pool<GemUI> pool;
    public GemUI prefab;
    public int total;

    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<GemUI>(prefab, total, this.transform);
    }
}
