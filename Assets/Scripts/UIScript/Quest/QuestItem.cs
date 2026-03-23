using Managers;
using System;
using System.DataBase;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private Text titleText;
    [SerializeField] private Text progressText;
    [SerializeField] private ProgressBar fill;
    [SerializeField] private Button actionButton;
    [SerializeField] private GameObject doneIcon;

    private MissionConfigRecord missionData;
    private UnityAction cachedClick;
    [SerializeField] private Text totalText;
    public int MissionID { get; set; }

    // =======================================================
    // LIFECYCLE
    // =======================================================
    private void OnEnable()
    {
        cachedClick = OnButtonClicked;
        actionButton.onClick.AddListener(cachedClick);

        MissionEvents.OnMissionProgressChanged += OnMissionProgressChanged;
        MissionEvents.OnMissionCompleted += OnMissionCompleted;
        MissionEvents.OnMissionClaimed += OnMissionClaimed;
    }

    private void OnDisable()
    {
        actionButton.onClick.RemoveListener(cachedClick);

        MissionEvents.OnMissionProgressChanged -= OnMissionProgressChanged;
        MissionEvents.OnMissionCompleted -= OnMissionCompleted;
        MissionEvents.OnMissionClaimed -= OnMissionClaimed;
    }

    // =======================================================
    // SETUP
    // =======================================================
    public void Setup(MissionConfigRecord data, int currentProgress)
    {
        missionData = data;
        MissionID = data.Id;
        icon.sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, data.IconName);

        UpdateProgressUI(currentProgress, data.Target);

        var state = DataAPIController.instance
            .GetMissionProgress(data.Id).state;
        
        if(data.Description != null)
            titleText.text = data.Description;
        RefreshState(state);
    }

    private void OnMissionProgressChanged(MissionConfigRecord mission, MissionProgress progress)
    {

        Debug.Log($"<color=blue>[DEBUG] Completete mission  {MissionID} and data id {mission.Id}</color>");

        if (mission.Id != MissionID)
            return;

        UpdateProgressUI(progress.current, mission.Target);
    }

    private void OnMissionCompleted(MissionConfigRecord mission)
    {
        // so sánh DATA ID với DATA ID
        if (mission.Id != MissionID)
            return;

        Debug.Log($"<color=blue>[QuestItem] Mission completed: dataId={mission.Id}</color>");

        UpdateProgressUI(mission.Target, mission.Target);
        RefreshState(MissionState.Completed);
    }


    private void OnMissionClaimed(MissionConfigRecord mission)
    {
        if (mission.Id != MissionID)
            return;
        RefreshState(MissionState.Claimed);
    }

    // =======================================================
    // UI STATE
    // =======================================================
    private void UpdateProgressUI(int current, int target)
    {
        progressText.text = $"{current}/{target}";
        fill.SetProgress(Mathf.Clamp01((float)current / target));
    }

    private void RefreshState(MissionState state)
    {
        switch (state)
        {
            case MissionState.InProgress:
                doneIcon.SetActive(false);
                actionButton.interactable = true;
                actionButton.gameObject.SetActive(true);
                actionButton.GetComponentInChildren<Text>().text = "Play";
                break;

            case MissionState.Completed:
                doneIcon.SetActive(false);
                actionButton.interactable = true;
                actionButton.gameObject.SetActive(true);
                actionButton.GetComponentInChildren<Text>().text = "Claim";
                break;

            case MissionState.Claimed:
                // UI panel sẽ refresh list nên item này có thể disable
                actionButton.interactable = false;
                actionButton.gameObject.SetActive(false);
                doneIcon.SetActive(true);
                break;
        }
    }

    // =======================================================
    // BUTTON
    // =======================================================
    private void OnButtonClicked()
    {
        var progress = DataAPIController.instance.GetMissionProgress(MissionID);

        if (progress.state == MissionState.Completed)
        {
            Debug.Log("[QuestItem] Claim mission: " + MissionID);
            MissionManager.ins.ClaimMission(missionData);
            return;
        }

        if (progress.state == MissionState.InProgress)
        {
            Debug.Log("[QuestItem] Play mission: " + missionData.Description);

            DialogManager.ins.HideDialog(DialogIndex.QuestDialog, () =>
            {
                int lv = DataAPIController.instance.GetPlayerLevel();
                LevelManager.ins.LoadLevel(lv);
            });
        }
    }
}
