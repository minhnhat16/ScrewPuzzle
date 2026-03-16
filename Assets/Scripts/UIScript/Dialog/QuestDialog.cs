using DG.Tweening;
using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestDialog : BaseDialog
{
    [Header("UI Containers")]
    [SerializeField] private Transform chestTrackParent;
    [SerializeField] private Transform stageTabParent;

    [Header("Mission Panels")]
    [SerializeField] private RectTransform missionPanelCurrent;
    [SerializeField] private RectTransform missionPanelNext;

    [Header("Prefabs")]
    [SerializeField] private GameObject chestItemPrefab;
    [SerializeField] private GameObject stageTabPrefab;
    [SerializeField] private GameObject specialChestPrefab;


    [SerializeField] private Button outsideClickCatcher;

    private QuestChestItem currentOpenChest;
    private readonly List<QuestItem> activeMissionItems = new();
    private readonly Dictionary<int, List<MissionConfigRecord>> cachedStageMissions = new();
    private readonly Dictionary<int, QuestChestItem> chestItems = new();

    private int currentStage;
    [SerializeField]
    private Transform topChestParent;
    [SerializeField]
    private Transform normalChestParent;



    public override void OnInit(Action callback = null)
    {
        base.OnInit(callback);
        SetupChestTrack();

    }

    // =====================================================
    // LIFECYCLE
    // =====================================================
    private void OnEnable()
    {
        StageEvents.OnChestProgressChanged += OnChestProgressChanged;
        StageEvents.OnStageUnlocked += OnStageUnlocked;
        MissionEvents.OnActiveMissionChanged += OnActiveMissionChanged;
        MissionEvents.OnMissionCompleted += OnMissionCompleted;
        MissionEvents.OnMissionClaimed += OnMissionClaimed;
        QuestDialogEvents.OnChestDetailOpened += OnChestDetailOpened;

        ChestEvent.OnChestUnlock += OnChestUnlocked;

        outsideClickCatcher.onClick.AddListener(OnOutsideClicked);
    }


    private void OnDisable()
    {
        StageEvents.OnChestProgressChanged -= OnChestProgressChanged;
        StageEvents.OnStageUnlocked -= OnStageUnlocked;
        MissionEvents.OnActiveMissionChanged -= OnActiveMissionChanged;
        MissionEvents.OnMissionCompleted -= OnMissionCompleted;
        ChestEvent.OnChestUnlock -= OnChestUnlocked;
        MissionEvents.OnMissionClaimed -= OnMissionClaimed;

        QuestDialogEvents.OnChestDetailOpened -= OnChestDetailOpened;
        outsideClickCatcher.onClick.RemoveAllListeners();

    }

    // =====================================================
    // SETUP
    // =====================================================
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);

        currentStage = 0;


        SetupStageTabs();
        RefreshStageTabs();


        CacheAllStageMissions();

        LoadMissions_WithSlide(currentStage, instant: false);
    }

    // =====================================================
    // EVENTS
    // =====================================================
    private void OnChestProgressChanged(int stageId, float progress, int required)
    {
        if (stageId != currentStage)
            return;
        if (chestItems.TryGetValue(stageId, out var chestItem))
        {
            chestItem.ApplyState();
            chestItem.SetProgress(progress, required);

        }
    }

    private void OnStageUnlocked(int stageId)
    {
        // rebuild tabs to reflect new unlocked state
        SetupStageTabs();
        RefreshStageTabs();
    }

    private void OnActiveMissionChanged(List<MissionConfigRecord> missions)
    {

        //LoadMissions_WithSlide(currentStage, instant: false);
    }

    private void OnMissionCompleted(MissionConfigRecord record)
    {

    }

     private void OnMissionClaimed(MissionConfigRecord record)
    {
        // When a mission is claimed update the chest progress UI for the current stage
        int stageId = DataAPIController.instance.GetCurrentStage();
        var stageProgress = DataAPIController.instance.GetStageProgress(stageId);
        if (stageProgress == null)
            return;

        var chestConfig = ConfigFileManager.Instance.GetConfig<ChestConfig>();
        var chest = chestConfig?.GetRecordByKeySearch(stageId)
                    ?? chestConfig?.GetAllRecord().FirstOrDefault(c => c.Id == stageId);

        int required = chest != null ? chest.RequiredProgress : 0;

        if (chestItems.TryGetValue(stageId, out var chestItem))
        {
            int progress = chestItem.Progress + 1;
            chestItem.SetProgress(progress, required);
        }

        SoundHelper.PlaySFX(SoundManager.SFX.MissionComplete);
    }

    // =====================================================
    // STAGE TABS
    // =====================================================
    private void SetupStageTabs()
    {
        ClearChildren(stageTabParent);

        int stageCount = ConfigExtensions.GetStageCount();
        for (int i = 0; i < stageCount; i++)
        {
            CreateStageTab(i);
        }
    }

    private void CreateStageTab(int index)
    {
        var obj = Instantiate(stageTabPrefab, stageTabParent);
        var tab = obj.GetComponent<QuestStageTab>();

        bool unlocked = DataAPIController.instance.CheckStageUnlocked(index);

        // Select only the tab that matches currentStage
        tab.Setup(
            index,
            isSelected: index == currentStage,
            isUnlocked: unlocked,
            onClickCallback: OnStageTabClicked
        );

    }
    private void OnStageTabClicked(int stageIndex)
    {


        Debug.Log("On stage tab clicked " + stageIndex + "and current index " + currentStage);
        if (stageIndex == currentStage)
            return;

        currentStage = stageIndex;

        RefreshStageTabs();
        //SetupChestTrack();
        LoadMissions_WithSlide(stageIndex, instant: false);
    }

    private void RefreshStageTabs()
    {
        foreach (Transform child in stageTabParent)
        {
            var tab = child.GetComponent<QuestStageTab>();
            tab.SetSelected(tab.StageIndex == currentStage);
        }
    }

    // =====================================================
    // CHEST TRACK
    // =====================================================
    private void SetupChestTrack()
    {
        ClearChildren(chestTrackParent);
        chestItems.Clear();

        var chestConfig = ConfigFileManager.Instance.GetConfig<ChestConfig>();
        var chests = chestConfig.GetAllRecord();

        for (int i = 0; i < chests.Count; i++)
        {
            bool isMaxTier = i == chests.Count - 1;
            CreateChestItem(chests[i], isMaxTier);
        }
    }
    private void CreateChestItem(ChestRecord chest, bool isMaxtier = false)
    {
        Transform parent = isMaxtier
            ? topChestParent
            : normalChestParent;

        GameObject prefab = isMaxtier
            ? specialChestPrefab
            : chestItemPrefab;

        var obj = Instantiate(prefab, parent);

        QuestChestItem item = obj.GetComponent<QuestChestItem>();
        if (item == null)
        {
            Debug.LogError($"[Quest] Chest item missing QuestChestItem, id = {chest.Id}");
            return;
        }

        var state = DataAPIController.instance.GetChestState(chest.Id);

        obj.transform.localScale = isMaxtier
            ? Vector3.one * 2f
            : Vector3.one;

        item.Setup(new QuestChestParam
        {
            chestId = chest.Id,
            isClaimed = state.isClaimed,
            isUnlocked = state.isUnlocked,
            progress = state.progress,
            target = chest.RequiredProgress,
            rewards = chest.Rewards,
        });

        chestItems[chest.Id] = item;

        //Debug.Log($"[Quest] Create chest id={chest.Id} special={isMaxtier}");
    }

    private void OnChestUnlocked(int chestId)
    {
        if (!chestItems.TryGetValue(chestId, out var item))
            return;
        item.SetChestUnlocked(true);  // FX / anim
    }


    // =====================================================
    // MISSIONS
    // =====================================================
    private void LoadMissions_WithSlide(int stageIndex, bool instant)
    {
        var missions = cachedStageMissions.TryGetValue(stageIndex, out var list)
        ? list
        : new List<MissionConfigRecord>();
        MissionManager.ins.FillActiveMission(missions);

        string ids = string.Join(", ", missions.Select(m => m.Id));


        Debug.Log($"[Mission] mission load to slide {stageIndex}: {ids}");



        FillMissionPanel(missionPanelNext, missions);

        if (instant)
        {
            missionPanelCurrent.anchoredPosition = Vector2.zero;
            missionPanelNext.anchoredPosition = new Vector2(2000, 0);
            SwapPanels();
            return;
        }

        missionPanelNext.anchoredPosition = new Vector2(800, 0);

        UISliding.ins.SlidePanel(
            missionPanelCurrent,
            new Vector2(-800, 0),
            missionPanelNext,
            Vector2.zero,
            .25f,
            Ease.OutCubic,
            SwapPanels
        );
    }

    private void FillMissionPanel(RectTransform panel, List<MissionConfigRecord> missions)
    {
        ClearPanel(panel);
        activeMissionItems.Clear();

        foreach (var mission in missions)
        {
            QuestItem item = QuestItemPool.ins.Spawn();
            item.transform.SetParent(panel, false);

            int progress = MissionManager.ins.GetProgress(mission.Id);
            item.Setup(mission, progress);

            activeMissionItems.Add(item);
        }
    }

    private void SwapPanels()
    {
        ClearPanel(missionPanelCurrent);

        var temp = missionPanelCurrent;
        missionPanelCurrent = missionPanelNext;
        missionPanelNext = temp;
    }

    private void ClearPanel(RectTransform panel)
    {
        for (int i = 0; i < panel.childCount; i++)
        {
            var item = panel.GetChild(i).GetComponent<QuestItem>();
            if (item != null)
                QuestItemPool.ins.Return(item);
        }
    }

    // =====================================================
    // CACHE
    // =====================================================
    private void CacheAllStageMissions()
    {
        cachedStageMissions.Clear();

        var questConfig = ConfigFileManager.Instance.GetConfig<QuestConfig>();
        if (questConfig == null)
            return;

        var records = questConfig.GetAllRecord();

        Debug.Log("[Mission] Caching all stage missions..." + records.Count);
        foreach (var stage in records)
        {

            if (cachedStageMissions.ContainsKey(stage.Id)) continue;


            cachedStageMissions[stage.Id] =
                ConfigExtensions.GetMissionsByID(
                    ConfigFileManager.Instance,
                    stage.MissionIds
                );


            string ids = string.Join(", ", cachedStageMissions[stage.Id].Select(m => m.Id));


            Debug.Log($"[Mission] mission load to slide {stage.Id}: {ids}");
        }
    }

    // =====================================================
    // DEBUG TOOLS (GIỮ – CHUẨN HOÁ)
    // =====================================================
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
            DebugCompleteAllActiveMissions();

        if (Input.GetKeyDown(KeyCode.F7))
            DebugUnlockNextStage();

        if (Input.GetKeyDown(KeyCode.F8))
            DebugUnlockAllStages();

        if (Input.GetKeyDown(KeyCode.F9))
            DebugCompleteFirstActiveMission();

        if (Input.GetKeyDown(KeyCode.F10))
            DebugIncrementFirstActiveMission();
    }

    private void DebugCompleteFirstActiveMission()
    {
        var missions = MissionManager.ins.ActiveMissions;
        if (missions == null || missions.Count == 0)
        {
            Debug.Log("<color=orange>[DEBUG][Mission] No active missions</color>");
            return;
        }

        var mission = missions[0];

        string idsString = string.Join(",", missions.Select(m => m.Id));
        Debug.Log($"<color=orange>[DEBUG][Mission]</color> mission stage {currentStage}, with id missions: {idsString}");

        // ⚠️ Id ở đây là DATA ID (MissionConfigRecord.Id)
        MissionManager.ins.ForceCompleteMissionDebug(mission.Id);
    }


    private void DebugCompleteAllActiveMissions()
    {
        var activeMIsssionS = MissionManager.ins.ActiveMissions.ToList();

        string idsString = string.Join(",", activeMIsssionS.Select(m => m.Id));
        Debug.Log($"<color=orange>[DEBUG][Mission]</color> mission stage {currentStage}, with id missions: {idsString}");
        foreach (var mission in MissionManager.ins.ActiveMissions.ToList())
        {
            MissionManager.ins.ForceCompleteMissionDebug(mission.Id);
        }
    }

    private void DebugIncrementFirstActiveMission()
    {
        var missions = cachedStageMissions[currentStage];

        string ids = string.Join(", ", missions.Select(m => m.Id));
        Debug.Log($"[DEBUG] Active mission IDs: {ids}, current stage {ids}");
        if (missions.Count == 0) return;

        MissionManager.ins.AddProgressToMission(missions[0].Id, 1);
    }

    private void DebugUnlockNextStage()
    {
        int stage = DataAPIController.instance.GetCurrentStage();
        DataAPIController.instance.UnlockStage(stage + 1);
        DataAPIController.instance.UnlockChest(stage);
        StageEvents.OnStageUnlocked?.Invoke(stage + 1);
    }

    private void DebugUnlockAllStages()
    {
        int count = ConfigExtensions.GetStageCount();
        for (int i = 0; i < count; i++)
        {
            DataAPIController.instance.UnlockStage(i);
            StageEvents.OnStageUnlocked?.Invoke(i);
            ChestEvent.OnChestUnlock?.Invoke(i);

        }
    }
#endif

    // =====================================================
    // UTILS
    // =====================================================
    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }


    private void OnChestDetailOpened(QuestChestItem item)
    {
        // đóng cái cũ
        if (currentOpenChest != null && currentOpenChest != item)
            currentOpenChest.HideDetail();

        currentOpenChest = item;
        outsideClickCatcher.gameObject.SetActive(true);
    }
  
    private void OnOutsideClicked()
    {
        if (currentOpenChest != null)
        {
            currentOpenChest.HideDetail();
            currentOpenChest = null;
        }

        outsideClickCatcher.gameObject.SetActive(false);
    }

}

public static class QuestDialogEvents
{
    public static Action<QuestChestItem> OnChestDetailOpened;
}