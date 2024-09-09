using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Dealer : MonoBehaviour
{
    [SerializeField] private int upgradeLevel;
    [SerializeField] private int id;
    [SerializeField] private int rewardGold;
    [SerializeField] private int rewardGem;
    [SerializeField] private SlotStatus status;
    //[SerializeField] private bool isUnlocked;
    public float fillOffset;
    public float scaleValue;

    public Vector3 fixedPosition;
    public Slot dealSlot;
    public Image fillImg;
    public Text level_lb;
    public Text gold_reward;
    public Text gem_reward;
    public RectTransform dealerFill;
    public RectTransform r_rewardGold;
    public RectTransform r_rewardGem;
    public GoldGroupAnim goldGroup;
    public GemGroup gemGroup;
    public RectTransform dealerLevel;
    public Transform _anchorPoint;
    public Transform _anchorLevel;
    public Transform _anchorReward;
    public SpriteRenderer render;
    public UpgradeSlotButton upgrade_btn;

    [HideInInspector] public UnityEvent<int> isUpgraded = new();
    public bool isPlashCard;

    public int UpgradeLevel { get { return upgradeLevel; } set { upgradeLevel = value; } }

    public int RewardGold { get => rewardGold; set => rewardGold = value; }
    public int RewardGem { get => rewardGem; set => rewardGem = value; }
    //public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }
    public int Id { get => id; set => id = value; }
    public SlotStatus Status { get => status; set => status = value; }

    private void OnEnable()
    {
        isUpgraded = upgrade_btn.levelUpgraded;
        //dealSlot.onToucheHandle.AddListener(dealSlot.TapHandler);
        isUpgraded.AddListener(OnUpgradedDealer);
        StartCoroutine(DealerEvent());
     
        //upgrade_btn = GetComponentInChildren<UpgradeSlotButton>();
    }
    private void OnDisable()
    {
        isUpgraded.RemoveAllListeners();
        dealSlot.onToucheHandle.RemoveAllListeners();
        isUpgraded.RemoveListener(OnUpgradedDealer);
        DataTrigger.UnRegisterValueChange(DataPath.DEALERDICT + $"{id}", UpdateDealerReward);
    }
    
    private void UpdateDealerReward(object data)
    {
        Debug.LogWarning($"Is data null {data is null}");
        if (data == null) return;
        DealerData newData = (DealerData)data;
        if (newData.id == id)
        {
            //Debug.LogWarning($"Update Dealer Reward {id}");
            var dealerRec = ConfigFileManager.Instance.DealerPriceConfig.GetRecordByKeySearch(newData.upgradeLevel);
            if (dealerRec == null)
            {
                dealerRec = ConfigFileManager.Instance.DealerPriceConfig.GetRecordByKeySearch(22);
                upgrade_btn.SetSlotButton(dealerRec.Cost, dealerRec.CurrencyType);
                RewardGem = dealerRec.LevelGem;
                RewardGold = dealerRec.LevelGold;
                DataAPIController.instance.MinusWalletByType(upgrade_btn.Price, upgrade_btn.UpgradeType, (bool isDone) =>
                {
                    SoundManager.instance.PlaySFX(SoundManager.SFX.UpgradeSFX);
                });
                RewardUpdate();
            }
            else
            {
                upgrade_btn.SetSlotButton(dealerRec.Cost, dealerRec.CurrencyType);
                RewardGem = dealerRec.LevelGem;
                RewardGold = dealerRec.LevelGold;
                RewardUpdate();
            }
        }

    }
    public void Init()
    {
        ScreenToWorld.Instance.SetWorldToCanvas(upgrade_btn.Rect);
        var data = DataAPIController.instance.GetDealerData(Id);
        upgradeLevel = DataAPIController.instance.GetDealerLevelByID(Id);
        var dealerRec = ConfigFileManager.Instance.DealerPriceConfig.GetRecordByKeySearch(upgradeLevel);
        var slotRec = ConfigFileManager.Instance.SlotConfig.GetRecordByKeySearch(id);
        if (dealerRec != null)
        {
            RewardGem = dealerRec.LevelGem;
            RewardGold = dealerRec.LevelGold;
        }
        upgrade_btn.SetSlotButton(dealerRec.Cost, dealerRec.CurrencyType);
        status = data.status;
        dealSlot.status = status;


        if (status == SlotStatus.Locked)
        {
            
            SetFillActive(false);
            SetRewardActive(false);
            SetDealerLvelActive(false);
            dealSlot.SetSlotPrice(id, slotRec.Price, slotRec.Currency);
        }
        else if (status == SlotStatus.Active)
        {
            fillImg.color = ConfigFileManager.Instance.ColorConfig.GetRecordByKeySearch(dealSlot.TopColor()).Color;
            fillImg.fillAmount = dealSlot._cards.Count * 0.1f;
            dealSlot.CheckOnTouchEvent();
        }
        else
        {
            isUpgraded = upgrade_btn.levelUpgraded;
            dealSlot.SettingBuyBtn(status != SlotStatus.Active);
        }


        bool isActive = status != SlotStatus.InActive;
        gameObject.SetActive(isActive);
        SetRender();

        // Subscribe to the onToucheHandle event
        Debug.Log($"Dealer {Id} subscribed to onToucheHandle event");
    }
    IEnumerator Start()
    {
        //Debug.Log("Start Dealer" + id);
        DataTrigger.RegisterValueChange(DataPath.DEALERDICT + $"{id}", UpdateDealerReward);
        yield return new WaitUntil(() => ConfigFileManager.Instance.DealerPriceConfig != null);
        Init();
        level_lb.text = $"{UpgradeLevel}";
        RewardCourountine();
        InvokeRepeating(nameof(DealerUpdating), 0.01f, 0.05f);
    }
    public void DealerUpdating()
    {
        _anchorPoint.position = transform.position - new Vector3(0, 1.7f, 0);
        int cardCout = dealSlot._cards.Count;
        if (cardCout != 0) return;
        fillImg.fillAmount = Mathf.Lerp(fillImg.fillAmount, 0, 5f * Time.deltaTime);
        //RewardCourountine();
        if (fillImg.fillAmount < 0.01f)
        {
            fillImg.fillAmount = 0;
        }
        //if (SlotCamera.Instance == null) return;
        if (!SlotCamera.Instance.isScalingCamera) return;
        else
        {
            UpdateFillPostion();

        }
    }
    public void UpdateFillPostion()
    {
        //TODO: IF CAMERA CHANGED , Change fill positon
        //Debug.LogWarning("Update Fill Position");
        ScreenToWorld.Instance.SetWorldToCanvas(dealSlot.BuyBtn);
        ScreenToWorld.Instance.SetWorldToCanvas(dealerFill);
        ScreenToWorld.Instance.SetWorldToCanvas(dealerLevel);
        //Debug.LogError("is upgrade button active " + id + upgrade_btn.gameObject.activeInHierarchy);
        ScreenToWorld.Instance.SetWorldToCanvas(upgrade_btn);
        ScreenToWorld.Instance.SetWorldToCanvas(r_rewardGem);
        ScreenToWorld.Instance.SetWorldToCanvas(r_rewardGold);

        dealerLevel.transform.SetPositionAndRotation(_anchorLevel.position, Quaternion.identity);
        dealSlot.BuyBtn.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        dealerFill.transform.SetPositionAndRotation(_anchorPoint.position, Quaternion.identity);
        upgrade_btn.transform.SetPositionAndRotation(_anchorPoint.position + new Vector3(0, -0.75f), Quaternion.identity);

        r_rewardGold.transform.SetPositionAndRotation(_anchorReward.position + new Vector3(0, 0.3f), Quaternion.identity);
        r_rewardGem.transform.SetPositionAndRotation(_anchorReward.position + new Vector3(0, -0.3f), Quaternion.identity);
    }
    public void SetRender()
    {
        switch (status)
        {
            case SlotStatus.Active:
                render.sprite = SpriteLibControl.Instance.GetSpriteByName($"dealer{dealSlot.status}");
                RewardUpdate();
                return;
            case SlotStatus.Locked:
                render.sprite = SpriteLibControl.Instance.GetSpriteByName($"Locked");
                dealSlot.SettingBuyBtn(true);
                UpdateFillPostion();
                return;
            case SlotStatus.InActive:
                SetDealerAndFillActive(false);
                return;
            default:
                return;

        }
    }
    public void RewardCourountine()
    {
        if (status != SlotStatus.Active) return;
        StartCoroutine(RewardUpdating());
    }
    IEnumerator RewardUpdating()
    {
        if (dealSlot.status != SlotStatus.Active)
        {
            SetRewardActive(false);
            yield return null;
        }
        yield return new WaitUntil(() => dealSlot._cards.Count == 0);
        SetRewardActive(dealSlot._cards.Count == 0);
        //yield return new WaitUntil(() => dealSlot._cards.Count != 0);
        //SetRewardActive(true);

    }
    public void SetRewardActive(bool isActive)
    {
        gold_reward.text = GameManager.instance.DevideCurrency(rewardGold);
        gem_reward.text = GameManager.instance.DevideCurrency(rewardGem);
        r_rewardGold.gameObject.SetActive(isActive);
        r_rewardGem.gameObject.SetActive(isActive);
        //if (rewardGem <= 0 || isActive) r_rewardGem.gameObject.SetActive(false);
        if (rewardGem <= 0) r_rewardGem.gameObject.SetActive(false);
        //else r_rewardGem.gameObject.SetActive(isActive);
    }
    public void RewardUpdate()
    {
        gold_reward.text = GameManager.instance.DevideCurrency(rewardGold);
        gem_reward.text = GameManager.instance.DevideCurrency(rewardGem);

        bool isSlotActive = dealSlot.status == SlotStatus.Active;
        r_rewardGold.gameObject.SetActive(isSlotActive);
        if (rewardGem <= 0 && isSlotActive) r_rewardGem.gameObject.SetActive(false);
        else r_rewardGem.gameObject.SetActive(true);

    }
    public void SetFillActive(bool isActive)
    {
        dealerFill.gameObject.SetActive(isActive);
    }
    public void SetUpgradeButtonActive(bool isActive)
    {
        upgrade_btn.gameObject.SetActive(isActive);
        Debug.Log("SetUpgradeButtonActive" + upgrade_btn.gameObject.activeInHierarchy);
    }
    public void SetDealerLvelActive(bool isActive)
    {
        dealerLevel.gameObject.SetActive(isActive);
    }
    public void SetDealerAndFillActive(bool isActive)
    {
        gameObject.SetActive(isActive);
        SetFillActive(isActive);
        SetUpgradeButtonActive(isActive);
        SetRewardActive(isActive);
        SetFillAndBtnToCanvas();
    }
    public void SetFillAndBtnToCanvas()
    {
        ScreenToWorld.Instance.SetWorldToCanvas(dealerFill);
        ScreenToWorld.Instance.SetWorldToCanvas(upgrade_btn);
    }

    private void OnUpgradedDealer(int price)
    {
        if (price > 0)
        {
            //Debug.Log("OnUpgradedDealer" + id);
            DataAPIController.instance.MinusWalletByType(price, upgrade_btn.UpgradeType, (isDOne) =>
            {
                upgradeLevel++;
                bool isMaxLevel = UpgradeLevel > ConfigFileManager.Instance.DealerPriceConfig.GetAllRecord().Count;
                if (isMaxLevel) return;
                DataAPIController.instance.SetDealerLevel(Id, UpgradeLevel);
                level_lb.text = $"{UpgradeLevel}";
            });
            
        }
    }
    public void SetDealerSprite()
    {
        switch (dealSlot.status)
        {
            case SlotStatus.Active:

                render.sprite = SpriteLibControl.Instance.GetSpriteByName(dealSlot.status.ToString());
                break;
            case SlotStatus.InActive:
                dealSlot.SettingBuyBtn(false);
                render.sprite = SpriteLibControl.Instance.GetSpriteByName(dealSlot.status.ToString());
                break;
            case SlotStatus.Locked:
                dealSlot.SettingBuyBtn(true);
                gold_reward.gameObject.SetActive(false);
                gem_reward.gameObject.SetActive(false);
                render.sprite = SpriteLibControl.Instance.GetSpriteByName(dealSlot.status.ToString());
                break;
            default: break;
        }
    }
    public void UpdateCardStackPosition(Vector3 currentPos)
    {
        if (dealSlot._cards.Count < 0) return;
        float x = currentPos.x;
        Debug.Log("UpdateCardStackPosition");
        foreach (Card c in dealSlot._cards)
        {
            Vector3 pos = c.transform.position;
            c.transform.SetPositionAndRotation(new Vector3(x, pos.y, pos.z), Quaternion.identity);
        }
    }
    private void OnDestroy()
    {
        Destroy(goldGroup.gameObject);
        Destroy(gemGroup.gameObject);
    }
    IEnumerator DealerEvent()
    {
        yield return new WaitUntil(() => IngameController.instance != null);
        dealSlot.expChanged = IngameController.instance.onExpChange;
    }
}