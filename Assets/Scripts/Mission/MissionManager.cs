using Enums;
using Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MissionManager : SingletonMono<MissionManager>
{
    private MissionConfig missionConfig;

    // List chứa tất cả mission trong file CSV
    private List<MissionConfigRecord> allMissions = new();


    [SerializeField]
    // List 3 mission đang active
    private List<MissionConfigRecord> activeMissions = new();

    // Queue chứa các mission còn lại
    private Queue<MissionConfigRecord> missionQueue = new();

    public event Action<MissionConfigRecord> OnMissionUpdated;
    public event Action<MissionConfigRecord> OnMissionCompleted;

    private const int MAX_ACTIVE = 3;



    public IEnumerator Init(Action callback = null)
    {
        missionConfig = ConfigFileManager.Instance.GetConfig<MissionConfig>();
        yield return new WaitUntil(()=> missionConfig!= null);

        allMissions = missionConfig.GetAllRecord();

        Debug.Log("Mission config loaded." + allMissions.Count);

        BuildMissionQueue();
        FillActiveMissions();
        callback?.Invoke();
    }
    // =============================================================
    // LOAD 3 MISSION HIỆN TẠI
    // =============================================================

    private void BuildMissionQueue()
    {
        missionQueue.Clear();

        // Sắp xếp theo ID hoặc độ khó
        var ordered = allMissions.OrderBy(m => m.Id);

        foreach (var mission in ordered)
        {
            var progress = DataAPIController.instance.GetMissionProgress(mission.Id);

            // Đẩy vào queue nếu chưa completed
            if (progress.state != MissionState.Completed)
                missionQueue.Enqueue(mission);
        }
    }

    private void FillActiveMissions()
    {
        activeMissions.Clear();

        Debug.Log($"[Mission] Adding mission to active list. Active count: {activeMissions.Count}, Queue count: {missionQueue.Count}");

        while (activeMissions.Count < MAX_ACTIVE && missionQueue.Count > 0)
        {

            var next = missionQueue.Dequeue();
            activeMissions.Add(next);
        }

        Debug.Log($"[Mission] Active missions: {activeMissions.Count}");
    }

    public List<MissionConfigRecord> GetActiveMissions()
    {
        return activeMissions;
    }

    // =============================================================
    // UPDATE PROGRESS
    // =============================================================

    public void AddProgress(ColorEnum color = ColorEnum.Clear)
    {
        foreach (var mission in activeMissions.ToList())
        {
            if (IsMissionCompleted(mission.Id))
                continue;

            switch (mission.MissionType)
            {
                case MissionType.CollectColor:
                    if (mission.Color == color)
                        IncreaseProgress(mission.Id, 1);
                    break;

                case MissionType.ClearNormalBoxes:
                    if (BoxQueue.ins.screwBoxes.Count == 0)
                        CompleteMissionForce(mission);
                    break;

                case MissionType.UseItem:
                    IncreaseProgress(mission.Id, 1);
                    break;

                case MissionType.TimeSurvive:
                    break; // TimeController sẽ gọi riêng
            }

            OnMissionUpdated?.Invoke(mission);
        }
    }

    private void IncreaseProgress(int missionId, int amount)
    {
        var mission = allMissions.Find(m => m.Id == missionId);
        var progress = DataAPIController.instance.GetMissionProgress(missionId);

        progress.current += amount;

        if (progress.current >= mission.Target)
        {
            progress.current = mission.Target;
            DataAPIController.instance.CompleteMission(missionId);
            HandleMissionCompleted(mission);
        }


        DataAPIController.instance.UpdateMissionProgress(progress);
    }

    private void CompleteMissionForce(MissionConfigRecord mission)
    {
        DataAPIController.instance.CompleteMission(mission.Id);
        HandleMissionCompleted(mission);
    }

    // =============================================================
    // MISSION COMPLETE HANDLER
    // =============================================================

    private void HandleMissionCompleted(MissionConfigRecord mission)
    {
        OnMissionCompleted?.Invoke(mission);

        Debug.Log($"[Mission] Completed: {mission.Id}");

        // Remove khỏi active list
        activeMissions.Remove(mission);

        // Đẩy mission mới vào
        if (missionQueue.Count > 0)
        {
            var next = missionQueue.Dequeue();
            activeMissions.Add(next);

            Debug.Log($"[Mission] Added new mission: {next.Id}");
        }
    }

    public bool IsMissionCompleted(int missionId)
    {
        var p = DataAPIController.instance.GetMissionProgress(missionId);
        return p.state == MissionState.Completed;
    }

    // =============================================================
    // SPECIAL: RAINBOW BOX
    // =============================================================

    public void OnRainbowBoxClosed(Box box)
    {
        foreach (var mission in activeMissions.ToList())
        {
            if (mission.MissionType == MissionType.ClearRainbowBox && !IsMissionCompleted(mission.Id))
            {
                IncreaseProgress(mission.Id, 1);
            }
        }
    }

    // =============================================================
    // GET PROGRESS
    // =============================================================

    public int GetProgress(int missionId)
    {
        var progress = DataAPIController.instance.GetMissionProgress(missionId);
        return progress?.current ?? 0;
    }
}
