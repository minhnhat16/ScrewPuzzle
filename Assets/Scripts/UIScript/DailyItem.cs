using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DailyItem : MonoBehaviour
{
    public GameObject CanBeClaimed;
    public GameObject Claimed;
    public GameObject CanNotClaimed;
    public Image itemImg;
    public Image tickImg;
    public int intAmount;
    public int day;
    public DailyReward itemName;
    public Text day_lb;
    public Text Amount_lb;
    public IEDailyType currentType;
    public Button daily_btn;
    public float rotationSpeedFactor = 1.0f; // Default speed factor
    [HideInInspector] public UnityEvent<bool> onClickDailyItem = new();
    [HideInInspector] public UnityEvent<bool> onItemClaim = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool> onRewardRemain = new UnityEvent<bool>();
    private bool hasExceeded180;

    private void OnEnable()
    {
        onRewardRemain.AddListener(DailyRemain);
    }

    private void OnDisable()
    {
        onItemClaim.RemoveAllListeners();
        //onRewardRemain.RemoveListener(DailyRemain);
    }
    private void Start()
    {
        //onRewardRemain.AddListener(DailyRemain);
    }

    public void Init(IEDailyType type, int amount, int day, string spriteName, DailyReward itemName)
    {
        SwitchType(type);
        SetAmountLb(amount);
        SetDayLB(day);
        SetItemImg(spriteName);
        SetItemNameType(itemName);
    }
    public void SetItemImg(string spriteName)
    {
        //Debug.Log(itemName);
        itemImg.sprite = SpriteLibControl.Instance.GetSpriteByName(spriteName);
    }
    public void SetItemNameType(DailyReward itemName)
    {
        this.itemName = itemName;
    }
    public void SetDayLB(int day)
    {
        //Debug.Log($"Set day lb {day}");
        this.day = day;
        day_lb.text = $"Day {day}";
    }
    public void SetAmountLb(int amount)
    {
        //Debug.Log($"Set amount lb {amount}");
        intAmount = amount;
        Amount_lb.text = amount.ToString();
    }
    public virtual void SwitchType(IEDailyType type)
    {
        currentType = type;
        daily_btn.enabled = true;
        switch (type)
        {
            case IEDailyType.Available:
                SetCanBeClaim();
                daily_btn.enabled = true;
                Debug.LogWarning("ondaily reward available");
                onRewardRemain?.Invoke(true);
                break;
            case IEDailyType.Unavailable:
                SetCantClaim();
                //daily_btn.gameObject.SetActive(false);
                break;
            case IEDailyType.Claimed:
                SetClaimed();
                daily_btn.enabled = false;
                Amount_lb.gameObject.SetActive(false);
                //itemImg.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }
    public void SetClaimed()
    {
        CanBeClaimed.SetActive(false);
        Claimed.SetActive(true);
        CanNotClaimed.SetActive(false);
        tickImg.gameObject.SetActive(true);

    }
    public void SetCanBeClaim()
    {
        CanBeClaimed.SetActive(true);
        Claimed.SetActive(false);
        CanNotClaimed.SetActive(false);
        tickImg.gameObject.SetActive(false);

    }
    public void SetCantClaim()
    {
        CanBeClaimed.SetActive(false);
        Claimed.SetActive(false);
        CanNotClaimed.SetActive(true);
        tickImg.gameObject.SetActive(false);
    }
    public void SwitchItemType(DailyReward item)
    {

        switch (item)
        {
            case DailyReward.Gold_S:
                DataAPIController.instance.AddGold(intAmount, null);
                break;
            case DailyReward.Gold_M:
                // Add logic for medium gold reward
                //Debug.Log("Reward: Medium Gold");
                DataAPIController.instance.AddGold(intAmount, null);
                break;
            case DailyReward.Bomb:
                //Debug.Log("Reward: Bomb");
                // Add logic for bomb reward
                DataAPIController.instance.AddItemTotal(ItemType.Bomb, intAmount);

                break;
            case DailyReward.Gold_L:
                //Debug.Log("Reward: Large Gold");
                // Add logic for large gold reward
                DataAPIController.instance.AddGold(intAmount, null);
                break;
            case DailyReward.Gem:
                //Debug.Log("Reward: Gem");
                // Add logic for gem reward
                DataAPIController.instance.AddGem(intAmount);
                break;
            case DailyReward.Magnet:
                //Debug.Log("Reward: Magnet");
                // Add logic for magnet reward
                DataAPIController.instance.AddItemTotal(ItemType.Magnet, intAmount);
                break;
            case DailyReward.Bonus:
                //Debug.Log("Reward: Bonus");
                // Add logic for bonus reward
                DataAPIController.instance.AddItemTotal(ItemType.Magnet, 10);
                DataAPIController.instance.AddItemTotal(ItemType.Magnet, 10);
                DataAPIController.instance.AddGold(1500, null);
                DataAPIController.instance.AddGem(20);
                break;
            default:
                //Debug.LogWarning("Unexpected reward type: " + item);
                break;
        }
    }
    public void ItemClaim(bool isClaim)
    {
        if (isClaim)
        {
            SwitchType(IEDailyType.Claimed);
            DataAPIController.instance.SetDailyData(day - 1, currentType);
            DataAPIController.instance.SetIsClaimTodayData(isClaim = true);
            DataAPIController.instance.SetTimeClaimItem(System.DateTime.Now);
            SwitchItemType(itemName);
        }
    }
    public void CheckItemAvailable()
    {
        //Debug.Log("On Click Daily Item");
        if (currentType == IEDailyType.Available)
        {
            //Debug.Log("On Click Daily Item" + IEDailyType.Available);
            onClickDailyItem?.Invoke(true);
        }
        else
        {
            //Debug.Log("On Click Daily Item" + IEDailyType.Unavailable);
            bool checkVidReward = ZenSDK.instance.IsVideoRewardReady();
            onClickDailyItem?.Invoke(checkVidReward);
        }
    }

    public void DailyRemain(bool isRemain)
    {

        if (isRemain && gameObject.activeInHierarchy)
        {
            Debug.LogWarning("daily remain and playing anim");
        }
        else
        {

            Debug.LogWarning("daily not  remain either playing anim");
        }

    }

    public virtual void OnClickDailyItem()
    {
        CheckItemAvailable();
    }
}