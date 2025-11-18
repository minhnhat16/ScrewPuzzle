using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackMiniItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    private int quantity;
    private string spriteName;
    [SerializeField] private Text textQuantity;

    public Image Icon => icon;

    public int Quantity => quantity;

    public string SpriteName => spriteName;

    public Text TextQuantity => textQuantity;


    public void Init(int quantity, Sprite sprite)
    {
        this.quantity = quantity;
        this.icon.sprite = sprite; 

    }
}
