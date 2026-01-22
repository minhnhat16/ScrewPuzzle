using System;
using System.DataBase;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIScript
{
    // Fix: Only inherit from ItemButton, which itself should inherit from MonoBehaviour
    public class ItemButtonIngame : ItemButton
    {
        public override void OnEnable()
        {
            //Button = GetComponent<Button>();
            DataTrigger.RegisterValueChange(DataPath.ITEMDICT, OnItemQuantityChanged);
            Button.onClick.AddListener(OnClick);
            AddQuantityBtn.onClick.AddListener(OnAddQuantity);
        }

        private void OnItemQuantityChanged(object arg0)
        {

        }

        public override void OnDisable()
        {
            Button.onClick.RemoveListener(OnClick);
            AddQuantityBtn.onClick.RemoveListener(OnAddQuantity);
        }

        private void Start()
        {
            IsItemAvailable();
        }
        public void SetItemQuantity(int qty)
        {
            Quantity = qty;
            TextLB.text = Quantity.ToString();
            IsItemAvailable();
        }
        public bool IsItemAvailable()
        {
            TextLB.gameObject.SetActive(Quantity > 0);
            AddQuantityBtn.gameObject.SetActive(Quantity <= 0);
            return Quantity > 0;
        }
        public override void OnClick()
        {

            Debug.Log("on button click " + Button.interactable);
        }

        public override void OnAddQuantity()
        {
            bool isAdsAvailable = ZenSDK.instance.IsVideoRewardReady();
            var itemConfig = PriceConfig(Type);

            AddItemDialogParam param = new AddItemDialogParam();
            param.ItemType = Type;
            param.ItemPrice = ZenSDK.instance.GetConfigInt($"price{Type}", itemConfig.Price);
            param.IsAdsAvailable = isAdsAvailable;
            Quantity += itemConfig.Quantity;
            DataAPIController.instance.AddItemTotal(Type, itemConfig.Quantity);
            DialogManager.ins.ShowDialog(DialogIndex.ItemDialog, param, () =>
            {
                Button.interactable = true;
                IsItemAvailable();

                Debug.Log("on button click show dialog " + Button.interactable);
            });
        }

        public ItemConfigRecord PriceConfig(ItemType itemType)
        {
            var itemPriceConfig = ConfigFileManager.Instance.GetItemConfig(itemType);
            return itemPriceConfig;
        }

        internal void AddListener(UnityAction action)
        {
            Button.onClick.AddListener(action);
        }

        internal void RemoveListener(UnityAction action)
        {
            Button.onClick.RemoveListener(action);

        }
    }
}
