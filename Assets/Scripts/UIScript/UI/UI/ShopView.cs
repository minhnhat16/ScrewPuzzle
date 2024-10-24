using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConfigFile;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.UI.UI
{
    public class ShopView : BaseView
    {
        [SerializeField] private int gold;
        [SerializeField] private RectTransform packRect;
        [SerializeField] private RectTransform priceRect;
        [SerializeField] private Slider switchSlider;
        [SerializeField] private List<ShopItem> shopItems;
        [SerializeField] private Text lablePrice;
        [SerializeField] private Text lablePack;
        [SerializeField] private Text goldLable;
        
        [SerializeField] private Button closeButton;
        // [SerializeField] private List<PackItem> packItems;
        public override void OnInit()
        {
            var ShopConfig = ConfigFileManager.Instance.PriceConfig.GetAllRecord();
            var shopItemConfigList = ShopConfig.Where(item => item.IdShop == 1).ToList();
            LoadItemFromConfig(shopItemConfigList);
            
            var packItemConfigLists =  ConfigFileManager.Instance.PackConfig.GetAllRecord();
            LoadPackFormConfig(packItemConfigLists);
            closeButton.onClick.AddListener(()=> ViewManager.Instance.SwitchView(ViewIndex.MainScreenView));
        }
        public override void Setup(ViewParam param)
        {
            var newParam = (ShopViewParam)param;
            gold = newParam.gold;
            goldLable.text = GameManager.instance.DevideCurrency(gold) ;
        }
        public override void OnStartShowView()
        {
            switchSlider.value = 0;
            SliderValueChange();
        }
        private void  LoadItemFromConfig(List<PriceConfigRecord>  shopItemConfigRecords)
        {
            int i = 0;
            foreach (var shopItemConfig in shopItemConfigRecords)
            {
                var shopItem = ShopItemPool.Instance.Pool.SpawnNonGravityWithIndex(i++);
                shopItem.Price = shopItemConfig.Price;
                shopItem.Quantity = shopItemConfig.Amount;
                shopItem.QuantityText.text = $"+{shopItem.Quantity}";
                shopItem.BuyPriceText.text = $"+{shopItem.Quantity}";
                var spriteImg = SpriteLibControl.Instance.GetSpriteByName(shopItemConfig.SpriteName);
                shopItem.IconImage.sprite = spriteImg;
                shopItem.IconImage.SetNativeSize();
                shopItem.BuyButton.onClick.AddListener(()=>OnShopItemPurchase(shopItem)); 
                Debug.LogError($"shop item config init {i}  and shop item {shopItem ==null}");
                shopItems.Add(shopItem);
            }
        }
        private void  LoadPackFormConfig(List<PackConfigRecord>  shopItemConfigRecords)
        {
            int i = 0;
            foreach (var packItemCR in shopItemConfigRecords)
            {
                var packItem = PackItemPool.Instance.Pool.SpawnNonGravityWithIndex(i++);
                packItem.Price = packItemCR.Price;
                packItem.RibbonText.text = packItemCR.RibbonText;
                var ribbonSprite = SpriteLibControl.Instance.GetSpriteByName(packItemCR.RibbonColorName);
                packItem.Ribon.sprite =ribbonSprite;

                PackMiniItem miniItem1 = new(packItemCR.QuantityItem1,packItemCR.IconItem1);
                PackMiniItem miniItem2 = new(packItemCR.QuantityItem2,packItemCR.IconItem2);
                PackMiniItem miniItem3 = new(packItemCR.QuantityItem3,packItemCR.IconItem3);

                packItem.MiniItemsDict = new ();
                packItem.MiniItemsDict.TryAdd(packItemCR.IconItem1,miniItem1);
                packItem.MiniItemsDict.TryAdd(packItemCR.IconItem2,miniItem2);
                packItem.MiniItemsDict.TryAdd(packItemCR.IconItem3,miniItem3);
                
                packItem.PurchaseButton.onClick.AddListener(()=>OnPackItemPurchase(packItem));
            }
        }
        private void OnShopItemPurchase(ShopItem shopItem)
        {
            // Your purchase logic here
            Debug.Log($"Purchased {shopItem.Quantity} of item costing {shopItem.Price}");
        }
        private void OnPackItemPurchase(PackItem shopItem)
        {
            // Your purchase logic here
            Debug.Log($"Purchased {shopItem.ItemIcon} of item costing {shopItem.Price}");
        }

        private void LoadShopItem(List<PriceConfigRecord>  packItemConfigRecords)
        {
       
        }

        public void SliderValueChange()
        {
            float value = switchSlider.value;
            bool valueBool = value < 1 ; // 0 là view price 1 là view pack
            SwitchMiniView(valueBool); // hàm set mini view cho price và pack
        }

        public void SwitchMiniView(bool isActive =false)
        {
            //set hiển thij label trên switch, chỉ có 1 cái được active 
            lablePrice.gameObject.SetActive(isActive);
            lablePack.gameObject.SetActive(!isActive);
            
            //set mini view active tương tự trên
            
            priceRect.gameObject.SetActive(isActive);
            packRect.gameObject.SetActive(!isActive);

        }
    }
 
}

