using Core.Match;
using Enums;
using Ingame;
using Ingame.Screw;
using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;

public class SideMissionManager : SingletonMono<SideMissionManager>, IResetable
{
    // ─────────────────────────────────────────
    // Config (Inspector)
    // ─────────────────────────────────────────

    [Header("Daily Limit")]
    [Tooltip("Số side mission tối đa mỗi ngày. 0 = không giới hạn.")]
    [SerializeField][Min(0)] private int maxDailyMissions = 5;

    [Tooltip("Số screw cần collect cho mỗi mission.")]
    [SerializeField][Min(1)] private int screwsPerMission = 3;

    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    public SideMission CurrentMission { get; private set; }
    public bool HasActiveMission => CurrentMission != null;
    public int MaxDailyMissions => maxDailyMissions;

    // ─────────────────────────────────────────
    // Injected
    // ─────────────────────────────────────────

    private IContainerQueue _containerQueue;

    /// <summary>
    /// Gọi từ ScrewGameBootstrapper sau khi BoxQueue được khởi tạo.
    /// </summary>
    public void Inject(IContainerQueue containerQueue)
    {
        _containerQueue = containerQueue;
        Debug.Log("[SideMissionManager] Injected IContainerQueue successfully.");
    }

    // ─────────────────────────────────────────
    // Daily Tracking
    // ─────────────────────────────────────────

    /// <summary>
    /// Kiểm tra và reset daily counter nếu sang ngày mới (UTC).
    /// Gọi từ InitSpecialMissionStep trước GenerateColorMission.
    /// </summary>
    public void CheckAndResetDaily()
    {
        var data = GetDailyData();
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        if (data.lastResetDate != today)
        {
            data.completedToday = 0;
            data.lastResetDate = today;
            SaveDailyData(data);
            Debug.Log("[SideMissionManager] Daily reset — new day detected.");
        }
    }

    /// <summary>
    /// Còn quota mission hôm nay không?
    /// </summary>
    public bool HasDailyQuota()
    {
        if (maxDailyMissions <= 0) return true; // 0 = unlimited
        var data = GetDailyData();
        return data.completedToday < maxDailyMissions;
    }

    /// <summary>
    /// Số mission còn lại hôm nay.
    /// </summary>
    public int RemainingToday()
    {
        if (maxDailyMissions <= 0) return int.MaxValue;
        var data = GetDailyData();
        return Mathf.Max(0, maxDailyMissions - data.completedToday);
    }

    /// <summary>
    /// Tăng counter sau khi mission hoàn thành.
    /// Gọi từ SpecialBoxManager hoặc khi mission IsCompleted == true.
    /// </summary>
    public void RecordMissionCompleted()
    {
        var data = GetDailyData();
        data.completedToday++;
        SaveDailyData(data);
        Debug.Log($"[SideMissionManager] Mission completed today: {data.completedToday}/{maxDailyMissions}");
    }

    // ─────────────────────────────────────────
    // Mission Generation
    // ─────────────────────────────────────────

    /// <summary>
    /// Tạo side mission khi level load xong.
    /// Chọn random 1 màu có đủ screw, xoá box màu đó khỏi queue,
    /// và đổi screw target sang Rainbow để route vào SpecialBoxManager.
    /// 
    /// Trả null nếu:
    /// - Daily quota hết
    /// - Không đủ screw
    /// - _containerQueue chưa inject
    /// </summary>
    public SideMission GenerateColorMission(Level.Level level, int require = -1)
    {
        if (require < 0) require = screwsPerMission;

        // ── Daily check ────────────────────────────────────────────
        CheckAndResetDaily();

        if (!HasDailyQuota())
        {
            Debug.Log($"[SideMissionManager] Daily quota reached ({maxDailyMissions}). No mission generated.");
            return null;
        }

        // ── Guard: injection ───────────────────────────────────────
        if (_containerQueue == null)
        {
            Debug.LogError("[SideMissionManager] _containerQueue not injected. " +
                "Ensure ScrewGameBootstrapper.Inject() is called before GenerateColorMission().");
            return null;
        }

        // ── Đếm screw theo màu (bỏ qua Rainbow đã có sẵn) ────────
        var colorCount = level.screws
            .Where(s => (ColorEnum)s.idColor != ColorEnum.Rainbow)
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

        int targetColorID = validColors[UnityEngine.Random.Range(0, validColors.Count)];

        CurrentMission = new SideMission
        {
            targetColorID = targetColorID,
            requiredCount = require,
            currentCount = 0
        };

        // Remove box of this color from queue
        if (_containerQueue is BoxQueue boxQueue)
        {
            int removeCount = require / 3;
            int removed = boxQueue.RemoveFromSequenceByColor((ColorEnum)targetColorID, removeCount);
            Debug.Log($"[SideMissionManager] Removed {removed}/{removeCount} box(es) " +
                      $"of color {(ColorEnum)targetColorID} from sequence");
        }

        Debug.Log($"[SideMissionManager] Mission created: collect {require} screws " +
                  $"of color {(ColorEnum)targetColorID} | Remaining today: {RemainingToday() - 1}");
        return CurrentMission;
    }

    // ─────────────────────────────────────────
    // Recolor Screws → Rainbow
    // ─────────────────────────────────────────

    /// <summary>
    /// Đổi screw target sang Rainbow.
    /// Gọi từ InitSpecialMissionStep SAU khi screws đã spawn.
    /// </summary>
    public void RecolorMissionScrews(ScrewManager screwManager, int count)
    {
        if (!HasActiveMission || screwManager == null) return;

        var targetColor = (ColorEnum)CurrentMission.targetColorID;

        var candidates = screwManager.Screws
            .Where(s => s != null && s.GetColor() == targetColor)
            .OrderBy(_ => UnityEngine.Random.value)
            .Take(count)
            .ToList();

        foreach (var screw in candidates)
        {
            screw.ChangeScrewColor(ColorEnum.Rainbow);
        }

        Debug.Log($"[SideMissionManager] Recolored {candidates.Count}/{count} " +
                  $"screw(s) from {targetColor} → Rainbow.");
    }

    // ─────────────────────────────────────────
    // Progress
    // ─────────────────────────────────────────

    /// <summary>
    /// Gọi từ IngameController khi có screw rainbow được collect.
    /// Tự record daily completion khi mission hoàn thành.
    /// </summary>
    public void UpdateMission(int count)
    {
        if (!HasActiveMission) return;

        bool wasDone = CurrentMission.IsCompleted;
        CurrentMission.currentCount += count;

        // Vừa hoàn thành → record vào daily counter
        if (!wasDone && CurrentMission.IsCompleted)
        {
            RecordMissionCompleted();
            Debug.Log("[SideMissionManager] ✅ Side mission completed!");
        }
    }

    // ─────────────────────────────────────────
    // Dialog
    // ─────────────────────────────────────────

    /// <summary>
    /// Hiện MissionDialog thông báo side mission cho player.
    /// Dùng MissionParam — class param chính thức cho MissionDialog.
    /// </summary>
    public void ShowMissionDialog()
    {
        if (!HasActiveMission) return;

        var param = new MissionParam
        {
            totalGold = WalletManager.ins.Get(Currency.Gold),
            totalTicket = WalletManager.ins.Get(Currency.Ticket),
            current = CurrentMission.currentCount,
            target = CurrentMission.requiredCount,
            SideMission = CurrentMission
        };

        DialogManager.ins.ShowDialog(DialogIndex.MissionDialog, param);
        Debug.Log($"[SideMissionManager] Showing MissionDialog: " +
                  $"color={(ColorEnum)CurrentMission.targetColorID}, " +
                  $"progress={CurrentMission.currentCount}/{CurrentMission.requiredCount}, " +
                  $"remaining today={RemainingToday()}");
    }

    // ─────────────────────────────────────────
    // Data Persistence Helpers
    // ─────────────────────────────────────────

    private SideMissionDailyData GetDailyData()
    {
        var data = DataAPIController.instance.ReadSideMissionDaily();
        if (data == null)
        {
            data = new SideMissionDailyData
            {
                completedToday = 0,

                lastResetDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
            };
            SaveDailyData(data);
        }
        return data;
    }

    private void SaveDailyData(SideMissionDailyData data)
    {
        DataAPIController.instance.SaveSideMissionDaily(data);
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
// SideMission data class (runtime — not persisted)
// ─────────────────────────────────────────────────────────────────

public class SideMission
{
    public int targetColorID;
    public int requiredCount;
    public int currentCount;
    public bool IsCompleted => currentCount >= requiredCount;
    public float Progress => requiredCount > 0 ? (float)currentCount / requiredCount : 0f;
}