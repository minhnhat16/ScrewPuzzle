using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestChestItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image chestIcon;
    [SerializeField] private ProgressBar progressFill;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject doneIcon;

    [Header("Detail Preview")]
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private CanvasGroup detailCanvasGroup;

    [Header("Button")]
    [SerializeField] private Toggle tgle;

    [Header("Spine")]
    [SerializeField] private SkeletonGraphic ske;

    [Header("Reward Items")]
    [SerializeField] private List<PackMiniItem> items;

    [SerializeField] private QuestChestParam param;

    public int ChestId => param.chestId;
    public int Progress => (int)param.progress;
    // =====================================================
    // LIFECYCLE
    // =====================================================
    public virtual void Awake()
    {
        items = GetComponentsInChildren<PackMiniItem>(true).ToList();

        if (ske == null)
            ske = GetComponentInChildren<SkeletonGraphic>(true);

        if (detailRoot != null)
            detailRoot.SetActive(false);
    }

    private void OnEnable()
    {
        tgle.onValueChanged.AddListener(OnQuestChestClick);
    }

    private void OnDisable()
    {
        tgle.isOn = false;
        tgle.onValueChanged.RemoveListener(OnQuestChestClick);
    }

    // =====================================================
    // SETUP
    // =====================================================
    public virtual void Setup(QuestChestParam param)
    {
        this.param = param;

        tgle.group = param.toggleGroup;

        LoadItemDetail(param.rewards);
        ApplyState();
        SetProgress(param.progress, param.target);

    }

    // =====================================================
    // STATE HANDLER (CORE)
    // =====================================================
    internal void ApplyState()
    {
        // an toàn null
        if (progressFill != null) progressFill.gameObject.SetActive(false);
        if (lockIcon != null) lockIcon.SetActive(false);
        if (doneIcon != null) doneIcon.SetActive(false);

        Debug.Log(
            $"[QuestChestItem] ApplyState → chestId: {param.chestId}, isClaimed: {param.isClaimed}, isUnlocked: {param.isUnlocked}, progress: {param.progress}, target: {param.target}"
        );

        if (param.isClaimed)
        {
            doneIcon?.SetActive(true);
            lockIcon?.SetActive(false);
            progressFill?.gameObject.SetActive(false);

            StopCanReward();
            if (ske != null) ske.AnimationState.SetAnimation(0, "open_idle", true);
            return;
        }

        if (param.isUnlocked)
        {
            doneIcon?.SetActive(false);
            lockIcon?.SetActive(false);
            progressFill?.gameObject.SetActive(false); 
            PlayCanReward(); // anim/FX để nhận biết có thể claim
            if (ske != null) ske.color = Color.white;
            return;
        }

        bool hasTarget = param.target > 0f;
        bool isOnProgress =
                    !param.isUnlocked &&
                    param.progress > 0 &&
                    param.progress < param.target;

        if (isOnProgress)
        {
            lockIcon?.SetActive(false);
            progressFill?.gameObject.SetActive(true); // ✅ bật fill 
            PlayOnProgress();
            if (ske != null) ske.color = Color.white;
            StopCanReward();
            return;
        }

   
        lockIcon?.SetActive(true);             
        progressFill?.gameObject.SetActive(false);
        StopCanReward();
        if (ske != null) ske.color = GameConstants.FadeColor;
    }

    // =====================================================
    // PROGRESS
    // =====================================================
    public void SetProgress(float progress, float target)
    {
        param.progress = progress;
        param.target = target;

        if (progressFill == null) return;

        float percent = target > 0 ? progress / target : 0f;


        Debug.Log("Progress percent: " + percent + " target " +target);
        progressFill.SetProgress(Mathf.Clamp01(percent));
    }

    // =====================================================
    // REWARD PREVIEW
    // =====================================================
    private void LoadItemDetail(List<RewardItem> rewards)
    {
        if (rewards == null || rewards.Count == 0 || items == null)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            if (i >= rewards.Count)
            {
                items[i].gameObject.SetActive(false);
                continue;
            }

            var reward = rewards[i];
            var item = items[i];

            Sprite sprite = SpriteLibControl.Instance.GetSprite(
                0,
                SpriteGroup.UI,
                reward.icon_name
            );

            item.Init(reward.itemType, reward.amount, sprite, false);
            item.gameObject.SetActive(true);
        }
    }

    // =====================================================
    // CLICK
    // =====================================================
    private void OnQuestChestClick(bool clicked)
    {
        if (!clicked )
        {
            HideDetail();
            return;
        }

        if (param.isClaimed)
            return;

        if (!param.isUnlocked)
        {
            ShowDetail();
            QuestDialogEvents.OnChestDetailOpened?.Invoke(this);
            return;
        }

        // unlocked → claim
        ShowClaimDialog();
    }

    // =====================================================
    // UI ACTIONS
    // =====================================================
    private void ShowDetail()
    {
        if (detailRoot == null || detailCanvasGroup == null)
            return;

        detailRoot.SetActive(true);
        detailCanvasGroup.alpha = 1f;
        detailCanvasGroup.blocksRaycasts = true;
        detailCanvasGroup.interactable = true;
    }

    public void HideDetail()
    {
        if (detailRoot == null || detailCanvasGroup == null)
            return;

        detailCanvasGroup.alpha = 0f;
        detailCanvasGroup.blocksRaycasts = false;
        detailCanvasGroup.interactable = false;
        detailRoot.SetActive(false);
    }

    private void ShowClaimDialog()
    {
        DialogManager.ins.ShowDialog(
            DialogIndex.GiftClaimDialog,
            new GiftParam(param)
        );

        SetChestClaimed(true);
    }

    // =====================================================
    // STATE UPDATE API
    // =====================================================
    public void SetChestUnlocked(bool unlocked)
    {
        param.isUnlocked = unlocked;
        ApplyState();
    }

    private void SetChestClaimed(bool claimed)
    {
        param.isClaimed = claimed;
        DataAPIController.instance.SetChestClaimed(param.chestId, true);

        tgle.isOn = false;
        ApplyState();
    }

    public void SelectToggle()
    {
        tgle.isOn = true;
    }

    // =====================================================
    // ANIMATION
    // =====================================================
    private void PlayCanReward()
    {
        if (ske == null) return;

        ske.color = Color.white;
        ske.AnimationState.SetAnimation(0, "open_idle", true);

    }

    private void StopCanReward()
    {
        if (ske == null) return;
        ske.AnimationState.SetAnimation(0, "idle", true);
    }
    internal void PlayOnProgress()
    {
        ske.AnimationState.SetAnimation(0, "idle", true);

    }
    internal void PlayUnlockAnim()
    {
    }
}

[Serializable]
public class QuestChestParam
{
    public int chestId;
    public string icon;

    public float progress;
    public float target;

    public bool isUnlocked;
    public bool isClaimed;

    public ToggleGroup toggleGroup;
    public List<RewardItem> rewards;
}
