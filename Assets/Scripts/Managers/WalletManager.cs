using System;
using System.DataBase;
using UnityEngine;

public class WalletManager : SingletonMono<WalletManager>
{
    // Optional event: UI lắng nghe để tự update
    public Action<Currency, long> OnCurrencyUpdated;

    // ────────────────────────────────────────────────────────────────
    // QUERY
    // ────────────────────────────────────────────────────────────────

    public long Get(Currency type)
    {
        switch (type)
        {
            case Currency.Gold: return DataAPIController.instance.GetGold();
            case Currency.Ticket: return DataAPIController.instance.GetTicket();
            default:
                Debug.LogError("[Wallet] Unknown currency: " + type);
                return 0;
        }
    }

    public bool HasEnough(Currency type, long amount)
    {
        // FIX: amount âm luôn coi là hợp lệ (không cần kiểm tra)
        if (amount < 0)
        {
            Debug.LogWarning($"[Wallet] HasEnough called with negative amount={amount}. Returning true.");
            return true;
        }
        return Get(type) >= amount;
    }

    // ────────────────────────────────────────────────────────────────
    // SPEND
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Trừ tiền. KHÔNG tự kiểm tra đủ tiền hay không.
    /// Nên gọi HasEnough() trước, hoặc dùng TrySpend() cho an toàn.
    /// </summary>
    public void Spend(Currency type, long amount)
    {
        // FIX: Validate amount hợp lệ
        if (amount <= 0)
        {
            Debug.LogWarning($"[Wallet] Spend called with invalid amount={amount}. Skipped.");
            return;
        }

        long current = Get(type);

        // FIX: Log rõ ràng nếu bị overspend thay vì âm thầm clamp
        if (current < amount)
        {
            Debug.LogWarning($"[Wallet] Overspend detected! type={type}, current={current}, spend={amount}. Clamping to 0.");
        }

        long final = Math.Max(0, current - amount);
        Set(type, final);
    }

    /// <summary>
    /// Trừ tiền an toàn — kiểm tra đủ tiền trước khi trừ.
    /// Trả về true nếu thành công, false nếu không đủ.
    /// </summary>
    public bool TrySpend(Currency type, long amount, Action onSuccess = null, Action onFailed = null)
    {
        // FIX: Đổi tên callback thành onSuccess/onFailed cho rõ nghĩa
        if (amount <= 0)
        {
            Debug.LogWarning($"[Wallet] TrySpend called with invalid amount={amount}.");
            onFailed?.Invoke();
            return false;
        }

        if (!HasEnough(type, amount))
        {
            Debug.LogWarning($"[Wallet] TrySpend failed: not enough {type}. Have={Get(type)}, Need={amount}");
            onFailed?.Invoke();
            return false;
        }

        Spend(type, amount);
        onSuccess?.Invoke();
        return true;
    }

    // ────────────────────────────────────────────────────────────────
    // ADD
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Cộng tiền. amount phải > 0.
    /// </summary>
    public void Add(Currency type, long amount)
    {
        // FIX: Chặn amount âm — tránh vô tình trừ tiền
        if (amount <= 0)
        {
            Debug.LogWarning($"[Wallet] Add called with invalid amount={amount}. Skipped.");
            return;
        }

        long current = Get(type);
        Set(type, current + amount);
    }

    /// <summary>
    /// Cộng tiền an toàn — trả về true nếu thành công.
    /// </summary>
    public bool TryAdd(Currency type, long amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[Wallet] TryAdd called with invalid amount={amount}.");
            return false;
        }

        try
        {
            Add(type, amount);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Wallet] TryAdd failed: {ex}");
            return false;
        }
    }

    // ────────────────────────────────────────────────────────────────
    // SET
    // ────────────────────────────────────────────────────────────────

    public void Set(Currency type, long value)
    {
        // FIX: Không cho set giá trị âm
        if (value < 0)
        {
            Debug.LogWarning($"[Wallet] Set called with negative value={value}. Clamping to 0.");
            value = 0;
        }

        switch (type)
        {
            case Currency.Gold:
                DataAPIController.instance.SaveGold(value);
                break;

            case Currency.Ticket:
                DataAPIController.instance.SaveTicket(value);
                break;

            default:
                Debug.LogError("[Wallet] Unknown currency: " + type);
                return;
        }

        OnCurrencyUpdated?.Invoke(type, value);
    }

    // ────────────────────────────────────────────────────────────────
    // VALIDATE (dùng trước khi show UI mua hàng)
    // ────────────────────────────────────────────────────────────────

    public bool ValidatePayment(Currency type, long price)
    {
        if (price <= 0)
        {
            // Free item — luôn hợp lệ
            return true;
        }

        if (!HasEnough(type, price))
        {
            Debug.LogWarning($"[Wallet] ValidatePayment failed: not enough {type}. Have={Get(type)}, Need={price}");
            return false;
        }

        return true;
    }

    // ────────────────────────────────────────────────────────────────
    // CHECK (helper — kiểm tra nếu cộng thêm amount có âm không)
    // ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Kiểm tra nếu trừ đi amount thì có còn >= 0 không.
    /// Tên Check() dễ hiểu nhầm — dùng HasEnough() thay thế nếu có thể.
    /// </summary>
    public bool Check(Currency type, long amount)
    {
        return Get(type) - amount >= 0;
    }
}