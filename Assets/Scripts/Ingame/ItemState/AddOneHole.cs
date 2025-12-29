using Ingame;
using System;

public class AddOneHold : FSMState<ItemController>
{

    public AddOneHold(ItemController sys)
    {
        Setup(sys);
    }

    internal void Use(Action callback= null)
    {
        MissionManager.ins.ProcessUseItem(ItemType.Drill, 1);
        ArrayScrew.Instance.SpawnNewHold();
        callback?.Invoke();
        sys.WaitFor(1, () => {
            sys.itemPerformed?.Invoke(true);
        });
    }
}