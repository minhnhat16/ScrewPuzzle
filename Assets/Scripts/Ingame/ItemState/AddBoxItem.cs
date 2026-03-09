using Managers;
using UnityEngine;

public class AddBoxItem : FSMState<ItemController>, IItem
{
    public ItemType ItemType => ItemType.AddBox;
    public bool IsHandling => sys.IsItemExecuting;

    public AddBoxItem(ItemController sys)
    {
        Setup(sys);
    }

    public void Use(Vector3 targetPos = default)
    {
        if (sys.IsItemExecuting) return;
        sys.SetExecuting(true);

        sys.PlayItemEffect(
            ItemType.AddBox,
            sys.transform.position,
            sys.transform.position,
            () =>
            {
                MissionManager.ins.ProcessUseItem(ItemType, 1);
                BoxQueue.ins.UnlockNextSlot();
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
        sys.itemPerformed.Invoke(false);
        IngameController.ins.OnItemFinished();
    }
}