using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotPool : MonoBehaviour
{
    public static SlotPool Instance;
    public BY_Local_Pool<Slot> pool;
    public Slot prefab;
    public  int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<Slot>(prefab, total, this.transform);
    }
    public void DespawnAll()
    {
        foreach (var slot in pool.list)
        {
            slot.gameObject.SetActive(false);   
        }
    }
}
