using UnityEngine;
using UnityEngine.UI;

public class ItemConfirmDialog : BaseDialog
{
    [SerializeField] private ItemType type;
    private bool isAds;
    [SerializeField] Text tutorial_lb;
    [SerializeField] Text price_lb;

    [SerializeField] Button ads;
    [SerializeField] Button buy;
    [SerializeField] int price;
    [SerializeField] GameObject bomb;
    [SerializeField] GameObject magnet;

    public Button Ads { get { return ads; } set { this.ads = value; } }
    public Button Confirm { get { return buy; } set { this.buy = value; } }
    private void OnEnable()
    {
        ads.onClick.AddListener(PlayAds);
        buy.onClick.AddListener(PurchaseItem);
    }
    private void OnDisable()
    {
        ads.onClick.RemoveListener(PlayAds);
        buy.onClick.RemoveListener(PurchaseItem);
    }
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        if (dialogParam != null)
        {
            ItemConfirmParam param = (ItemConfirmParam)dialogParam;
            type = param.type;
            isAds = param.isAds;
            ItemCase(type);
        }

    }
    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        price = ZenSDK.instance.GetConfigInt($"price+{type}", 3000);
        price = Mathf.RoundToInt(price * IngameController.instance.GetPlayerLevel() / 2);
        price_lb.text = price.ToString();
        Player.Instance.isAnimPlaying = true;

    }
    public override void OnEndHideDialog()
    {
        base.OnEndHideDialog();
        bomb.SetActive(false);
        magnet.SetActive(false);
        Player.Instance.isAnimPlaying = false;

    }
    // Start is called before the first frame update
    public void PlayAds()
    {
        ZenSDK.instance.ShowVideoReward((isWatched) =>
        {
            if (isWatched)
            {
                DataAPIController.instance.SetItemTotal(type, 1);
                PurchaseItem();
            }
            else
            {
                //Debug.LogWarning("Watch reward unsuccesfull");
                CancelUsingItem();
            }
            ;
        });
    }
    public void PurchaseItem()
    {
        int wallet = DataAPIController.instance.GetGold();
        if (wallet >= price)
        {
            //Play successfully buy sound
            //Debug.LogWarning("Puchasing item");
            ads.onClick.RemoveListener(PlayAds);
            buy.onClick.RemoveListener(PurchaseItem);
            DataAPIController.instance.MinusGoldWallet(price, (isDone) =>
            {
            });
            //Debug.LogWarning("Puchasing item done");
            DataAPIController.instance.AddItemTotal(type, 1);
            GamePlayView view = ViewManager.Instance.currentView as GamePlayView;
            if (type == ItemType.Magnet) view.magnetItemEvent.Invoke(true);
            else if (type == ItemType.Bomb) view.bombItemEvent.Invoke(true);
            CancelUsingItem();
        }
        else
        {
            //play unsuccessfull sound;
            CancelUsingItem();
        }
    }
    public void CancelUsingItem()
    {
        DialogManager.Instance.HideDialog(dialogIndex, () =>
        {
            var currentView = ViewManager.Instance.currentView as GamePlayView;
            if (currentView == null) return;
            if (type == ItemType.Bomb) currentView.Bomb_Btn.interactable = true;
            else if (type == ItemType.Magnet) currentView.Magnet_btn.interactable = true;
        });
    }
    void ItemCase(ItemType type)
    {
        switch (type)
        {
            case ItemType.Bomb:
                tutorial_lb.text = "RANDOMLY CLEAR ALL CARD IN ONE SLOT";
                bomb.SetActive(true);
                magnet.SetActive(false);
                break;
            case ItemType.Magnet:
                tutorial_lb.text = "RAMDOMLY CHOOSE ONE COLOR TO CLEAR PER SLOT";
                magnet.SetActive(true);
                bomb.SetActive(false);
                break;
            default:
                tutorial_lb.text = "SOME THING WENT WRONG";
                break;
        }
    }
}
