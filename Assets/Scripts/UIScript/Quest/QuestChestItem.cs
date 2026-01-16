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
    private List<PackMiniItem> items;
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

        // stop any running shake tween to avoid leaks/continued animation
        StopCanReward();
    }

    // =====================================================
    // SETUP
    // =====================================================
    public void Setup(QuestChestParam param)
    {
        this.param = param;

        // progress
        if (progressFill != null)
        {
            float percent = param.target > 0 ? param.progress / param.target : 0f;
            progressFill.SetProgress(Mathf.Clamp01(percent));
            progressFill.gameObject.SetActive(param.isUnlocked);
        }

        // state
        lockIcon.SetActive(!param.isUnlocked);
        doneIcon.SetActive(param.isClaimed);

        Color color = param.isUnlocked ? Color.white : GameConstants.FadeColor;
        if(color == null ) ske.color = Color.white;
        else ske.color = color;
        tgle.group = param.toggleGroup;
        // rewards preview
        LoadItemDetail(param.rewards);
        if (param.isUnlocked && !param.isClaimed) PlayCanReward();
        else StopCanReward();
    }

    // =====================================================
    // REWARD PREVIEW
    // =====================================================
    private void LoadItemDetail(List<RewardItem> rewards)
    {
        if (rewards == null || rewards.Count == 0)
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
        DataAPIController.instance.SetChestClaimed(ChestId,true);
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
        if (shakeTween != null && shakeTween.IsActive())
        {
            shakeTween.Kill();
            shakeTween = null;
        }

        progressFill.gameObject.SetActive(false);   
        // Create a 0.8s shake and loop it infinitely. This avoids using Mathf.Infinity as duration.
        shakeTween = transform
                .DOShakeRotation(0.8f, strength: 10f, vibrato: 10, randomness: 90f)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear)
                .SetId(this);
        shakeTween.OnUpdate(() => Debug.Log("Shaking chesst"));
    }

    // Stop and kill the shake tween safely
    public void StopCanReward()
    {
        if (shakeTween != null)
        {
            if (shakeTween.IsActive())
                shakeTween.Kill();
            shakeTween = null;

            // reset transform rotation to identity to avoid leftover rotation
            transform.localRotation = Quaternion.identity;
        }
    }
}

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

