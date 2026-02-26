using Ingame;
using System;
using UnityEngine;

public class ItemService : IItemService
{
    private readonly IBoxQueue _boxQueue;
    private readonly IArrayScrew _arrayScrew;
    private readonly IItemView _itemView;
    private readonly IInventoryService _inventory;

    public event Action<ItemType> OnItemUsed;
    public event Action<ItemType> OnItemUseFailed;

    public ItemService(
        IBoxQueue boxQueue,
        IArrayScrew arrayScrew,
        IItemView itemView,
        IInventoryService inventory)
    {
        _boxQueue = boxQueue;
        _arrayScrew = arrayScrew;
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
            Debug.LogError($"Item execution failed: {e}");
            OnItemUseFailed?.Invoke(type);
        }
    }

    public bool CanUseItem(ItemType type)
    {
        if (!_inventory.HasItem(type))
            return false;

        return type switch
        {
            ItemType.Magnet => _arrayScrew.HasAny(),
            ItemType.Breaker => _boxQueue.HasLockedBox(),
            ItemType.Drill => _arrayScrew.IsFull,
            _ => false
        };
    }

    private void ExecuteItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.Magnet:
                _arrayScrew.Clear();
                break;

            case ItemType.Breaker:
                _boxQueue.UnlockNextBox();
                break;

            case ItemType.Drill:
                _arrayScrew.AddOneHold();
                break;
        }
    }
}