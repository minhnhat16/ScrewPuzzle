using System.Collections;
using System.Collections.Generic;
using Ingame;
using UnityEngine;

public class LevelObjectPool : MonoBehaviour
{
    public static LevelObjectPool Instance;
    public BY_Local_Pool<BaseLevelObject> pool;
    public BaseLevelObject prefab;
    public int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<BaseLevelObject>(prefab, total, transform);
    }

    public BaseLevelObject SpawnNonGravity()
    {
        return pool.SpawnNonGravity();
    }

    public void Despawn(BaseLevelObject obj)
    {
        pool.ReturnToPool(obj);
    }
}
    