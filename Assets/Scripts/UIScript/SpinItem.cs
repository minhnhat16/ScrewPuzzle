// using Coffee.UIEffects;

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpinItem : MonoBehaviour
{

    [SerializeField] int id;
    [SerializeField] SpinEnum itemType;
    [SerializeField] Currency currencyType;
    [SerializeField] int amount;
    [SerializeField] Image itemImg;
    [SerializeField] Text amount_lb;
    [SerializeField] float rotateValue;
    // [SerializeField] UIShiny ui_Shiny;
    [SerializeField] RectTransform itemRectTransform;
    [SerializeField] RectTransform rect;

    // Start is called before the first frame update

    public int ID {  get { return id; } }
    public SpinEnum Type { get { return itemType; } }
    public int Amount {  get { return amount; } }

    public float RotateValue { get => rotateValue; set => rotateValue = value; }
    public RectTransform Rect { get => rect; set => rect = value; }

    void Start()
    {
        rotateValue = transform.localRotation.z;
        // ui_Shiny = GetComponentInChildren<UIShiny>();
        itemRectTransform = itemImg.GetComponent<RectTransform>();
        rect = GetComponent<RectTransform>();
    }

    public void InitItem(SpinConfigRecord record)
    {
        //Debug.Log(record.Type + " " + record.Id);
        id = record.Id;
        itemType = record.Type;
        amount = record.Amount;
        amount_lb = GetComponentInChildren<Text>();
        amount_lb.text = $"{amount}";
        itemImg.sprite = SpriteLibControl.Instance.GetSpriteByName(record.ItemImg);
        itemImg.SetNativeSize();
    }
    public void OnRewardItem()
    {
       
        if (itemType == SpinEnum.Gold)
        {
            DataAPIController.instance.AddGold(Amount,null);
        }
        else if (itemType == SpinEnum.Gem)
        {
            DataAPIController.instance.AddGem(Amount);

        }
        else if (itemType == SpinEnum.Magnet && itemType == SpinEnum.Bomb )
        {
            //Debug.Log($"Added to data {amount} item {itemType} ");

            DataAPIController.instance.AddItemTotal((ItemType)itemType, amount);
        }
        else if (itemType == SpinEnum.Bonus)
        {
            //Debug.Log($"Added to data {amount} item {type} ");
            //DataAPIController.instance.SaveFruitSkin(amount);
        }
    }
    public void ItemRewarding()
    {
        itemRectTransform.DOScale(1.4f, 2f);
        // ui_Shiny.Play();
        amount_lb.fontSize = 40;
    }
}
