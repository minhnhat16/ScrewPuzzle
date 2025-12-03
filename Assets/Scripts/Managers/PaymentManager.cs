using ConfigFile;
using System;
using System.DataBase;
using UnityEngine;


public struct PaymentResult
{
    public bool success;
    public string message;
}
public class PaymentManager : SingletonMono<PaymentManager>
{
    public event System.Action<PaymentResult> OnPaymentCompleted;

    public void PurchasePack(PackConfigRecord config)
    {
        if (config.CurrencyType == Currency.RealMoney)
        {
            BuyRealMoneyItem(config);
        }
        else if (config.CurrencyType == Currency.Ads)
        {
        }
        else
        {
            BuySoftCurrencyItem(config);
        }
    }

    private void BuySoftCurrencyItem(PackConfigRecord cfg)
    {
        long price = cfg.Price;

        if (!WalletManager.ins.HasEnough(cfg.CurrencyType, price))
        {
            TriggerResult(false, "Not enough " + cfg.CurrencyType);
            return;
        }

        // Deduct currency
        WalletManager.ins.Spend(cfg.CurrencyType, price);

        // Give items
        DataAPIController.instance.AddItemByConfig(cfg.Items);

        // Return result (NO UI HERE)
        TriggerResult(true, "Purchase successful!");
    }
    public void TriggerResult(PaymentResult result)
    {
        OnPaymentCompleted?.Invoke(result);
    }
    public void TriggerResult(bool success, string msg)
    {
        PaymentResult result = new PaymentResult()
        {
            success = success,
            message = msg,
            // pack = cfg
        };

        OnPaymentCompleted?.Invoke(result);
    }

    public void BuyRealMoneyItem(PackConfigRecord cfg)
    {

        Debug.Log("BuyRealMoneyItem " + cfg);
        //IAPManager.Instance.BuyProduct(cfg.IAPId, () =>
        //{
        //    success
        //    InventoryManager.Instance.AddItems(cfg.Items);
        //    UIManager.ShowPopup("Purchase completed!");

        //});

    }

    public void PurchaseGameplayItem(ItemType type, int price)
    {
        if (!WalletManager.ins.HasEnough(Currency.Gold, price))
        {
            TriggerResult(false, "Not enough gold");
            return;
        }

        WalletManager.ins.Spend(Currency.Gold, price);
        DataAPIController.instance.AddItemTotal(type, 1);

        TriggerResult(new PaymentResult
        {
            success = true,
            message = "Purchased item successfully"
        });
    }

}
