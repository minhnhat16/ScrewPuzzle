using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour
{
    public static CardPool Instance;
    public BY_Local_Pool<Card> pool;
    public Card prefab;
    public  int total;

    private void Awake()
    {
       Instance = this;
        pool = new BY_Local_Pool<Card>(prefab, total, this.transform);
    }
    
}
