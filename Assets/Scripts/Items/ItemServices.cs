using Core.Match;
using System;
using UnityEngine;

public class ItemService : IItemService
{
    private readonly IContainerQueue _containerQueue;
    private readonly ITempQueue _tempQueue;
    private readonly IItemView _itemView;
    private readonly IInventoryService _inventory;

    public event Action<ItemType> OnItemUsed;
    public event Action<ItemType> OnItemUseFailed;

    public ItemService(
        IContainerQueue containerQueue,
        ITempQueue tempQueue,
        IItemView itemView,
        IInventoryService inventory)
    {
        _containerQueue = containerQueue;
        _tempQueue = tempQueue;
        _itemView = itemView;
        _inventory = inventory;
    }

    public void UseItem(ItemType type, Vector3 targetPos)
    {
        if (!CanUseItem(type))
        {
            OnItemUseFailed?.Invoke(type);
            return;
        }

        try
        {
            ExecuteItem(type);
            _inventory.Consume(type);
            _itemView.PlayItemEffect(type, Vector3.zero, targetPos, () =>
            {
                OnItemUsed?.Invoke(type);
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"[ItemService] Lỗi khi dùng item {type}: {e}");
            OnItemUseFailed?.Invoke(type);
        }
    }

    public bool CanUseItem(ItemType type)
    {
        if (!_inventory.HasItem(type)) return false;

        return type switch
        {
            ItemType.Magnet => _tempQueue.HasAny,          // còn screw trong array
            ItemType.Breaker => _containerQueue.HasLocked(), // có box bị lock
            ItemType.Drill => _tempQueue.IsFull,           // array đang full
            _ => false
        };
    }

    private void ExecuteItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.Magnet:
                _tempQueue.Clear();
                break;

            case ItemType.Breaker:
                break;

            case ItemType.Drill:
                _tempQueue.AddSlot();
                break;
        }
    }
}