using Ingame;
using Managers;
using System;

public class ClearArrayState : FSMState<ItemController>
{

    public ClearArrayState(ItemController itemController)
    {
        Setup(itemController);
    }

    internal void Use(Action callback = null)
    {
        MissionManager.ins.ProcessUseItem(ItemType.Magnet, 1);

        ArrayScrew.Instance.StartClearHiding();
        callback?.Invoke();
        sys.WaitFor(1, () => {
            sys.itemPerformed?.Invoke(true);
        });
    }
}