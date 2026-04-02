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
    }

    internal void Perform(BasePart part, Vector3 startPos)
    {
        if (!sys.IsItemSelected || sys.IsItemExecuting) return;

        // Part đang ở layer hidden hoặc prereview → không thể break
        if (part == null || !part.IsBreakableByItem)
        {
            Debug.Log($"[RemovePartState] Perform blocked — part '{part?.uniqueID}' is not breakable.");
            return;
        }

        sys.SetExecuting(true);

        sys.PlayItemEffect(
            ItemType.Breaker,
            startPos,
            part.Transform.position,
            () =>
            {
                MissionManager.ins.ProcessUseItem(ItemType.Breaker, 1);
                LevelManager.ins.RemovePartItem(part);

                var boxQueue = BoxQueue.ins;
                if (boxQueue != null)
                {
                    foreach (var box in boxQueue.GetActiveBoxes())
                    {
                        if (!box.IsFull && !box.IsLocked && box.RemainingCapacity > 0)
                            boxQueue.ResolveAllHiddenForBox(box);
                    }
                }
                
                // MỞ KHOÁ SÂN CHƠI: Nếu ArrayScrew đang cấm người chơi bấm vì lý do gì, ta báo nó nhận định lại
                if (ArrayScrew.ins != null)
                {
                    // Lợi dụng việc SetGameActive(true) hoặc gọi Evaluate ngay lập tức
                    // Đơn giản nhất là mở khoá input trước, rồi Request Evaluator.
                    var p = Object.FindAnyObjectByType<Ingame.Player>();
                    p?.UnlockInput();

                    // Force lại evaluator nếu như lượng khay được giải phóng
                    ArrayScrew.ins.SetGameActive(true); 
                }

                sys.SetExecuting(false);
                sys.SetSelected(false);
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