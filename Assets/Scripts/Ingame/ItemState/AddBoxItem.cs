using Managers;

public class AddBoxItem : FSMState<ItemController>, IItem
{
    private bool isHadling;
    public ItemType ItemType => ItemType.Magnet;

    public bool IsHandling => isHadling;

    public AddBoxItem(ItemController sys)
    {
        Setup(sys);
    }
    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public void Discard()
    {
    }

    public void HandlingItem()
    {
    }

    public void Use()
    {
        sys.IsHandlingHammer = false;
        MissionManager.ins.ProcessUseItem(ItemType, 1);
        IngameController.ins.OnRevive();
        sys.itemPerformed?.Invoke(true);
    }
}