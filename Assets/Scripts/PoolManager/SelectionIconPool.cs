using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIconPool : MonoBehaviour
{
    public static SelectionIconPool Instance;
    public BY_Local_Pool<SelectionIcon> pool;
    public SelectionIcon prefab;
    public int total;
    public RectTransform anchor;
    public Vector3 percent;
    private void Awake()
    {
        Instance = this;
        pool = new BY_Local_Pool<SelectionIcon>(prefab, total, anchor.transform);
    }
    private void Start()
    {
        GameObject a;
        for (int i = 0; i < total; i++)
        {
            a= pool.list[i].gameObject;
            a.SetActive(true);
        }
    }
}
