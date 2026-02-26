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
        if (!sys.IsItemSelected || sys.IsItemExecuting) return;
        sys.SetExecuting(true);
        sys.PlayItemEffect(
            ItemType.Breaker,
            Vector3.zero,
            targetPos,
            () =>
            {
                MissionManager.ins.ProcessUseItem(ItemType.Drill, 1);
                ArrayScrew.ins.AddSlot ();
                sys.SetExecuting(false);
                sys.SetSelected(false);
                callback?.Invoke();
            });

    }


}