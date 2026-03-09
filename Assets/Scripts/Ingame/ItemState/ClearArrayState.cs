using Ingame;
using Managers;
using System.Collections;
using UnityEngine;

public class ClearArrayState : FSMState<ItemController>, IItem
{
    public ItemType ItemType => ItemType.Magnet;
    public bool IsHandling => sys.IsItemExecuting;

    public ClearArrayState(ItemController itemController)
    {
        Setup(itemController);
    }

    public void Use(Vector3 targetPos = default)
    {
        if (!sys.IsItemSelected || sys.IsItemExecuting) return;
        sys.SetExecuting(true);

        sys.PlayItemEffect(
            ItemType.Magnet,
            sys.transform.position,
            targetPos,
            () => sys.StartCoroutine(ClearAndResolve()));
    }

    /// <summary>
    /// Đợi ClearToHidden() xong → screws đã vào ScrewManager
    /// → mới notify item finished → BoxQueue spawn box mới → resolve đúng
    /// </summary>
    private IEnumerator ClearAndResolve()
    {
        // Chờ tất cả screw move vào hidden storage xong
        yield return ArrayScrew.ins.ClearToHidden();

        MissionManager.ins.ProcessUseItem(ItemType.Magnet, 1);

        sys.SetExecuting(false);
        sys.SetSelected(false);
        sys.itemPerformed.Invoke(true);

        // OnItemFinished → TransitionTo(Playing) → event chain
        // Lúc này ScrewManager đã có hidden screws → ResolveAllHiddenForBox sẽ hit ✅
        IngameController.ins.OnItemFinished();
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