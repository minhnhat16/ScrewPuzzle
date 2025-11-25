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
    [SerializeField] private Text textDetail;


    public RectTransform rectTransform;
    public Image Icon => icon;

    public int Quantity => quantity;

    public string SpriteName => spriteName;

    public Text TextQuantity => textQuantity;

    private void Awake()
    {
        icon.preserveAspect = true;
        this.rectTransform = GetComponent<RectTransform>();
    }
    public void Init(ItemType type, int quantity, Sprite sprite)
    {
        this.quantity = quantity;
        this.icon.sprite = sprite;

        this.TextQuantity.text = $"x{quantity}";

        if (textDetail != null)
            this.textDetail.gameObject.SetActive(type == ItemType.Ticket);
    }
}
