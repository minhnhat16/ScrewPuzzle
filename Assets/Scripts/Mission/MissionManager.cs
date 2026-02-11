using Enums;
using Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ConfigFile;
using System.DataBase;
using JetBrains.Annotations;

public class MissionManager : SingletonMono<MissionManager>
{
    private MissionConfig missionConfig;
    private List<MissionConfigRecord> allMissions = new();

    [SerializeField]
    private List<MissionConfigRecord> activeMissions = new();
    private const int MAX_ACTIVE = 3;

    private readonly Dictionary<int, MissionProgress> runtimeProgress = new();

    public List<MissionConfigRecord> ActiveMissions => activeMissions;

    public object OnScrewCollected { get; internal set; }


    //private void OnEnable()
    //{
    //    MissionEvents.OnMissionClaimed += MissionClaim();
    //}

    //private void OnDisable()
    //{
    //    MissionEvents.OnMissionClaimed -= MissionClaim();
    //}



    // ============================================================
    // INIT
    // ============================================================
    public IEnumerator Init(Action callback = null)
    {
        missionConfig = ConfigFileManager.Instance.GetConfig<MissionConfig>();
        yield return new WaitUntil(() => missionConfig != null);

        allMissions = missionConfig.GetAllRecord();
        runtimeProgress.Clear();
        activeMissions.Clear();

        callback?.Invoke();
    }

    // ============================================================
    // ACTIVE MISSIONS
    // ============================================================
    public void FillActiveMission (List<MissionConfigRecord> missions)
    {
        activeMissions = missions; 
    }

    private MissionConfigRecord GetRandomMission()
    {
        if (allMissions.Count == 0) return null;

        var tries = 10;
        while (tries-- > 0)
        {
            var rand = allMissions[UnityEngine.Random.Range(0, allMissions.Count)];
            var saved = DataAPIController.instance.GetMissionProgress(rand.Id);

            if (saved.state != MissionState.Claimed)
                return rand;
        }

        return null;
    }

    // ============================================================
    // PROGRESS
    // ============================================================
    public void AddProgress(ColorEnum color = ColorEnum.Clear, int amount = 1)
    {
        foreach (var mission in activeMissions.ToList())
        {
            if (!ShouldProcessMission(mission, color))
                continue;

            IncreaseProgress(mission.Id, amount);
        }
    }

    private bool ShouldProcessMission(MissionConfigRecord mission, ColorEnum color)
    {
        return mission.MissionType switch
        {
            MissionType.CollectColor => mission.Color == color,
            MissionType.ClearNormalBoxes => true,
            MissionType.UseItem => true,
            MissionType.TimeSurvive => true,
            MissionType.CompleteLevel => true,
            MissionType.ScoreReached => true,
            _ => false
        };
    }

    private void IncreaseProgress(int missionId, int amount)
    {
        var mission = allMissions.FirstOrDefault(m => m.Id == missionId);
        if (mission == null) return;

        if (!runtimeProgress.TryGetValue(missionId, out var prog))
        {
            prog = DataAPIController.instance.GetMissionProgress(missionId);
            runtimeProgress[missionId] = prog;
        }

        if (prog.state != MissionState.InProgress)
            return;

        prog.current = Mathf.Min(prog.current + amount, mission.Target);

        MissionEvents.OnMissionProgressChanged?.Invoke(mission, prog);

        if (prog.current >= mission.Target)
        {
            prog.state = MissionState.Completed;
            HandleMissionCompleted(mission);
        }
        else
        {
            DataAPIController.instance.UpdateMissionProgress(prog);
        }
    }


    public void ProcessUseItem(ItemType usedItem, int amount = 1)
    {
        foreach (var mission in activeMissions.ToList())
        {
            if (mission.MissionType != MissionType.UseItem)
                continue;

            // Nếu mission yêu cầu item cụ thể
            // if (mission.RequiredItemType != usedItem) continue;

            IncreaseProgress(mission.Id, amount);
        }
    }

    internal void ProcessCollectScrew(ColorEnum color, int amount)
    {
        if(color == ColorEnum.Rainbow)
        {
            DataAPIController.instance.AddSpecial(amount);
        }
        // Only update missions of type CollectColor that match the collected color
        foreach (var mission in activeMissions.ToList())
        {
            if (mission.MissionType != MissionType.CollectColor)
                continue;

            // If mission.Color == ColorEnum.Empty, treat as "any color"
            if (mission.Color == ColorEnum.Empty || mission.Color == color)
            {
                IncreaseProgress(mission.Id, amount);

                // Notify UI if not completed yet
                if (!IsMissionCompleted(mission.Id))
                    MissionEvents.OnMissionProgressChanged?.Invoke(mission, runtimeProgress[mission.Id]);
            }
        }
    }
    internal void ProcessLevelComplete()
    {
        Debug.Log("[Mission] ProcessLevelComplete: awarding CompleteLevel missions.");

        // Increment progress for missions that require completing levels.
        // Use a snapshot since ReplaceCompletedMission may modify activeMissions.
        foreach (var mission in activeMissions.ToList())
        {
            if (mission == null) continue;

            if (mission.MissionType == MissionType.CompleteLevel ||
                mission.MissionType == MissionType.CompleteSpecialLevel)
            {
                // Increase by 1 level completed
                IncreaseProgress(mission.Id, 1);
            }
        }
    }

    internal void ProcessBoxClosed(ColorEnum color, int v)
    {
        // Snapshot active missions because IncreaseProgress / ReplaceCompletedMission may modify the list.
        foreach (var mission in activeMissions.ToList())
        {
            if (mission == null) continue;

            // Handle box-related missions:
            // - ClearNormalBoxes: may require a specific color or accept any color (ColorEnum.Empty)
            // - ClearRainbowBox: target rainbow boxes (color == Rainbow)
            if (mission.MissionType == MissionType.ClearNormalBoxes ||
                mission.MissionType == MissionType.ClearRainbowBox)
            {
                bool match = mission.Color == ColorEnum.Empty    // accepts any color
                             || mission.Color == color          // matches specific color
                             || (mission.MissionType == MissionType.ClearRainbowBox && color == ColorEnum.Rainbow);

                if (match)
                {
                    IncreaseProgress(mission.Id, v);
                }
            }
        }
    }


    public IReadOnlyList<MissionConfigRecord> GetActiveMissions()
    {
        return activeMissions;
    }

    // ============================================================
    // COMPLETE / CLAIM
    // ============================================================
    private void HandleMissionCompleted(MissionConfigRecord mission)
    {
        DataAPIController.instance.UpdateMissionProgress(runtimeProgress[mission.Id]);
        
        MissionEvents.OnMissionCompleted?.Invoke(mission);
        UpdateChestProgressForCurrentStage();
        ReplaceCompletedMission(mission);
    }

    private void UpdateChestProgressForCurrentStage()
    {
        int stageId = DataAPIController.instance.GetCurrentStage();
        var stageProgress = DataAPIController.instance.GetStageProgress(stageId);

        if (stageProgress == null)
        {
            Debug.LogError("[Stage] StageProgress NULL");
            return;
        }

        stageProgress.chestProgress += 1;

        int required = GetChestTarget(stageId);

        if (stageProgress.chestProgress > required)
            stageProgress.chestProgress = required;

        DataAPIController.instance.UpdateStageProgress(stageProgress);

        Debug.Log(
            $"[Chest] Stage {stageId} progress: {stageProgress.chestProgress}/{required}"
        );

        // notify UI
        StageEvents.OnChestProgressChanged?.Invoke(
            stageId,
            stageProgress.chestProgress,
            required
        );
    }
    private int GetChestTarget(int stageId)
    {
        var chestConfig = ConfigFileManager.Instance.GetConfig<ChestConfig>();
        var chest = chestConfig.GetRecordByKeySearch(stageId);
        return chest.RequiredProgress;
    }

    public void ClaimMission(MissionConfigRecord mission)
    {
        if (mission == null)
            return;

        var progress = DataAPIController.instance.GetMissionProgress(mission.Id);

        if (progress.state != MissionState.Completed || progress.rewardClaimed)
            return;

        // ===== MARK CLAIMED =====
        progress.rewardClaimed = true;
        progress.state = MissionState.Claimed;
        DataAPIController.instance.UpdateMissionProgress(progress);

        GrantMissionReward(mission);
        MissionEvents.OnMissionClaimed?.Invoke(mission);

        // ===== STAGE CLAIM COUNT =====
        int stageId = DataAPIController.instance.GetCurrentStage();
        var stageProgress = DataAPIController.instance.GetStageProgress(stageId);

        stageProgress.claimedMissions++;
        DataAPIController.instance.UpdateStageProgress(stageProgress);

        Debug.Log(
            $"[Stage] Stage {stageId} claimed missions: {stageProgress.claimedMissions}/3"
        );

        // ===== UNLOCK STAGE + CHEST =====
        const int CLAIM_REQUIRED = 3;

        if (stageProgress.claimedMissions >= CLAIM_REQUIRED)
        {
            UnlockStageAndChest(stageId);
        }
    }
    private void UnlockStageAndChest(int stageId)
    {
        int nextStage = stageId + 1;

        // ===== UNLOCK STAGE =====
        DataAPIController.instance.UnlockStage(nextStage);
        DataAPIController.instance.CheckStageUnlocked(stageId);
        StageEvents.OnStageUnlocked?.Invoke(nextStage);
        Debug.Log($"<color=cyan>[Stage]</color> Unlocked stage {nextStage}");
        // ===== UNLOCK CHEST =====
        UnlockStageChest(stageId);
    }

    private void UnlockStageChest(int stageId)
    {
        var chestConfig = ConfigFileManager.Instance.GetConfig<ChestConfig>();
        if (chestConfig == null)
            return;

        // Giả sử mỗi stage có 1 chest
        var chest = chestConfig.GetAllRecord()
            .FirstOrDefault(c => c.Id == stageId);

        if (chest == null)
        {
            Debug.LogWarning($"[Chest] No chest found for stage {stageId}");
            return;
        }

        var chestState = DataAPIController.instance.GetChestState(chest.Id);
        if (chestState.isUnlocked)
            return;

        chestState.isUnlocked = true;
        DataAPIController.instance.UnlockChest(stageId );

        Debug.Log(
            $"<color=gold>[Chest]</color> Unlocked chest {chest.Id} for stage {stageId}"
        );

        // notify UI
        StageEvents.OnChestProgressChanged?.Invoke(
            stageId,
             chest.RequiredProgress,
            chest.RequiredProgress
        );
        
    }
    private void ReplaceCompletedMission(MissionConfigRecord completed)
    {
        activeMissions.Remove(completed);

        var newMission = GetRandomMission();
        if (newMission != null)
            activeMissions.Add(newMission);

        MissionEvents.OnActiveMissionChanged?.Invoke(activeMissions);
    }

    private void GrantMissionReward(MissionConfigRecord mission)
    {
        if (mission.RewardItemType == ItemType.Gold)
            WalletManager.ins.Add(Currency.Gold, mission.RewardAmount);

        DataAPIController.instance.AddItemTotal(mission.RewardItemType, 1);
    }

    // ============================================================
    // HELPERS
    // ============================================================
    public bool IsMissionCompleted(int missionId)
    {
        var p = runtimeProgress.TryGetValue(missionId, out var prog)
            ? prog
            : DataAPIController.instance.GetMissionProgress(missionId);

        return p.state == MissionState.Completed;
    }

    public int GetProgress(int missionId)
    {
        return runtimeProgress.TryGetValue(missionId, out var p)
            ? p.current
            : DataAPIController.instance.GetMissionProgress(missionId)?.current ?? 0;
    }
#if UNITY_EDITOR
    // =====================================================
    // DEBUG / FORCE HELPERS
    // =====================================================

    /// <summary>
    /// Force hoàn thành mission ngay lập tức (editor only)
    /// - set progress = target
    /// - set state = Completed
    /// - trigger full completion flow (events, chest, stage...)
    /// </summary>
    public void ForceCompleteMissionDebug(int missionId)
    {
        var mission = allMissions.FirstOrDefault(m => m.Id == missionId);
        if (mission == null)
        {
            Debug.LogWarning($"[Mission][DEBUG] Mission {missionId} not found");
            return;
        }

        // ensure runtime progress exists
        if (!runtimeProgress.TryGetValue(missionId, out var prog))
        {
            prog = DataAPIController.instance.GetMissionProgress(missionId);
            runtimeProgress[missionId] = prog;
        }

        // already completed?
        if (prog.state == MissionState.Completed || prog.state == MissionState.Claimed)
            return;

        prog.current = mission.Target;
        prog.state = MissionState.Completed;



        // reuse normal flow
        HandleMissionCompleted(mission);

        Debug.Log($"<color=lime>[DEBUG] Force completed mission {missionId}</color>");
    }

    /// <summary>
    /// Add progress thủ công cho mission (editor only)
    /// - nếu đủ target sẽ auto complete
    /// </summary>
    public void AddProgressToMission(int missionId, int amount)
    {
        var mission = activeMissions.FirstOrDefault(m => m.Id == missionId);


        Debug.Log($"[AddProgressToMission] Mission ids {string.Join("'", activeMissions.Select(m => m.Id))} and current mission id {missionId}");
        if (mission == null)
        {
            Debug.LogWarning($"[Mission][DEBUG] Mission {missionId} not found");
            return;
        }

        if (!runtimeProgress.TryGetValue(missionId, out var prog))
        {
            prog = DataAPIController.instance.GetMissionProgress(missionId);
            runtimeProgress[missionId] = prog;
        }

        if (prog.state == MissionState.Completed || prog.state == MissionState.Claimed)
            return;

        prog.current = Mathf.Min(prog.current + amount, mission.Target);

        if (prog.current >= mission.Target)
        {
            prog.current = mission.Target;
            prog.state = MissionState.Completed;

            HandleMissionCompleted(mission);
        }
        else
        {
            DataAPIController.instance.UpdateMissionProgress(prog);
            MissionEvents.OnMissionProgressChanged?.Invoke(mission, prog);
        }

        Debug.Log($"<color=cyan>[DEBUG] Mission {missionId} progress {prog.current}/{mission.Target}</color>");
    }
#endif

#if UNITY_EDITOR
    // =====================================================
    // DEBUG API (EDITOR ONLY)
    // =====================================================
#if UNITY_EDITOR
    public void Debug_CompleteCurrentStage()
    {
        int stage = DataAPIController.instance.GetCurrentStage();
        DataAPIController.instance.CompleteStage(stage);
        StageEvents.OnStageCompleted?.Invoke(stage);
    }
#endif
    public void Debug_ForceCompleteMission(int missionId)
    {
        ForceCompleteMissionDebug(missionId);
        Debug.Log($"[DEBUG][Mission] Force completed mission {missionId}");
    }

    public void Debug_AddProgress(int missionId, int amount)
    {
        AddProgressToMission(missionId, amount);
        Debug.Log($"[DEBUG][Mission] Add {amount} progress to mission {missionId}");
    }
#endif

    public bool IsMissionAvailable(SideMission mission)
    {
        var newPlayer = DataAPIController.instance.IsNewPlayer();
        return !newPlayer;
    }
}
