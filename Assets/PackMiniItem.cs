using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackMiniItem : MonoBehaviour
{
    [SerializeField] private Sprite icon;
    [SerializeField] private int quantity;
    [SerializeField] private string spriteName;
    [SerializeField] private Text textQuantity;

    public Sprite Icon => icon;

    public int Quantity => quantity;

    public string SpriteName => spriteName;

    public Text TextQuantity => textQuantity;

    public PackMiniItem()
    {
    }

    public PackMiniItem(int quantity, string spriteName)
    {
        this.quantity = quantity;
        this.spriteName = spriteName;
    }
}