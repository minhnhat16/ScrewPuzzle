
public interface IItem
{
    ItemType ItemType { get; }
    bool IsHandling { get; }

    void HandlingItem();
    void Use();
    void Discard();
}

