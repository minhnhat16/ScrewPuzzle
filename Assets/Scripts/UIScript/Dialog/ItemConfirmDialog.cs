using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class ItemConfirmDialog : BaseDialog
    {
        [SerializeField] private ItemType type;
        [SerializeField] private bool isAds;
        [SerializeField] Text tutorial_lb;
        [SerializeField] Text price_lb;
        [SerializeField] Button ads;
        [SerializeField] Button buy;
        [SerializeField] int price;
        [SerializeField] private string detail;

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
                AddItemDialogParam param = (AddItemDialogParam)dialogParam;
                type = param.ItemType;
                isAds = param.IsAdsAvailable;
                detail = param.detail;
                price = param.ItemPrice;
                AdsButtonActiveIs(isAds);
                SetTutorialDetail(detail);
                SetPriceLb(price.ToString());
            }

        }
        public override void OnStartShowDialog()
        {
            base.OnStartShowDialog();
            //  price = Mathf.RoundToInt(price * IngameController.instance.GetPlayerLevel() / 2);

        }
        public override void OnEndHideDialog()
        {
            base.OnEndHideDialog();
            /*bomb.SetActive(false);
            magnet.SetActive(false);*/
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
                /*if (type == ItemType.AddBox) currentView.Bomb_Btn.interactable = true;
                else if (type == ItemType.AddHold) currentView.Magnet_btn.interactable = true;
                else if (type == ItemType.ClearOneScrew) currentView.Magnet_btn.interactable = true;*/

            });
        }

        private void AdsButtonActiveIs(bool isAdsAvailable)
        {
            ads.gameObject.SetActive(isAdsAvailable);
        }

        private void SetTutorialDetail(string detail)
        {
            tutorial_lb.text = detail.ToUpper();
        }
        private void SetPriceLb(string price)
        {
            price_lb.text = price;
        }
    }
}
