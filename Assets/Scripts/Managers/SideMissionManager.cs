using Core.Match;
using Enums;
using Ingame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SideMissionManager : SingletonMono<SideMissionManager>, IResetable
{
    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    public SideMission CurrentMission { get; private set; }
    public bool HasActiveMission => CurrentMission != null;

    // ─────────────────────────────────────────
    // Injected
    // ─────────────────────────────────────────

    private IContainerQueue _containerQueue;

    /// <summary>
    /// Gọi từ ScrewGameBootstrapper sau khi BoxQueue được khởi tạo.
    /// Thay thế FindAnyObjectByType trong Awake.
    /// </summary>
    public void Inject(IContainerQueue containerQueue)
    {
        _containerQueue = containerQueue;
        Debug.Log("[SideMissionManager] Injected IContainerQueue successfully.");
    }

    // ─────────────────────────────────────────
    // Mission Generation
    // ─────────────────────────────────────────

    /// <summary>
    /// Tạo side mission khi level load xong.
    /// Chọn random 1 màu có đủ screw, xoá box màu đó khỏi queue.
    /// </summary>
    public SideMission GenerateColorMission(Level.Level level, int require = 3)
    {
        // Guard: ensure injection happened
        if (_containerQueue == null)
        {
            Debug.LogError("[SideMissionManager] _containerQueue not injected. " +
                "Ensure ScrewGameBootstrapper.Inject() is called before GenerateColorMission().");
            return null;
        }

        // Đếm screw theo màu
        var colorCount = level.screws
            .GroupBy(s => s.idColor)
            .ToDictionary(g => g.Key, g => g.Count());

        if (colorCount.Count == 0)
        {
            Debug.Log("[SideMissionManager] No screws in level.");
            return null;
        }

        // Chỉ chọn màu có đủ số screw yêu cầu
        var validColors = colorCount
            .Where(p => p.Value >= require)
            .Select(p => p.Key)
            .ToList();

        if (validColors.Count == 0)
        {
            Debug.Log("[SideMissionManager] No color has enough screws for side mission.");
            return null;
        }

        int targetColorID = validColors[Random.Range(0, validColors.Count)];

        CurrentMission = new SideMission
        {
            targetColorID = targetColorID,
            requiredCount = require,
            currentCount = 0
        };

        // Remove box of this color from queue — player must unscrew into special box instead
        if (_containerQueue is BoxQueue boxQueue)
        {
            boxQueue.RemoveBoxByColor((ColorEnum)targetColorID, require / 3);
            Debug.Log($"[SideMissionManager] Removed {require / 3} box(es) of color {(ColorEnum)targetColorID}");
        }

        Debug.Log($"[SideMissionManager] Mission created: collect {require} screws of color {(ColorEnum)targetColorID}");
        return CurrentMission;
    }

    // ─────────────────────────────────────────
    // Progress
    // ─────────────────────────────────────────

    /// <summary>
    /// Gọi từ IngameController khi có screw rainbow được collect.
    /// Guard null để tránh crash khi không có mission active.
    /// </summary>
    public void UpdateMission(int count)
    {
        if (!HasActiveMission) return;
        CurrentMission.currentCount += count;
    }

    // ─────────────────────────────────────────
    // IResetable
    // ─────────────────────────────────────────

    public void OnReset()
    {
        CurrentMission = null;
    }
}

// ─────────────────────────────────────────────────────────────────
// SideMission data class
// ─────────────────────────────────────────────────────────────────

public class SideMission
{
    public int targetColorID;
    public int requiredCount;
    public int currentCount;

    public bool IsCompleted => currentCount >= requiredCount;
    public float Progress => requiredCount > 0 ? (float)currentCount / requiredCount : 0f;
}