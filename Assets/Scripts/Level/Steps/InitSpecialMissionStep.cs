using LevelSystem.Core;
using Managers;
using System.Collections;
using UnityEngine;

/// <summary>
/// Step 7: Tạo Side Mission cho level hiện tại.
///
/// Flow:
///  1. Skip nếu không có boxConfig hoặc thiếu screw
///  2. GenerateColorMission → chọn màu, remove box khỏi BoxQueue
///  3. EnableSpecialMode trên BoxQueue để SpecialBox UI hiển thị
///  4. Set totalStarInLevel trên IngameController
/// </summary>
public class InitSpecialMissionStep : ILevelLoadStep
{
    public string StepName => "Init Special Mission";

    private readonly IngameController _ingameController;
    private readonly SideMissionManager _sideMissionManager;
    private readonly ILevelBoxQueue _boxQueue;

    public InitSpecialMissionStep(
        IngameController ingameController,
        SideMissionManager sideMissionManager,
        ILevelBoxQueue boxQueue)
    {
        _ingameController = ingameController;
        _sideMissionManager = sideMissionManager;
        _boxQueue = boxQueue;
    }

    public IEnumerator Execute(LevelContext ctx)
    {
        var level = ctx.LevelData;

        // ── Guard ──────────────────────────────────────────────────
        if (level == null)
        {
            Debug.LogWarning("[InitSpecialMissionStep] LevelData null — skip.");
            yield break;
        }

        // Tính số box records → dùng cho totalStarInLevel
        int boxCount = level.boxConfig?.records?.Count ?? 0;

        // ── Skip nếu thiếu screw ───────────────────────────────────
        if (level.screws == null || level.screws.Count < 3)
        {
            Debug.Log("[InitSpecialMissionStep] Not enough screws — skip mission.");
            SetTotalStar(boxCount, missionBonus: 0);
            yield break;
        }

        // ── Generate mission ───────────────────────────────────────
        // GenerateColorMission xử lý nội bộ:
        //   - Đếm screw theo màu
        //   - Chọn random màu đủ điều kiện (>= require)
        //   - Tự gọi boxQueue.RemoveBoxByColor để xóa box màu đó
        //   - Trả về SideMission hoặc null nếu không hợp lệ
        var mission = _sideMissionManager.GenerateColorMission(level, require: 3);

        if (mission == null)
        {
            Debug.Log("[InitSpecialMissionStep] No valid mission generated — skip.");
            SetTotalStar(boxCount, missionBonus: 0);
            yield break;
        }

        // ── Enable SpecialBox mode trên BoxQueue ───────────────────
        // EnableSpecialMode chỉ có trên BoxQueue concrete
        if (_boxQueue is BoxQueue concreteQueue)
            concreteQueue.EnableSpecialMode(mission);

        // ── Set totalStarInLevel ───────────────────────────────────
        // boxCount star từ box + 1 bonus star nếu có mission
        SetTotalStar(boxCount, missionBonus: 1);

        Debug.Log($"[InitSpecialMissionStep] Mission ready: " +
                  $"colorId={mission.targetColorID} required={mission.requiredCount}");

        yield return null;
    }

    // ─── Helper ────────────────────────────────────────────────────

    private void SetTotalStar(int boxCount, int missionBonus)
    {
        if (_ingameController == null) return;
        _ingameController.SetTotalStar(boxCount + missionBonus);
    }
}