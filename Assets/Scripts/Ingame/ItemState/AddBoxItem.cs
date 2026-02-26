using Managers;

public class AddBoxItem : FSMState<ItemController>, IItem
{
    private bool isHadling;

    public ItemType ItemType => ItemType.AddBox;

    public bool IsHandling => isHadling;

    public AddBoxItem(ItemController sys)
    {
        Setup(sys);
    }

    public void Discard()
    {
    }

    public void HandlingItem()
    {
    }

    public void Use()
    {
        sys.PlayItemEffect(
            ItemType,
            sys.transform.position,
            sys.transform.position,
            () =>
            {
                MissionManager.ins.ProcessUseItem(ItemType, 1);
                IngameController.ins.Revive();
            });
    }
}