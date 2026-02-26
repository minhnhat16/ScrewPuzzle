public interface IInventoryService
{
    bool HasItem(ItemType type);
    void Consume(ItemType type);
}