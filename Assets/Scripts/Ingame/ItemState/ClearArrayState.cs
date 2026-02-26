using Ingame;
using Managers;
using System;
using UnityEngine;

public class ClearArrayState : FSMState<ItemController>
{

    public ClearArrayState(ItemController itemController)
    {
        Setup(itemController);
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
                MissionManager.ins.ProcessUseItem(ItemType.Magnet, 1);
                ArrayScrew.ins.Clear ();
                sys.SetExecuting(false);
                sys.SetSelected(false);
                callback?.Invoke();
            });


    }
}