using System.Collections.Generic;
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
    [SerializeField] private Button chestBtn;

    private List<PackMiniItem> items;
    private QuestChestParam param;

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
        chestBtn.onClick.AddListener(OnQuestChestClick);
    }

    private void OnDisable()
    {
        chestBtn.onClick.RemoveAllListeners();
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

            Sprite sprite = SpriteLibControl.Instance
                .GetSpriteByName(reward.icon_name);

            item.Init(reward.itemType, reward.amount, sprite,false);
            item.gameObject.SetActive(true);
        }
    }

    // =====================================================
    // CLICK
    // =====================================================
    public void OnQuestChestClick()
    {
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

}

public class QuestChestParam
{
    public int chestId;

    public string icon;

    public float progress;
    public float target;

    public bool isUnlocked;
    public bool isClaimed;

    public List<RewardItem> rewards;
}

