using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

public class SpecialDialog : BaseDialog
{
   [SerializeField] private bool isPaymentAvailable;
    [SerializeField] private bool isPaid;
    [SerializeField] private float price;
    [SerializeField] private string currentTime;
    [SerializeField] private string currency;
    [SerializeField] private List<ShopItem> _shopItem;
    [SerializeField] GoldDisplay goldDisplay;
    [SerializeField] Button onPurchaceButton;
    [SerializeField] Button closeBtn;
    [SerializeField] Text timeCounter;
    [SerializeField] Text priceLable;
    [SerializeField] Action<bool> onPaymentHasDone;
    public void OnEnable()
    {
        closeBtn.onClick.AddListener(CloseDialog);
        DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
        RefreshCurrencyUI();
    }

    private void OnDisable()
    {
        closeBtn.onClick.RemoveListener(CloseDialog);
        DataTrigger.UnRegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
    }
    public override void Setup(DialogParam dialogParam)
    {
        SpecialDialogParam param = dialogParam as SpecialDialogParam;
        isPaymentAvailable = param.isPaymentAvailable;
        isPaid = param.isPaid;
        price = param.price;

        currentTime = param.time;
        currency = param.currency;
        _shopItem = param.specialItems;
        long userGold = param.totalGold;
        goldDisplay.SetGoldToLable(userGold);
        SetPriceLabel(price, currency);
        SetTimeCounter(currentTime);

    }
    
    public void SetPriceLabel(float price, string currency)
    {
        priceLable.text = $"{price} {currency}";
    }

    public void SetTimeCounter(string time)
    {
        DayTimeController.instance.StartCountingTime(time, timeCounter);
    }
    public void OnPurchase()
    {
        if (isPaymentAvailable)
        {
           
            Debug.Log("Doing payment method");
            bool isPaymentDone = true; //Doing payment and switch this flag
            onPaymentHasDone?.Invoke(isPaymentDone);
        }
    }
    public void AddItemAfterPurchasing(bool isSuccess)
    {
        
    }
    public void CloseDialog()
    {
        DialogManager.ins.HideDialog(dialogIndex);
    }

    private void RefreshCurrencyUI()
    {
        goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
    }

    private void OnGoldChanged(object _)
    {
        goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
    }
}
