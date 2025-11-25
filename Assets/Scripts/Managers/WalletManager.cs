using System.DataBase;
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

    // OPTIONAL EVENT: Update UI automatically
    public System.Action<Currency, long> OnCurrencyUpdated;
}
