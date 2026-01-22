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
        sys.IsHandlingHammer = false;
        sys.PlaySkeAnimOnTarget(ItemType.Magnet, new UnityEngine.Vector3(0, -5, 0), targetPos, () =>
        {
            MissionManager.ins.ProcessUseItem(ItemType.Magnet, 1);

            ArrayScrew.Instance.StartClearHiding();
            sys.WaitFor(1, () =>
            {
                sys.itemPerformed?.Invoke(true);
            });
            callback?.Invoke();
        });
    }
}