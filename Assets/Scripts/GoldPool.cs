using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldPool : MonoBehaviour
{
    public static GoldPool Instance;
    public BY_Local_Pool<GoldUI> pool;
    public GoldUI prefab;
    public int total;

    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<GoldUI>(prefab, total, this.transform);
    }

}
