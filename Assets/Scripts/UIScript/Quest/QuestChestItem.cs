using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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

    private List<PackMiniItem> items;
    private QuestChestParam param;


    public int ChestId => param.chestId;
    // =====================================================
    // LIFECYCLE
    // =====================================================
    private void Awake()
    {
        items = GetComponentsInChildren<PackMiniItem>(true).ToList();

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
        tgle.onValueChanged.RemoveAllListeners();

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


        tgle.group = param.toggleGroup;
        // rewards preview
        LoadItemDetail(param.rewards);
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
    }

    internal void SelectToggle()
    {
       tgle.isOn = true;
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

