using System;
using UnityEngine;

public class SparkVFXPool : MonoBehaviour
{
    public static SparkVFXPool Instance;
    public BY_Local_Pool<SparkVFX> pool;
    public SparkVFX prefab;
    public int total = 10;

    internal SparkVFX Spawn()
    {
        return pool.SpawnNonGravity();
    }

    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<SparkVFX>(prefab, total, transform);
    }
}
