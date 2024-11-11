using System;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{

    [SerializeField] private int quantity;
    [SerializeField] private int price;
    [SerializeField] private ItemType type;

    [SerializeField] private Button buyButton;
    [SerializeField] private Text buyPriceText;
    [SerializeField] private Text quantityText;
    [SerializeField] private Image iconImage;

    public int Quantity
    {
        get => quantity;
        set => quantity = value;
    }

    public int Price
    {
        get => price;
        set => price = value;
    }

    public Button BuyButton
    {
        get => buyButton;
        set => buyButton = value;
    }

    public Text BuyPriceText
    {
        get => buyPriceText;
        set => buyPriceText = value;
    }

    public Text QuantityText
    {
        get => quantityText;
        set => quantityText = value;
    }

    public Image IconImage
    {
        get => iconImage;
        set => iconImage = value;
    }
    public ItemType Type 
    { 
        get => type; 
        set => type = value; 
    }

    public ShopItem(int quantity, int price, Button buyButton, Text buyPriceText, Text quantityText, Image iconImage, ItemType type = default)
    {
        this.quantity = quantity;
        this.price = price;
        this.buyButton = buyButton;
        this.buyPriceText = buyPriceText;
        this.quantityText = quantityText;
        this.iconImage = iconImage;
        this.type = type;
    }

    public void PurchasingItem(int price, Action<bool> callback = null)
    {
        DataAPIController.instance.MinusGoldWallet(price, callback);
    }
    public void DonePurchase(ItemType type, int quantity)
    {
        DataAPIController.instance.AddItemTotal(type, quantity);
    }
}
