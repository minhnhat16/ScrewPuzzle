using Ingame;
using System;

public class RemovePartState : FSMState<ItemController>
{
    private ItemController itemController;

    public RemovePartState(ItemController itemController)
    {
        Setup(itemController);
    }

    public void Use()
    {
        sys.IsHandlingHammer = true;
    }
    internal void Peform(BasePart part)
    {
        LevelManager.ins.RemovePartItem(part);
        sys.IsHandlingHammer = false;
        sys.itemPerformed?.Invoke(true);
    }
}