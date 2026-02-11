using Ingame;
using UnityEngine;

public class RemovePartState : FSMState<ItemController>
{
    private ItemController itemController;
    private Vector3 targetPos;
    public RemovePartState(ItemController itemController)
    {
        Setup(itemController);
    }

    public void Use(Vector3 targetPos)
    {
        sys.IsHandlingHammer = true;
        this.targetPos = targetPos;
    }
    internal void Peform(BasePart part,Vector3 target)
    {
        sys.PlaySkeAnimOnTarget(ItemType.Breaker, target, target, () =>
        {
            SoundHelper.PlaySFX(SoundManager.SFX.Breaker);
            MissionManager.ins.ProcessUseItem(ItemType.Breaker, 1);
            LevelManager.ins.RemovePartItem(part);
            sys.IsHandlingHammer = false;
            sys.itemPerformed?.Invoke(true);
        });

    }
}