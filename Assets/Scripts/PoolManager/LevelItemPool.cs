using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelItemPool : MonoBehaviour
{
    public static LevelItemPool Instance;
    public BY_Local_Pool<LevelItem> pool;
    public LevelItem prefab;
    public RectTransform parent;
    public int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<LevelItem>(prefab, total, parent);
    }

}
