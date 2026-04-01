using Managers;
using System;
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
        [SerializeField] private Image icon;
        [SerializeField] private Button ads;
        [SerializeField] private Button buy;
        [SerializeField] private int price;
        [SerializeField] private GoldDisplay goldDisplay;
        [SerializeField] private GoldDisplay ticketDisplay;

        public override void Setup(DialogParam dialogParam)
        {
            base.Setup(dialogParam);

            var param = dialogParam as AddItemDialogParam;

            type = param.ItemType;
            detail = param.detail;
            isAds = param.IsAdsAvailable;
            price = param.ItemPrice;
            icon.sprite = param.sprite;
            if (detail != null)
                tutorial_lb.text = detail.ToUpper();
            price_lb.text = price.ToString();
            ads.gameObject.SetActive(isAds);
        }

        private void OnEnable()
        {
            ads.onClick.AddListener(PlayAds);
            buy.onClick.AddListener(PurchaseItem);
            WalletManager.ins.OnCurrencyUpdated += OnCurrencyUpdated;
            if (PaymentManager.ins != null)
                PaymentManager.ins.OnPaymentCompleted += OnPaymentCompleted;

            RefreshCurrencyLabels();
            RefreshButtonState();
        }

        private void OnDisable()
        {
            ads.onClick.RemoveListener(PlayAds);
            buy.onClick.RemoveListener(PurchaseItem);

            WalletManager.ins.OnCurrencyUpdated -= OnCurrencyUpdated;
            if (PaymentManager.ins != null)
                PaymentManager.ins.OnPaymentCompleted -= OnPaymentCompleted;

        }



        private void PlayAds()
        {
            if (!ZenSDK.instance.IsVideoRewardReady())
            {
                PaymentManager.ins.TriggerGameplayItemResult(false, "Ads not available", type);
                return;
            }

            ZenSDK.instance.TrackRewardOfferAccept(GameConstants.FREE_ITEM_KEY, DataAPIController.instance.GetPlayerLevel().ToString(), ZenSDK.instance.IsNetworkConnected().ToString());
            ZenSDK.instance.ShowVideoReward((isWatched) =>
            {
                if (!isWatched)
                {
                    PaymentManager.ins.TriggerGameplayItemResult(false, "Ads not completed", type);
                    return;
                }

                DataAPIController.instance.AddItemTotal(type, 1);
                PaymentManager.ins.TriggerGameplayItemResult(true, "Reward received!", type);
                DialogManager.ins.HideDialog(dialogIndex);
            }, GameConstants.FREE_ITEM_KEY, DataAPIController.instance.GetPlayerLevel().ToString());
        }

        private void PurchaseItem()
        {
            PaymentManager.ins.PurchaseGameplayItem(type, price);
        }

        private void OnCurrencyUpdated(Currency currency, long value)
        {
            if (currency == Currency.Gold)
                goldDisplay.SetGoldToLable(value);
            else if (currency == Currency.Ticket)
                ticketDisplay.SetGoldToLable(value);

            RefreshButtonState();
        }

        private void OnPaymentCompleted(PaymentResult result)
        {
            if (!result.isGameplayItemPurchase || result.gameplayItemType != type)
                return;

            RefreshButtonState();

            if (result.success)
                DialogManager.ins.HideDialog(dialogIndex);
        }

        private void RefreshCurrencyLabels()
        {
            goldDisplay.SetGoldToLable(WalletManager.ins.Get(Currency.Gold));
            ticketDisplay.SetGoldToLable(WalletManager.ins.Get(Currency.Ticket));
        }

        private void RefreshButtonState()
        {
            if (buy != null)
                buy.interactable = !PaymentManager.ins.IsPurchasing && WalletManager.ins.HasEnough(Currency.Gold, price);

            if (ads != null)
                ads.interactable = !PaymentManager.ins.IsPurchasing && isAds && ZenSDK.instance.IsVideoRewardReady();
        }
    }
}
