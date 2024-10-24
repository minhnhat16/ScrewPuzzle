using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
  
   [SerializeField] private int quantity;
   [SerializeField] private int price;
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

   public ShopItem(int quantity, int price, Button buyButton, Text buyPriceText, Text quantityText, Image iconImage)
   {
      this.quantity = quantity;
      this.price = price;
      this.buyButton = buyButton;
      this.buyPriceText = buyPriceText;
      this.quantityText = quantityText;
      this.iconImage = iconImage;
   }  
}
