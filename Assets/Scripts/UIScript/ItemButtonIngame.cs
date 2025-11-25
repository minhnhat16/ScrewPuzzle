using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIScript
{
    public class ItemButtonIngame : ItemButton
    {
        public override void OnEnable()
        {
            //Button = GetComponent<Button>();
            Button.onClick.AddListener(OnClick);
            AddQuantityBtn.onClick.AddListener(OnAddQuantity);
        }
        public override void OnDisable()
        {
            Button.onClick.RemoveListener(OnClick);
            AddQuantityBtn.onClick.RemoveListener(OnAddQuantity);
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
            DialogManager.ins.ShowDialog(DialogIndex.AddItemDialog, param, () =>
            {
                Button.interactable = true;

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
