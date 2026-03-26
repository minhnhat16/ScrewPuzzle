using System;
using Enums;

public enum RewardKind
{
    Currency,   // Gold, Ticket → fly-to-HUD
    Item        // Booster, v.v. → popup + fade
}

public struct RewardResult
{
    public RewardKind Kind;
    public ItemType ItemType;
    public int Amount;
    public string IconName;  // tên sprite để hiển thị

    /// <summary>
    /// Helper tạo nhanh từ ItemType — tự phân loại Currency hay Item.
    /// </summary>
    public static RewardResult Create(ItemType type, int amount, string iconName = null)
    {
        bool isCurrency = type == ItemType.Gold || type == ItemType.Ticket;
        return new RewardResult
        {
            Kind = isCurrency ? RewardKind.Currency : RewardKind.Item,
            ItemType = type,
            Amount = amount,
            IconName = iconName
        };
    }
}

/// <summary>
/// Event bus dùng chung — bất kỳ hệ thống nào (Mission, Shop, Daily...)
/// chỉ cần gọi RewardEvents.Fire() là xong.
/// </summary>
public static class RewardEvents
{
    public static event Action<RewardResult> OnRewardGranted;

    public static void Fire(RewardResult reward)
    {
        OnRewardGranted?.Invoke(reward);
    }

    /// <summary>Shorthand tiện lợi không cần tạo struct thủ công.</summary>
    public static void Fire(ItemType type, int amount, string iconName = null)
    {
        Fire(RewardResult.Create(type, amount, iconName));
    }
}