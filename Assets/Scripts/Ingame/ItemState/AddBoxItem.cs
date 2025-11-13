using System.Runtime.InteropServices.WindowsRuntime;

public class AddBoxItem : FSMState<ItemController>, IItem
{
    private bool isHadling;
    public ItemType ItemType => ItemType.Magnet;

    public bool IsHandling => isHadling;

    
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
        throw new System.NotImplementedException();
    }
}