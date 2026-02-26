using Enums;
using Ingame.Screw;
using System.Collections.Generic;
using UnityEngine;

public class BoxScrewStorage : MonoBehaviour
{
    [SerializeField] private Transform screwRoot;

    private List<ScrewController> screws = new();
    private int capacity;
    private ColorEnum boxColor;

    public bool IsFull => screws.Count >= capacity;
    public int RemainingCapacity => capacity - screws.Count;

    public void Initialize(int cap, ColorEnum color)
    {
        capacity = cap;
        boxColor = color;
        screws.Clear();
    }

    public bool TryAdd(ScrewController screw)
    {
        if (IsFull) return false;
        if (screw.GetColor() != boxColor) return false;

        screws.Add(screw);

        screw.transform.SetParent(screwRoot);
        screw.transform.localPosition = Vector3.zero;

        return true;
    }

    public void Clear()
    {
        screws.Clear();
    }
}