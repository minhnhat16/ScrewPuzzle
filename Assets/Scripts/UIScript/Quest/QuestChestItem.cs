using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86;

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
    [SerializeField] private SkeletonGraphic ske;
    [SerializeField]
    private List<PackMiniItem> items;
    [SerializeField]
    private QuestChestParam param;

    private Animator animController;
    private Action animCallBack;

    
    // Tween reference so we can stop/kill it safely
    private Tween shakeTween;
    public int ChestId => param.chestId;
    // =====================================================
    // LIFECYCLE
    // =====================================================
    internal virtual void Awake()
    {
        items = GetComponentsInChildren<PackMiniItem>(true).ToList();
        animController = GetComponent<Animator>();

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
        // ensure toggle cleaned up
        tgle.isOn = false;
        tgle.onValueChanged.RemoveAllListeners();
    }

    // =====================================================
    // SETUP
    // =====================================================
    public virtual void Setup(QuestChestParam param)
    {
        this.param = param;
        SetProgress(param.progress, param.target);
        SetUnlocked(param.isUnlocked);
        SetClaimed(param.isClaimed);
        tgle.group = param.toggleGroup;
        LoadItemDetail(param.rewards);
        Debug.Log("Setup chest id: " + param.chestId + " param null: " + param ==null);
    }

    public void SetClaimed(bool claimed)
    {
        param.isClaimed = claimed;

        if (doneIcon != null)
            doneIcon.SetActive(claimed);
            ske.AnimationState.SetAnimation(0,"idle",true); 
        if (claimed)
            StopCanReward();
    }

    public void SetProgress(float progress, float target)
    {
        param.progress = progress;
        param.target = target;

        if (progressFill == null) return;

        float percent = target > 0 ? (float)progress / target : 0f;
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

            Sprite sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, reward.icon_name);

            item.Init(reward.itemType, reward.amount, sprite, false);
            item.gameObject.SetActive(true);
        }
    }

    // =====================================================
    // CLICK
    // =====================================================
    public void OnQuestChestClick(bool clicked)
    {
        Debug.Log("On quest clickedd");
        if (!clicked)
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
    internal void SetChestUnlocked(bool isUnlocked)
    {
        lockIcon.SetActive(!isUnlocked);
        progressFill.gameObject.SetActive(!isUnlocked);
        PlayUnlockAnim();
    }
    // =====================================================
    // UI ACTIONS
    // =====================================================
    public void ShowDetail()
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
        param.isClaimed = true;
        SetChestClaimed(param.isClaimed);
    }

    private void SetChestClaimed(bool isClaim)
    {
        doneIcon.SetActive(isClaim);
        lockIcon.SetActive(false);
        tgle.isOn = false;
        progressFill.gameObject.SetActive(false);
        DataAPIController.instance.SetChestClaimed(param.chestId,true);
        if (isClaim) PlayCanReward();
        else StopCanReward();
            // stop any reward animation when claimed
            StopCanReward();
    }

    internal void SelectToggle()
    {
        tgle.isOn = true;
    }

    // Start an infinite shake safely: we create a short shake tween and loop it.
    public void PlayCanReward()
    {
        // Kill existing tween if present
        ske.AnimationState.SetAnimation(0, "idle", true);
        progressFill.gameObject.SetActive(false);   
        // Create a 0.8s shake and loop it infinitely. This avoids using Mathf.Infinity as duration.
     
    }

    // Stop and kill the shake tween safely
    public void StopCanReward()
    {
        ske.AnimationState.SetAnimation(0, "open_idle", true);
    }

    public void SetUnlocked(bool unlocked)
    {
        param.isUnlocked = unlocked;

        // progress
        if (progressFill != null)
            progressFill.gameObject.SetActive(!unlocked);

        // lock icon
        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        // color
        if (ske != null)
            ske.color = unlocked ? Color.white : GameConstants.FadeColor;

        // reward FX
        if (unlocked && !param.isClaimed)
            PlayCanReward();
        else
            StopCanReward();
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

