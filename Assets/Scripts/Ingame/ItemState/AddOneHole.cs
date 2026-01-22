using Ingame;
using System;
using UnityEngine;

public class AddOneHold : FSMState<ItemController>
{

    public AddOneHold(ItemController sys)
    {
        Setup(sys);
    }

    internal void Use(Vector3 targetPos, Action callback = null)
    {
        sys.IsHandlingHammer = false;
        sys.PlaySkeAnimOnTarget(ItemType.Drill,new UnityEngine.Vector3(0,-10,0), targetPos, () =>
        {
            MissionManager.ins.ProcessUseItem(ItemType.Drill, 1);
            ArrayScrew.Instance.SpawnNewHold();
            sys.WaitFor(1, () => {
                sys.itemPerformed?.Invoke(true);
            });
            callback?.Invoke();
        });
       
    }

  
}