using Enums;
using Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : SingletonMono<MissionManager>
{
    private MissionConfig missionConfig;

    // full mission list loaded from CSV
    private List<MissionConfigRecord> allMissions = new();

    // 3 missions shown on UI
    [SerializeField]
    private List<MissionConfigRecord> activeMissions = new();

    // missions waiting (not completed)
    private Queue<MissionConfigRecord> missionQueue = new();

    private const int MAX_ACTIVE = 3;

    // runtime progress (NOT written to DataModel)
    private Dictionary<int, MissionProgress> runtimeProgress = new();

    // Events fired by gameplay
    public static UnityEvent<ColorEnum,int> OnScrewCollected = new();
    public static UnityEvent OnBoxClosed = new();
    public static UnityEvent OnItemUsed = new();
    public static UnityEvent OnLevelCompleted = new();
    public static UnityEvent OnSecondTick = new();

    // events for UI
    public event Action<MissionConfigRecord> OnMissionUpdated;
    public event Action<MissionConfigRecord> OnMissionCompleted;

    private void OnEnable()
    {
        OnScrewCollected.AddListener((color,total)=> AddProgress(color,total));
        OnBoxClosed.AddListener(() => AddProgress());
        OnItemUsed.AddListener(() => AddProgress());
        OnLevelCompleted.AddListener(() => SaveAllMissionProgress());
        OnSecondTick.AddListener(() => AddProgress());
    }

    private void OnDisable()
    {
        OnScrewCollected.RemoveAllListeners();
        OnBoxClosed.RemoveAllListeners();
        OnItemUsed.RemoveAllListeners();
        OnLevelCompleted.RemoveAllListeners();
        OnSecondTick.RemoveAllListeners();
    }

    // ============================================================
    // INITIALIZATION
    // ============================================================

    public IEnumerator Init(Action callback = null)
    {
        missionConfig = ConfigFileManager.Instance.GetConfig<MissionConfig>();

        yield return new WaitUntil(() => missionConfig != null);

        allMissions = missionConfig.GetAllRecord();

        runtimeProgress.Clear();
        missionQueue.Clear();
        activeMissions.Clear();

        BuildMissionQueue();
        FillActiveMissions();

        callback?.Invoke();
    }

    private void BuildMissionQueue()
    {
        missionQueue.Clear();

        foreach (var mission in allMissions.OrderBy(m => m.Id))
        {
            var progress = DataAPIController.instance.GetMissionProgress(mission.Id);

            if (progress.state != MissionState.Completed)
                missionQueue.Enqueue(mission);
        }
    }

    private void FillActiveMissions()
    {
        activeMissions.Clear();

        while (activeMissions.Count < MAX_ACTIVE && missionQueue.Count > 0)
        {
            var next = missionQueue.Dequeue();
            activeMissions.Add(next);
        }

        Debug.Log($"[Mission] Active missions loaded: {activeMissions.Count}");
    }

    public List<MissionConfigRecord> GetActiveMissions() => activeMissions;

    // ============================================================
    // ADD PROGRESS
    // ============================================================

    public void AddProgress(ColorEnum color = ColorEnum.Clear,int total = 1)
    {


        Debug.Log("add progress mission ");
        foreach (var mission in activeMissions.ToList())
        {
            if (IsMissionCompleted(mission.Id))
                continue;

            switch (mission.MissionType)
            {
                case MissionType.CollectColor:
                    if (mission.Color == color)
                    {
                        IncreaseProgress(mission.Id, total);
                        Debug.Log("increase progress " + mission.Id);   
                    }
                    break;

                case MissionType.ClearNormalBoxes:
                    if (BoxQueue.ins.screwBoxes.Count == 0)
                        CompleteMissionForce(mission);
                    break;

                case MissionType.UseItem:
                    IncreaseProgress(mission.Id, total);
                    break;

                case MissionType.TimeSurvive:
                    IncreaseProgress(mission.Id, total);
                    break;

                case MissionType.ClearRainbowBox:
                    // sẽ gọi từ BoxQueue.OnRainbowBoxClosed
                    break;

                case MissionType.CompleteLevel:
                    // gọi khi level complete
                    break;

                case MissionType.ScoreReached:
                    // gọi khi gameplay đạt điểm
                    break;
            }

            OnMissionUpdated?.Invoke(mission);
        }
    }

    private void IncreaseProgress(int missionId, int amount)
    {
        var mission = allMissions.Find(m => m.Id == missionId);

        if (!runtimeProgress.TryGetValue(missionId, out var prog))
        {
            prog = DataAPIController.instance.GetMissionProgress(missionId);
            runtimeProgress[missionId] = prog;
        }

        prog.current += amount;

        if (prog.current >= mission.Target)
        {
            prog.current = mission.Target;
            prog.state = MissionState.Completed;
            HandleMissionCompleted(mission);
        }
    }

    private void CompleteMissionForce(MissionConfigRecord mission)
    {
        if (!runtimeProgress.TryGetValue(mission.Id, out var p))
        {
            p = DataAPIController.instance.GetMissionProgress(mission.Id);
            runtimeProgress[mission.Id] = p;
        }

        p.current = mission.Target;
        p.state = MissionState.Completed;

        HandleMissionCompleted(mission);
    }

    // ============================================================
    // HANDLE COMPLETE
    // ============================================================

    private void HandleMissionCompleted(MissionConfigRecord mission)
    {
        Debug.Log("[Mission] Completed: " + mission.Id);

        OnMissionCompleted?.Invoke(mission);
        activeMissions.Remove(mission);

        if (missionQueue.Count > 0)
        {
            var next = missionQueue.Dequeue();
            activeMissions.Add(next);
        }
    }

    public bool IsMissionCompleted(int missionId)
    {
        var p = runtimeProgress.ContainsKey(missionId)
            ? runtimeProgress[missionId]
            : DataAPIController.instance.GetMissionProgress(missionId);

        return p.state == MissionState.Completed;
    }

    // ============================================================
    // SPECIAL: RAINBOW BOX
    // ============================================================

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

    // ============================================================
    // SAVE ON LEVEL COMPLETE
    // ============================================================

    private void SaveAllMissionProgress()
    {
        Debug.Log("[Mission] Saving progress on level complete...");

        foreach (var kv in runtimeProgress)
        {
            DataAPIController.instance.UpdateMissionProgress(kv.Value);
        }

        runtimeProgress.Clear();

        BuildMissionQueue();
        FillActiveMissions();
    }

    // ============================================================
    // GET PROGRESS
    // ============================================================

    public int GetProgress(int missionId)
    {
        if (runtimeProgress.TryGetValue(missionId, out var p))
            return p.current;

        return DataAPIController.instance.GetMissionProgress(missionId)?.current ?? 0;
    }
}
