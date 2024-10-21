using System.DataBase;
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
      //  price = Mathf.RoundToInt(price * IngameController.instance.GetPlayerLevel() / 2);
        price_lb.text = price.ToString();

    }
    public override void OnEndHideDialog()
    {
        base.OnEndHideDialog();
        bomb.SetActive(false);
        magnet.SetActive(false);
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
        // Get the current gold in the wallet
        int wallet = DataAPIController.instance.GetGold();

        // Check if the user has enough gold to purchase the item
        if (wallet >= price)
        {
            // Play successfully purchased sound (add sound logic here)

            // Remove any previously added listeners for ads and purchase buttons
            ads.onClick.RemoveListener(PlayAds);
            buy.onClick.RemoveListener(PurchaseItem);

            // Deduct the price from the wallet
            DataAPIController.instance.MinusGoldWallet(price, (isDone) =>
            {
                if (isDone)
                {
                    // Successfully deducted gold; proceed with adding the item
                    DataAPIController.instance.AddItemTotal(type, 1);
                
                    // Get the current gameplay view and trigger the respective item event
                    GamePlayView view = ViewManager.Instance.currentView as GamePlayView;
                    if (view != null)
                    {
                        // Check the item type and invoke the corresponding event
                        if (type == ItemType.AddBox)
                            view.magnetItemEvent?.Invoke(true);
                        else if (type == ItemType.AddHold)
                            view.bombItemEvent?.Invoke(true);
                        else if (type == ItemType.ClearOneScrew)
                            view.bombItemEvent?.Invoke(true);
                    }

                    // After purchase, cancel any item usage
                    CancelUsingItem();
                }
            });
        }
        else
        {
            // Play unsuccessful sound (add sound logic here)

            // Not enough gold, cancel the purchase process
            CancelUsingItem();
        }
    }
    public void CancelUsingItem()
    {
        DialogManager.Instance.HideDialog(dialogIndex, () =>
        {
            var currentView = ViewManager.Instance.currentView as GamePlayView;
            if (currentView == null) return;
            if (type == ItemType.AddBox) currentView.Bomb_Btn.interactable = true;
            else if (type == ItemType.AddHold) currentView.Magnet_btn.interactable = true;
            else if (type == ItemType.ClearOneScrew) currentView.Magnet_btn.interactable = true;

        });
    }
    void ItemCase(ItemType type)
    {
        switch (type)
        {
            case ItemType.AddBox:
                tutorial_lb.text = "ADD ON BOX TO THE BOX LIST";
                bomb.SetActive(true);
                magnet.SetActive(false);
                break;
            case ItemType.AddHold:
                tutorial_lb.text = "ADD ONE MORE HOLD TO YOU";
                magnet.SetActive(true);
                bomb.SetActive(false);
                break;
            case ItemType.ClearOneScrew:
                tutorial_lb.text = "ADD ONE MORE HOLD TO YOU";
                magnet.SetActive(true);
                bomb.SetActive(false);
                break;
            default:
                tutorial_lb.text = "SOME THING WENT WRONG";
                break;
        }
    }
}
