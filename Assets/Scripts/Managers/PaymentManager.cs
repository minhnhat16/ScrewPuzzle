using ConfigFile;
using System;
using System.DataBase;
using UnityEngine;

public struct PaymentResult
{
    public bool success;
    public string message;
    public PackConfigRecord pack; // FIX: thêm để caller biết pack nào vừa mua
    public bool isGameplayItemPurchase;
    public ItemType gameplayItemType;
}

public class PaymentManager : SingletonMono<PaymentManager>
{
    public event Action<PaymentResult> OnPaymentCompleted;

    // ─── FIX: Guard chống double-purchase ───────────────────────────
    private bool _isPurchasing = false;
    public bool IsPurchasing => _isPurchasing;

    // ────────────────────────────────────────────────────────────────
    // ENTRY POINT
    // ────────────────────────────────────────────────────────────────

    public void PurchasePack(PackConfigRecord config)
    {
        // FIX: Block nếu đang xử lý một purchase khác
        if (_isPurchasing)
        {
            Debug.LogWarning("[PaymentManager] Purchase already in progress. Ignoring.");
            return;
        }

        if (config == null)
        {
            Debug.LogError("[PaymentManager] PackConfigRecord is null.");
            return;
        }

        _isPurchasing = true;

        switch (config.CurrencyType)
        {
            case Currency.RealMoney:
                BuyRealMoneyItem(config);
                // FIX: RealMoney KHÔNG reset _isPurchasing ở đây
                // vì IAP callback async — reset trong callback
                break;

            case Currency.Ads:
                BuyWithAds(config);
                break;

            default:
                BuySoftCurrencyItem(config);
                // Soft currency xử lý sync → reset ngay
                _isPurchasing = false;
                break;
        }
    }

    // ────────────────────────────────────────────────────────────────
    // SOFT CURRENCY (Gold / Ticket)
    // ────────────────────────────────────────────────────────────────

    private void BuySoftCurrencyItem(PackConfigRecord cfg)
    {
        long price = cfg.Price;

        if (!WalletManager.ins.HasEnough(cfg.CurrencyType, price))
        {
            TriggerResult(false, "Not enough " + cfg.CurrencyType, cfg);
            return;
        }

        WalletManager.ins.Spend(cfg.CurrencyType, price);
        DataAPIController.instance.AddItemByConfig(cfg.Items);
        TriggerResult(true, "Purchase successful!", cfg);
    }

    // ────────────────────────────────────────────────────────────────
    // ADS — FIX: không còn để trống
    // ────────────────────────────────────────────────────────────────

    private void BuyWithAds(PackConfigRecord cfg)
    {
        if (!ZenSDK.instance.IsVideoRewardReady())
        {
            Debug.LogWarning("[PaymentManager] Ads not ready.");
            TriggerResult(false, "Ads not available", cfg);
            _isPurchasing = false;
            return;
        }

        ZenSDK.instance.ShowVideoReward((isWatched) =>
        {
            if (isWatched)
            {
                DataAPIController.instance.AddItemByConfig(cfg.Items);
                TriggerResult(true, "Ads reward received!", cfg);
            }
            else
            {
                TriggerResult(false, "Ads not completed", cfg);
            }

            // FIX: reset sau khi ads callback xong (async)
            _isPurchasing = false;
        }, GameConstants.FREE_COIN_DAILY_KEY, DataAPIController.instance.GetPlayerLevel().ToString());
    }

    // ────────────────────────────────────────────────────────────────
    // REAL MONEY (IAP)
    // ────────────────────────────────────────────────────────────────

    public void BuyRealMoneyItem(PackConfigRecord cfg)
    {
        // TODO: Tích hợp IAP SDK thực tế ở đây
        // Ví dụ với Unity IAP:
        //
        // IAPManager.Instance.BuyProduct(cfg.IAPId,
        //     onSuccess: () =>
        //     {
        //         DataAPIController.instance.AddItemByConfig(cfg.Items);
        //         TriggerResult(true, "Purchase successful!", cfg);
        //         _isPurchasing = false;
        //     },
        //     onFailed: (reason) =>
        //     {
        //         TriggerResult(false, "IAP failed: " + reason, cfg);
        //         _isPurchasing = false;
        //     }
        // );

        // Placeholder cho đến khi IAP được tích hợp
        Debug.LogWarning("[PaymentManager] BuyRealMoneyItem: IAP not implemented yet.");
        TriggerResult(false, "IAP not implemented", cfg);
        _isPurchasing = false;
    }

    // ────────────────────────────────────────────────────────────────
    // GAMEPLAY ITEM (mua item trong game bằng Gold)
    // ────────────────────────────────────────────────────────────────

    public void PurchaseGameplayItem(ItemType type, int price)
    {
        if (_isPurchasing)
        {
            Debug.LogWarning("[PaymentManager] Purchase already in progress.");
            TriggerGameplayItemResult(false, "Purchase already in progress.", type);
            return;
        }

        if (!WalletManager.ins.HasEnough(Currency.Gold, price))
        {
            TriggerGameplayItemResult(false, "Not enough gold", type);
            return;
        }

        _isPurchasing = true;
        WalletManager.ins.Spend(Currency.Gold, price);
        DataAPIController.instance.AddItemTotal(type, 1);
        TriggerGameplayItemResult(true, "Purchased item successfully", type);
        _isPurchasing = false;
    }

    // ────────────────────────────────────────────────────────────────
    // TRIGGER RESULT
    // ────────────────────────────────────────────────────────────────

    public void TriggerResult(bool success, string msg, PackConfigRecord pack = null)
    {
        OnPaymentCompleted?.Invoke(new PaymentResult
        {
            success = success,
            message = msg,
            pack = pack,
            isGameplayItemPurchase = false,
            gameplayItemType = default
        });
    }

    public void TriggerGameplayItemResult(bool success, string msg, ItemType itemType)
    {
        OnPaymentCompleted?.Invoke(new PaymentResult
        {
            success = success,
            message = msg,
            pack = null,
            isGameplayItemPurchase = true,
            gameplayItemType = itemType
        });
    }

    public void TriggerResult(PaymentResult result)
    {
        OnPaymentCompleted?.Invoke(result);
    }
}
