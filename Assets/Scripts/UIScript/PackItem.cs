using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UIScript
{
    public class PackItem : MonoBehaviour
    {
        [SerializeField] private float price;
        [SerializeField] private Text ribbonText;
        [SerializeField] private Dictionary<string, PackMiniItem> miniItemsDict;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image ribon;

        public Image ItemIcon
        {
            get => itemIcon;
            set => itemIcon = value;
        }

        public Image Ribon
        {
            get => ribon;
            set => ribon = value;
        }

        public Text RibbonText
        {
            get => ribbonText;
            set => ribbonText = value;
        }

        public Dictionary<string, PackMiniItem> MiniItemsDict
        {
            get => miniItemsDict;
            set => miniItemsDict = value;
        }

        public Button PurchaseButton
        {
            get => purchaseButton;
            set => purchaseButton = value;
        }

        public float Price
        {
            get => price;
            set => price = value;
        }

        public PackItem()
        {
        }

        public PackItem(Text ribbonText, float price)
        {
            this.ribbonText = ribbonText;
            this.price = price;
        }
    }
}