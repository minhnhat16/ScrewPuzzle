using System;
using System.DataBase;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

public class WalletManager : SingletonMono<WalletManager>
{
    public bool HasEnough(Currency type, long amount)
    {
        return Get(type) >= amount;
    }

    public void Spend(Currency type, long amount)
    {
        long current = Get(type);
        long final = (long)Mathf.Max(0, current - amount);

        Set(type, final);
    }

    public void Add(Currency type, long amount)
    {
        long current = Get(type);
        long final = current + amount;

        Set(type, final);
    }
    public bool Check(Currency type, long amount)
    {
        long current = Get(type);
        long final = current + amount;
        return final >= 0;
    }
    public long Get(Currency type)
    {
        switch (type)
        {
            case Currency.Gold:
                return DataAPIController.instance.GetGold();

            case Currency.Ticket:
                return DataAPIController.instance.GetTicket();

            default:
                Debug.LogError("Unknown currency: " + type);
                return 0;
        }
    }

    public void Set(Currency type, long value)
    {
        switch (type)
        {
            case Currency.Gold:
                DataAPIController.instance.SaveGold(value);
                break;

            case Currency.Ticket:
                DataAPIController.instance.SaveTicket(value);
                break;
         

            default:
                Debug.LogError("Unknown currency: " + type);
                break;
        }

        // Optional UI update
        OnCurrencyUpdated?.Invoke(type, value);
    }
    public bool TrySpend(Currency type, long amount, Action callback = null)
    {
        try
        {
            if (!HasEnough(type, amount))
                return false;

            Spend(type, amount);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Wallet] TrySpend failed: {ex}");
            callback?.Invoke();
            return false;
        }
    }
    public bool TryAdd(Currency type, long amount)
    {
        try
        {
            Add(type, amount);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Wallet] TryAdd failed: {ex}");
            return false;
        }
    }
    public bool ValidatePayment(Currency type, long price)
    {
        if (!HasEnough(type, price))
        {
            Debug.LogWarning("[Wallet] Not enough currency.");
            return false;
        }

        return true;
    }



    // OPTIONAL EVENT: Update UI automatically
    public System.Action<Currency, long> OnCurrencyUpdated;
}
