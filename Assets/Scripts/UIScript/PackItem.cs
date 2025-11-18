using ConfigFile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UIScript
{
    public class PackItem : MonoBehaviour
    {
        [SerializeField] private float time;
        [SerializeField] private float price;
        [SerializeField] private int amount;
        [SerializeField] private Text amountText;
        [SerializeField] private Text ribbonText;
        [SerializeField] private Text priceText;
        [SerializeField] private Dictionary<string, PackMiniItem> miniItemsDict;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image ribon;

        [SerializeField]
        private readonly RectTransform itemContainer;


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
            this.priceText.text = GameUtils.FormatPrice(price);
            this.amount = amount;
            this.amountText.text = $"x{amount}";

        }

        public void Init(PackConfigRecord packConfig)
        {
            // Set pack title + price
            ribbonText.text = packConfig.Name;
            priceText.text = GameUtils.FormatPrice(packConfig.Price);

            // Clear old items
            foreach (Transform child in itemContainer)
                Destroy(child.gameObject);

            // Spawn each item inside the bundle
            foreach (var item in packConfig.Items)
            {
              
                var miniItem = Instantiate(itemPrefab, itemContainer).GetComponent<PackMiniItem>();
               Sprite sprite =   SpriteLibControl.Instance.GetSpriteByName(miniItem.name);
                miniItem.Init(item.Quantity, sprite);
            }
        }
    }
}
