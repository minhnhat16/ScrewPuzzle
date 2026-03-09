using Ingame;
using Managers;
using UnityEngine;

public class AddOneHold : FSMState<ItemController>, IItem
{
    public ItemType ItemType => ItemType.Drill;
    public bool IsHandling => sys.IsItemExecuting;

    public AddOneHold(ItemController sys)
    {
        Setup(sys);
    }

    public void Use(Vector3 targetPos = default)
    {
        if (!sys.IsItemSelected || sys.IsItemExecuting) return;
        sys.SetExecuting(true);

        sys.PlayItemEffect(
            ItemType.Drill,
            sys.transform.position,
            targetPos,
            () =>
            {
                MissionManager.ins.ProcessUseItem(ItemType.Drill, 1);
                ArrayScrew.ins.AddSlot();
                sys.SetExecuting(false);
                sys.SetSelected(false);

                // Auto item → invoke ngay sau khi effect xong
                sys.itemPerformed.Invoke(true);

                IngameController.ins.OnItemFinished();
            });
    }

    public void HandlingItem() { }

    public void Discard()
    {
        sys.SetExecuting(false);
        sys.SetSelected(false);
        sys.itemPerformed.Invoke(false);
        IngameController.ins.OnItemFinished();
    }
}