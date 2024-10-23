using System.Transactions;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript
{
    public class ItemButtonIngame : ItemButton
    {
        public override void OnEnable()
        {
            //Button = GetComponent<Button>();
            Button.onClick.AddListener(OnClick);
            
            //AddQuantityBtn = GetComponentInChildren<Button>();
            AddQuantityBtn.onClick.AddListener(OnAddQuantity);
            
        }
        public override void OnClick()
        {   
        }

        public override void OnAddQuantity()
        {
            bool isAdsAvailable = ZenSDK.instance.IsVideoRewardReady();
            var itemConfig = PriceConfig(Type);
            
            AddItemDialogParam param = new AddItemDialogParam();
            param.ItemType = Type;
            param.ItemPrice =ZenSDK.instance.GetConfigInt($"price{Type}",  itemConfig.Price);
            param.IsAdsAvailable = isAdsAvailable;
            DialogManager.Instance.ShowDialog(DialogIndex.AddItemDialog,param,null );
        }

        public ItemConfigRecord PriceConfig(ItemType itemType)
        {
            var itemPriceConfig = ConfigFileManager.Instance.ItemConfig.GetRecordByKeySearch(itemType);
            return itemPriceConfig;
        }
    }
}
