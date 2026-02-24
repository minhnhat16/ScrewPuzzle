using System;
using UnityEngine;

public interface IItemService
{
    /// <summary>
    /// Kiểm tra có thể sử dụng item hay không
    /// </summary>
    bool CanUseItem(ItemType type);

    /// <summary>
    /// Sử dụng item tại vị trí chỉ định
    /// </summary>
    void UseItem(ItemType type, Vector3 worldPosition);

    /// <summary>
    /// Sự kiện khi item được sử dụng thành công
    /// </summary>
    event Action<ItemType> OnItemUsed;

    /// <summary>
    /// Sự kiện khi item bị fail (không đủ điều kiện)
    /// </summary>
    event Action<ItemType> OnItemUseFailed;
}