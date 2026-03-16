using LevelSystem.Core;
using Managers;
using System.Collections;
using System.DataBase;
using UnityEngine;

/// <summary>
/// Step 7: Tạo Side Mission cho level hiện tại.
///
/// Flow:
///  1. Skip nếu là tutorial level (new player)
///  2. Check daily quota — skip nếu hết
///  3. GenerateColorMission → chọn màu, remove box khỏi BoxQueue
///  4. RecolorMissionScrews → đổi screw target sang Rainbow
///  5. EnableSpecialMode trên BoxQueue để SpecialBox UI hiển thị
///  6. ShowMissionDialog → thông báo cho player
///  7. Set totalStarInLevel trên IngameController
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

        int starCount = level.screws.Count != 0 ? level.screws.Count : 1;
        Debug.Log($"[InitSpecialMissionStep] Stars count in level: {starCount}");

        // ── Skip tutorial level ────────────────────────────────────
        // Tutorial chạy khi IsNewPlayer — side mission sẽ conflict
        // với tutorial flow (highlight, block input, etc.)
        if (IsTutorialLevel(ctx))
        {
            Debug.Log("[InitSpecialMissionStep] Tutorial level detected — skip side mission.");
            SetTotalStar(starCount, missionBonus: 0);
            yield break;
        }

        // ── Skip nếu thiếu screw ───────────────────────────────────
        if (level.screws == null || level.screws.Count < 3)
        {
            Debug.Log("[InitSpecialMissionStep] Not enough screws — skip mission.");
            SetTotalStar(starCount, missionBonus: 0);
            yield break;
        }

        // ── Generate mission (daily check inside) ──────────────────
        var mission = _sideMissionManager.GenerateColorMission(level);

        if (mission == null)
        {
            Debug.Log("[InitSpecialMissionStep] No valid mission generated — skip.");
            SetTotalStar(starCount, missionBonus: 0);
            yield break;
        }

        // ── Recolor screw target → Rainbow ─────────────────────────
        if (ctx.ScrewManager != null)
        {
            _sideMissionManager.RecolorMissionScrews(ctx.ScrewManager, mission.requiredCount);
        }
        else
        {
            Debug.LogWarning("[InitSpecialMissionStep] ScrewManager null — cannot recolor screws.");
        }

        // ── Enable SpecialBox mode trên BoxQueue ───────────────────
        if (_boxQueue is BoxQueue concreteQueue)
            concreteQueue.EnableSpecialMode(mission);

        // ── Show mission dialog ────────────────────────────────────
        _sideMissionManager.ShowMissionDialog();

        // ── Set totalStarInLevel ───────────────────────────────────
        SetTotalStar(starCount, missionBonus: 1);

        Debug.Log($"[InitSpecialMissionStep] Mission ready: " +
                  $"colorId={mission.targetColorID} required={mission.requiredCount} " +
                  $"(screws recolored to Rainbow)");

        yield return null;
    }

    // ─── Helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Kiểm tra level hiện tại có phải tutorial không.
    /// Tutorial = new player HOẶC level có screw với tutorialKey.
    /// </summary>
    private bool IsTutorialLevel(LevelContext ctx)
    {
        // 1. New player → LevelStartService sẽ trigger TutorialManager
        if (DataAPIController.instance != null && DataAPIController.instance.IsNewPlayer())
            return true;

        // 2. Level có screw gắn tutorialKey → là tutorial level
        if (ctx.LevelData?.screws != null)
        {
            foreach (var screw in ctx.LevelData.screws)
            {
                if (screw != null && !string.IsNullOrEmpty(screw.tutorialKey))
                    return true;
            }
        }

        return false;
    }

    private void SetTotalStar(int boxCount, int missionBonus)
    {
        if (_ingameController == null) return;
        _ingameController.SetTotalStar(boxCount + missionBonus);
    }
}