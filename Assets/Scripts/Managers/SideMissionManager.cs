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
        if (_containerQueue == null)
        {
            Debug.LogError("[SideMissionManager] _containerQueue chưa được inject.");
            return null;
        }

        // Đếm screw theo màu
        var colorCount = level.screws
            .GroupBy(s => s.idColor)
            .ToDictionary(g => g.Key, g => g.Count());

        // Chỉ chọn màu có đủ số screw yêu cầu
        var validColors = colorCount
            .Where(p => p.Value >= require)
            .Select(p => p.Key)
            .ToList();

        if (validColors.Count == 0)
        {
            Debug.Log("[SideMissionManager] Không có màu nào đủ screw cho side mission.");
            return null;
        }

        int targetColorID = validColors[Random.Range(0, validColors.Count)];

        CurrentMission = new SideMission
        {
            targetColorID = targetColorID,
            requiredCount = require,
            currentCount = 0
        };

        // Xoá box màu này khỏi queue — player phải gỡ vào special box thay vì box thường
        // BoxQueue vẫn expose RemoveBoxByColor qua concrete type — gọi qua event hoặc cast
        if (_containerQueue is BoxQueue boxQueue)
            boxQueue.RemoveBoxByColor((ColorEnum)targetColorID, require / 3);

        Debug.Log($"[SideMissionManager] Mission tạo: gỡ {require} screw màu {(ColorEnum)targetColorID}");
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