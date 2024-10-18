using System.Collections;
using System.Collections.Generic;
using Ingame;
using UnityEngine;

public class TwoHoldBoxPool : MonoBehaviour
{
    public static TwoHoldBoxPool Instance;
    public BY_Local_Pool<BoxTwoHold> pool;
    public BoxTwoHold prefab;
    public int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<BoxTwoHold>(prefab, total, transform);
    }
}
