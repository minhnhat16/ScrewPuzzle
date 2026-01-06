


using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProgressMultipleTargetParam
{
    public float CurrentProgress;
    public float TargetProgress;
    public float Duration;
    public List<int> TargetIndices = new List<int>();
    public Action Callback;
    public ProgressMultipleTargetParam(float targetProgress, float duration = 1f, Action callback = null)
    {
        TargetProgress = targetProgress;
        Duration = duration;
        Callback = callback;
    }
}
public class ProgressBarMultipleTarget: ProgressBar
{
    [SerializeField]
    private RectTransform chestTrackParent;
    public GameObject prefab;
    ProgressMultipleTargetParam param;
    private ToggleGroup toggleGroup;

    List<QuestChestItem> chestItems = new List<QuestChestItem>();

    public HashSet<int> unlockedStageIndices = new HashSet<int>();

    public override void Awake()
    {
        base.Awake();   
        toggleGroup = chestTrackParent.GetComponent<ToggleGroup>();
    }
    public void Init(ProgressMultipleTargetParam param)
    {
        this.param = param;
        var configs = ConfigFileManager.Instance.GetConfig<ChestConfig>();
        var chestsData = DataAPIController.instance.GetChestsData(ChestLocation.Puzzle);
        // map nhanh theo chestId để lookup O(1)
        Dictionary<int, ChestStageData> dataMap = new();

        if (chestsData != null)
        {
            foreach (var d in chestsData)
                dataMap[d.chestId] = d;
        }

        foreach (var record in configs.GetAllRecord())
        {
            dataMap.TryGetValue(record.Id, out var chestData);
            ChestInit(record, chestData);
        }

        float progressPerChest = this.param.CurrentProgress/ this.param.TargetProgress;    
        SetProgress(param.TargetProgress);
    }

    public override void SetProgress(float value)
    {
        base.SetProgress(value);
        CheckUnlockStages(value);
    }
    private void CheckUnlockStages(float progress)
    {
        if (chestItems == null || chestItems.Count == 0)
            return;

        for (int i = 0; i < chestItems.Count; i++)
        {
            if (unlockedStageIndices.Contains(i))
                continue;

            float threshold = (i + 1) / (float)chestItems.Count;

            if (progress >= threshold)
            {
                unlockedStageIndices.Add(i);
                OnStageUnlocked(i, chestItems[i]);
            }
        }
    }
    private void OnStageUnlocked(int index, QuestChestItem chest)
    {
        Debug.Log($"[Progress] Stage {index} unlocked (Chest {chest.ChestId})");

        // 1️⃣ Update chest state
        chest.SetChestUnlocked(true);

        // 2️⃣ Optional: auto-select toggle
        chest.SelectToggle();

        // 3️⃣ Save state
        DataAPIController.instance.CheckStageUnlocked(chest.ChestId);

        // 4️⃣ Callback (nếu có)
        param?.Callback?.Invoke();
    }

    public void ChestInit(ChestRecord record, ChestStageData data)
    {
        // ===============================
        // GUARD: tránh spawn trùng
        // ===============================
        if (chestItems.Any(c => c.ChestId == record.Id))
        {
            Debug.LogWarning($"[ChestInit] Chest {record.Id} already exists");
            return;
        }

        // ===============================
        // INSTANTIATE
        // ===============================
        var obj = Instantiate(prefab, chestTrackParent);
        var item = obj.GetComponent<QuestChestItem>();

        // ===============================
        // LOAD STATE
        // ===============================
        var state = DataAPIController.instance.GetChestState(record.Id);

        // ===============================
        // SETUP PARAM
        // ===============================
        item.Setup(new QuestChestParam
        {
            chestId = record.Id,
            icon = ChestTierHelper.GetSpriteName(record.Tier),
            isClaimed = state.isClaimed,
            isUnlocked = state.isUnlocked,
            progress = state.progress,
            rewards = record.Rewards,
            toggleGroup = toggleGroup,
        });

        chestItems.Add(item);

        Debug.Log(
            $"[ChestInit] id={record.Id}, unlocked={state.isUnlocked}, claimed={state.isClaimed}, progress={state.progress}"
        );
    }

   
}
