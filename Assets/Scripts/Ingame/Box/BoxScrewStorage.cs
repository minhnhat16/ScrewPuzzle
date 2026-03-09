using Enums;
using Ingame.Screw;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxScrewStorage : MonoBehaviour
{
    [SerializeField] private Transform screwRoot;
    [SerializeField] private List<HoldScrew> holdSlots; // 3 HoldScrew slots trên box

    private readonly List<ScrewController> screws = new();
    private int capacity;
    private ColorEnum boxColor;

    public bool IsFull => screws.Count >= capacity && capacity > 0;
    public int RemainingCapacity => Mathf.Max(0, capacity - screws.Count);

    public void Initialize(int cap, ColorEnum color)
    {
        capacity = cap;
        boxColor = color;
        screws.Clear();

        if (holdSlots == null || holdSlots.Count == 0)
        {
            Debug.LogError($"[BoxScrewStorage] holdSlots chưa được assign trên {gameObject.name}!");
            return;
        }

        foreach (var hold in holdSlots)
            hold.RemoveScrew();

        Debug.Log($"[BoxScrewStorage] Initialized — capacity={capacity}, color={color}, slots={holdSlots.Count}");
    }

    /// <summary>
    /// Thêm screw vào slot trống đầu tiên.
    /// </summary>
    /// <param name="isTele">true = set position ngay, không animate (dùng cho hidden screw)</param>
    public bool TryAdd(ScrewController screw, bool isTele = false, Action onComplete = null)
    {
        if (capacity <= 0)
        {
            Debug.LogError($"[BoxScrewStorage] capacity={capacity} — Initialize chưa được gọi?");
            return false;
        }

        if (IsFull)
        {
            Debug.LogWarning($"[BoxScrewStorage] IsFull: screws={screws.Count}, capacity={capacity}");
            return false;
        }

        if (screw.GetColor() != boxColor)
        {
            Debug.LogWarning($"[BoxScrewStorage] Color mismatch: screw={screw.GetColor()}, box={boxColor}");
            return false;
        }

        var emptySlot = holdSlots?.FirstOrDefault(h => h.IsEmpty());
        if (emptySlot == null)
        {
            Debug.LogWarning($"[BoxScrewStorage] Không còn slot trống — screws={screws.Count}, slots={holdSlots?.Count}");
            return false;
        }

        screws.Add(screw);

        // isTele = true → teleport ngay, không animate
        emptySlot.AddScrew(screw, isTele: isTele, callback: _ =>
        {
            Debug.Log($"[BoxScrewStorage] Screw added (isTele={isTele}) — {screws.Count}/{capacity}");
            onComplete?.Invoke();
        });

        return true;
    }
    /// <summary>
    /// Trả về world position của tất cả holdSlot đang có screw.
    /// Dùng để spawn star tại đúng vị trí từng screw trên box.
    /// </summary>
    public List<Vector3> GetOccupiedSlotWorldPositions()
    {
        var result = new List<Vector3>();
        if (holdSlots == null) return result;

        foreach (var hold in holdSlots)
        {
            if (!hold.IsEmpty())
                result.Add(hold.transform.position);
        }

        return result;
    }
    public void Clear()
    {
        screws.Clear();
        if (holdSlots == null) return;
        foreach (var hold in holdSlots)
            hold.RemoveScrew();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (holdSlots != null && holdSlots.Count != capacity && capacity > 0)
            Debug.LogWarning($"[BoxScrewStorage] holdSlots.Count ({holdSlots.Count}) != capacity ({capacity}) trên {gameObject.name}");
    }
#endif
}