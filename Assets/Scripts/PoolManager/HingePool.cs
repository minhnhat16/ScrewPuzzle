using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingePool : MonoBehaviour
{
    
    public static HingePool Instance;
    public BY_Local_Pool<HingeObject> pool;
    public HingeObject prefab;
    public int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<HingeObject>(prefab, total, transform);
    }
}
