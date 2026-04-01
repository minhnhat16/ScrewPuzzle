using Enums;
using Ingame;
using Ingame.Screw;
using PoolManager;
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
            return false;

        if (IsFull)
            return false;

        if (screw == null)
            return false;

        if (screw.GetColor() != boxColor)
            return false;

        if (screws.Contains(screw))
            return false;

        // Prevent duplicate: check if any hold slot already contains this screw
        if (holdSlots != null && holdSlots.Any(h => h.GetScrew() == screw))
            return false;

        var emptySlot = holdSlots?.FirstOrDefault(h => h.IsEmpty());
        if (emptySlot == null)
            return false;


        //Debug.Log("[BoxScrewStorage] Adding screw to box: " + screw.hingeController.BodyConnect.name);
        screws.Add(screw);

        emptySlot.AddScrew(screw, isTele: isTele, callback: _ =>
        {
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

    /// <summary>
    /// Activate all screws currently stored in this box (used when the box becomes active on screen).
    /// Ensures visual/physics state is restored so screws move with the box and respond correctly.
    /// </summary>
    public void ActivateAllScrews()
    {
        // Activate screws tracked in storage list
        for (int i = 0; i < screws.Count; i++)
        {
            var screw = screws[i];
            if (screw == null) continue;

            try
            {
                screw.SetActive(true);
                screw.EnableColliderAndRig(true); // enable collider / rigid behavior for box-held screws
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BoxScrewStorage] ActivateAllScrews: failed on screw {screw?.name}: {ex.Message}");
            }
        }

        // Also ensure any screw still referenced by holdSlots are active
        if (holdSlots != null)
        {
            foreach (var hold in holdSlots)
            {
                if (hold == null) continue;
                var s = hold.GetScrew();
                if (s == null) continue;
                if (!s.gameObject.activeSelf)
                {
                    s.SetActive(true);
                }
                // Replace this line in ActivateAllScrews():
                 s.ResetHoldState();
                // with this line:
                s.EnableColliderAndRig(true);
            }
        }
    }

    public void Clear()
    {
        var pool = ScrewPool.Instance;
        try
        {
            var uniqueScrews = new HashSet<ScrewController>(screws.Where(s => s != null));

            if (holdSlots != null)
            {
                foreach (var hold in holdSlots)
                {
                    if (hold == null) continue;
                    var hScrew = hold.GetScrew();
                    if (hScrew != null)
                        uniqueScrews.Add(hScrew);
                }
            }

            foreach (var screw in uniqueScrews)
            {
                try
                {
                    screw.OnReset();
                    screw.SetActive(false);
                    pool?.Pool.ReturnToPool(screw);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BoxScrewStorage] Failed to return screw to pool: {ex.Message}");
                }
            }

            if (holdSlots != null)
            {
                foreach (var hold in holdSlots)
                {
                    if (hold == null) continue;
                    hold.RemoveScrew();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BoxScrewStorage] Clear: unexpected error: {ex.Message}");
        }
        finally
        {
            screws.Clear();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (holdSlots != null && holdSlots.Count != capacity && capacity > 0)
            Debug.LogWarning($"[BoxScrewStorage] holdSlots.Count ({holdSlots.Count}) != capacity ({capacity}) trên {gameObject.name}");
    }
#endif
}
