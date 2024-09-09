using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmButton : MonoBehaviour
{
    [SerializeField] private ButtonType btnType;
    [SerializeField] private Text text;
    [SerializeField] List<Image> _typesImage;
    public string priceTxt;
    public ButtonType Btntype  { get { return btnType; } }
    public  UnityEvent<bool> onClickAction = new UnityEvent<bool>();
    private void OnEnable()
    {
        
    }
    private void Start()
    {
        
    }
    public void OnClickButton()
    {
        //Debug.Log("OnclickButton");
        onClickAction?.Invoke(true);
    }
   
    public void SwitchButtonType(ButtonType type)
    {
        DisableAllButton();
        switch (type)
        {
            case ButtonType.Ads:
                //Ads type on 
                btnType = type;
                //Debug.Log(type.ToString() + " int " + btnType);
                EnableButtonImage(type);
                break;
            case ButtonType.Buy:
                //Buy type on 
                btnType = type;
                EnableButtonImage(type);
                break;
            case ButtonType.Equiped:
                //Equiped type on 
                btnType = type;
                //Debug.Log(type.ToString() + " int " + btnType);
                EnableButtonImage(type);
                break;

            case ButtonType.Unquiped:
                btnType = type;
                EnableButtonImage(type);
                break;
            //unEquiped type on 
            default:
                //disable button
                break;
        }
    }
    public void DisableAllButton()
    {
        foreach(var item in _typesImage)
        {
           item.gameObject.SetActive(false);

        }
    }
    public void EnableButtonImage(ButtonType type)
    {
        var item = _typesImage[(int)type];
        item.gameObject.SetActive(true);
    }
    public void UpdatePriceLb(string price)
    {
        text.text = price;
    }
}

public enum ButtonType
{
    None = 0,
    Equiped =1,
    Unquiped =2,
    Ads = 3,
    Buy =4,
}

