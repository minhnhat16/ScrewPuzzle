using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionItemPool : MonoBehaviour
{
    public static CollectionItemPool Instance;
    public BY_Local_Pool<CollectionItem> pool;
    public CollectionItem prefab;
    public RectTransform parent;
    public int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<CollectionItem>(prefab, total, parent);
    }
}
