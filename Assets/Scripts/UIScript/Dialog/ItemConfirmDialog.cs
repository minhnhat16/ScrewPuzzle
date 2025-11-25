using Managers;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class ItemConfirmDialog : BaseDialog
    {
        [SerializeField] private ItemType type;
        [SerializeField] private string detail;
        [SerializeField] private bool isAds;
        [SerializeField] private Text tutorial_lb;
        [SerializeField] private Text price_lb;
        
        [SerializeField] private Button ads;
        [SerializeField] private Button buy;
        [SerializeField] private int price;

        public override void Setup(DialogParam dialogParam)
        {
            base.Setup(dialogParam);

            var param = dialogParam as AddItemDialogParam;

            type = param.ItemType;
            detail = param.detail;
            isAds = param.IsAdsAvailable;
            price = param.ItemPrice;

            tutorial_lb.text = detail.ToUpper();
            price_lb.text = price.ToString();
            ads.gameObject.SetActive(isAds);
        }

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

        private void PlayAds()
        {
            ZenSDK.instance.ShowVideoReward((isWatched) =>
            {
                if (!isWatched)
                {
                    DialogManager.ins.HideDialog(dialogIndex);
                    return;
                }

                DataAPIController.instance.AddItemTotal(type, 1);
                PaymentManager.ins.TriggerResult(true, "Reward received!");
                DialogManager.ins.HideDialog(dialogIndex);
            });
        }

        private void PurchaseItem()
        {
            // ❗ CHUYỂN QUA PAYMENT MANAGER
            PaymentManager.ins.PurchaseGameplayItem(type, price);
            DialogManager.ins.HideDialog(dialogIndex);
        }
    }
}
