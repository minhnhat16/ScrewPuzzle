using Ingame;
using UnityEngine;

public class RemovePartState : FSMState<ItemController>
{
    private Vector3 targetPos;

    public RemovePartState(ItemController itemController)
    {
        Setup(itemController);
    }

    public void Use()
    {
        if (sys.IsItemExecuting) return;

        sys.SetSelected(true);
    }

    internal void Perform(BasePart part, Vector3 startPos)
    {
        if (!sys.IsItemSelected || sys.IsItemExecuting) return;

        sys.SetExecuting(true);

        sys.PlayItemEffect(
            ItemType.Breaker,
            startPos,
            targetPos,
            () =>
            {
                MissionManager.ins.ProcessUseItem(ItemType.Breaker, 1);
                LevelManager.ins.RemovePartItem(part);

                sys.SetExecuting(false);
                sys.SetSelected(false);
            });
    }
}