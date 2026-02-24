public class ItemService : IItemService
{
    private readonly IBoxQueue _boxQueue;
    private readonly IArrayScrew _arrayScrew;
    private readonly IItemView _itemView;

    public ItemService(
        IBoxQueue boxQueue,
        IArrayScrew arrayScrew,
        IItemView itemView)
    {
        _boxQueue = boxQueue;
        _arrayScrew = arrayScrew;
        _itemView = itemView;
    }

    public void UseItem(ItemType type, Vector3 pos)
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
                // logic
                break;
        }

        _itemView.PlayItemEffect(type, Vector3.zero, pos);
    }
}