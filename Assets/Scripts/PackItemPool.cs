using System.Collections;
using System.Collections.Generic;
using UIScript;
using UnityEngine;

public class PackItemPool : MonoBehaviour
{
    public static PackItemPool Instance;
    public BY_Local_Pool<PackItem> Pool;
    public PackItem prefab;
    public int total;
    public Transform content;
    private void Awake()
    {
        Instance = this;
        Pool = new BY_Local_Pool<PackItem>(prefab, total, content);
    }
}
