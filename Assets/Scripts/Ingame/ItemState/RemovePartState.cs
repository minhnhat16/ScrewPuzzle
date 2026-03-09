using Ingame;
using Managers;
using UnityEngine;

public class RemovePartState : FSMState<ItemController>, IItem
{
    public ItemType ItemType => ItemType.Breaker;
    public bool IsHandling => sys.IsItemExecuting;

    public RemovePartState(ItemController itemController)
    {
        Setup(itemController);
    }

    public void Use(Vector3 targetPos = default)
    {
        if (sys.IsItemExecuting) return;
        sys.SetSelected(true);
        // Manual item — KHÔNG invoke itemPerformed ở đây
        // Chờ player tap part → Perform()
    }

    internal void Perform(BasePart part, Vector3 startPos)
    {
        if (!sys.IsItemSelected || sys.IsItemExecuting) return;
        sys.SetExecuting(true);

        sys.PlayItemEffect(
            ItemType.Breaker,
            startPos,
            part.Transform.position,
            () =>
            {
                MissionManager.ins.ProcessUseItem(ItemType.Breaker, 1);
                LevelManager.ins.RemovePartItem(part);
                sys.SetExecuting(false);
                sys.SetSelected(false);

                // Manual item → invoke SAU KHI player đã perform xong
                sys.itemPerformed.Invoke(true);

                IngameController.ins.OnItemFinished();
            });
    }

    public void HandlingItem() { }
    public void Discard()
    {
        sys.SetExecuting(false);
        sys.SetSelected(false);
        // Cancel mà không perform → cũng hide description
        sys.itemPerformed.Invoke(false);
        IngameController.ins.OnItemFinished();
    }
}