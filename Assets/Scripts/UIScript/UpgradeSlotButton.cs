using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeSlotButton : MonoBehaviour
{
    [SerializeField] private int price;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Currency upgradeType;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Text lb_priceUpgrade;
    [HideInInspector] public UnityEvent<int> levelUpgraded = new();
    [SerializeField] public Image[] images;

    public RectTransform Rect { get => rect; set => rect = value; }
    public int Price { get => price; set => price = value; }
    public Currency UpgradeType { get => upgradeType; set => upgradeType = value; }

    private void OnEnable()
    {
        upgradeButton.onClick.AddListener(OnClickUpgradeButton);
    }
    private void OnDisable()
    {
        upgradeButton.onClick.RemoveListener(OnClickUpgradeButton);
    }
    public void Start()
    {
        //Debug.Log("Upgrade SLot button Start");
        rect = GetComponent<RectTransform>();
        lb_priceUpgrade = GetComponentInChildren<Text>();

    }
    public void SetSlotButton(int price, Currency upgradeType)
    {
        //Debug.Log($"SetSlot Button {price} & {upgradeType}");
        this.price = price;
        this.upgradeType = upgradeType;
        lb_priceUpgrade.text = price.ToString();
        
        if(upgradeType == Currency.Gem)
        {
            SetImage(false);
        }
        else SetImage(true);
    }
    public void SetImage(bool isActive)
    {
        images[0].gameObject.SetActive(isActive);
        images[1].gameObject.SetActive(!isActive);
    }
    private void OnClickUpgradeButton()
    {
        //Debug.Log("ON CLICK UPGRADE BUTTON");
        CurrencyWallet wallet = DataAPIController.instance.GetWalletByType(upgradeType);
        if (price > wallet.amount || GameManager.instance.IsNewPlayer) return;
         levelUpgraded?.Invoke(price);
     
    }

}