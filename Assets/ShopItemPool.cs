using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemPool : MonoBehaviour
{
    public static ShopItemPool Instance;
    public BY_Local_Pool<ShopItem> Pool;
    public ShopItem prefab;
    public int total;
    public Transform content;
    private void Awake()
    {
        Instance = this;
        Pool = new BY_Local_Pool<ShopItem>(prefab, total, content);
    }
}