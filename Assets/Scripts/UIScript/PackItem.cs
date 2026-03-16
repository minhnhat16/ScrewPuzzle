using Coffee.UIExtensions;
using ConfigFile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace UIScript
{
    public class PackItem : MonoBehaviour
    {
        private float time;
        private float price;
        private int amount;
        [SerializeField] internal Text amountText;
        [SerializeField] internal Text ribbonText;
        [SerializeField] internal Text priceText;
        [SerializeField] internal Dictionary<string, PackMiniItem> miniItemsDict;
        [SerializeField] internal Button purchaseButton;
        [SerializeField] internal Image itemIcon;
        [SerializeField] internal Image ribon;


        [SerializeField] internal UIParticle particles;
        [SerializeField]
        private RectTransform itemContainer;


        public GameObject itemPrefab;

        public int Amount { get => amount; set => amount = value; }
        public float Time { get => time; set => time = value; }
        public float Price { get => price; set => price = value; }
        public Text AmountText { get => amountText; set => amountText = value; }
        public Text RibbonText { get => ribbonText; set => ribbonText = value; }
        public Text PriceText { get => priceText; set => priceText = value; }
        public Dictionary<string, PackMiniItem> MiniItemsDict1 { get => miniItemsDict; set => miniItemsDict = value; }
        public Button PurchaseButton1 { get => purchaseButton; set => purchaseButton = value; }
        public Image ItemIcon1 { get => itemIcon; set => itemIcon = value; }
        public Image Ribon1 { get => ribon; set => ribon = value; }
        public RectTransform ItemContainer { get => itemContainer; set => itemContainer = value; }

        public Action<PackConfigRecord> OnBuyClicked;
        internal PackConfigRecord packData;


        public PackItem()
        {
        }
        public PackItem(string packname, float price, int amount)
        {
            this.ribbonText.text = packname;
            this.priceText.text = GameUtils.FormatPrice((long)price);
            this.amount = amount;
            this.amountText.text = $"x{amount}";
        }
        public void Init(string name, long price, int amount)
        {
            this.ribbonText.name = name;
            this.price = price;
            this.priceText.text = price > 0 ?  GameUtils.FormatPrice(price) : "FREE";
            this.amount = amount;
            this.amountText.text = $"x{amount}";

        }

        private void OnEnable()
        {
            if(particles != null)
                particles.SetMaterialDirty();
            purchaseButton.onClick.AddListener(() =>
            {

                Debug.Log("Pack item on clicked");
                OnBuyClicked?.Invoke(packData);
            });
        }
        private void OnDisable()
        {
            purchaseButton.onClick.RemoveAllListeners();
        }
        public virtual void Init(PackConfigRecord packConfig)
        {
            this.packData = packConfig;
         
            // Set pack title + price
            if(ribbonText != null)  ribbonText.text = packConfig.Name;
            this.priceText.text = packConfig.Price > 0 ? GameUtils.FormatPrice(packConfig.Price) : "FREE";

            // Clear old items
            foreach (Transform child in ItemContainer)
                child.gameObject.SetActive(false);

            var itemConfig = packConfig.Items;
            if (itemConfig.Count < 2)
            {
                var item = itemConfig.FirstOrDefault();
                PackMiniItem miniItem;
                miniItem = Instantiate(itemPrefab, ItemContainer).GetComponent<PackMiniItem>();
                itemContainer.sizeDelta = Vector2.one * GameConstants.MINI_SIZE * 2;
                Sprite sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, item.Id.ToString());
                miniItem.Init(item.Id, item.Quantity, sprite);
                return;
            }
            // Spawn each item inside the bundle
            foreach (var item in packConfig.Items)
            {
                PackMiniItem miniItem;
                miniItem = Instantiate(itemPrefab, ItemContainer).GetComponent<PackMiniItem>();
                miniItem.rectTransform.sizeDelta = Vector2.one * GameConstants.MINI_SIZE;
                Sprite sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, item.Id.ToString());
                miniItem.Init(item.Id, item.Quantity, sprite);
            }
        }
    }
}
