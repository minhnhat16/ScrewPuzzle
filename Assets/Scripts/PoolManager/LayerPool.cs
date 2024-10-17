using System.Collections;
using System.Collections.Generic;
using Ingame.Board;
using UnityEngine;

public class LayerPool : MonoBehaviour
{
    public static LayerPool Instance;
    public BY_Local_Pool<BaseLayer> pool;
    public BaseLayer prefab;
    public int total;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<BaseLayer>(prefab, total, transform);
    }
}
