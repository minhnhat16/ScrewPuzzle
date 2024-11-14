using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarPool : MonoBehaviour
{
    public static StarPool Instance;
    public BY_Local_Pool<BoxStar> pool;
    public BoxStar prefab;
    public int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<BoxStar>(prefab, total, transform);
    }
}
