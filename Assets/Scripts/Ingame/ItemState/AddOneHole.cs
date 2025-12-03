using Ingame;
using Mono.Cecil;
using System;

public class AddOneHold : FSMState<ItemController>
{

    public AddOneHold(ItemController sys)
    {
        Setup(sys);
    }

    internal void Use(Action callback= null)
    {
        ArrayScrew.Instance.SpawnNewHold();
        callback?.Invoke();
        sys.WaitFor(1, () => {
            sys.itemPerformed?.Invoke(true);
        });
    }
}