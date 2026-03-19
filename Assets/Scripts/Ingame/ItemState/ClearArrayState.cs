// Assets\Scripts\Ingame\ItemState\ClearArrayState.cs
using Enums;
using Ingame;
using Ingame.Screw;
using Managers;
using System.Collections;
using System.Collections.Generic;
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
        int itemUsed = 0;

        // Lấy toàn bộ screw trong array
        var arrayScrews = new List<ScrewController>(ArrayScrew.ins.HeldScrews);
        if (arrayScrews.Count == 0)
        {
            MissionManager.ins.ProcessUseItem(ItemType.Magnet, 0);
            sys.SetExecuting(false);
            sys.SetSelected(false);
            sys.itemPerformed.Invoke(true);
            IngameController.ins.OnItemFinished();
            yield break;
        }

        // Ẩn visual từng screw
        foreach (var screw in arrayScrews)
        {
            if (screw == null) continue;
            screw.SetActive(false);
            yield return null;
        }

        // Xử lý từng screw như RemovePart: Rainbow → SpecialBox, match box → add, else → hidden
        var hidden = new List<ScrewController>();
        foreach (var screw in arrayScrews)
        {
            if (screw == null) continue;

            // Remove from ArrayScrew
            ArrayScrew.ins.Dequeue(screw);

            // Rainbow: chuyển vào SpecialBoxManager
            if (screw.GetColor() == ColorEnum.Rainbow)
            {
                SpecialBoxManager.ins.AddSingle(screw);
                continue;
            }

            // Route vào box nếu có
            var box = BoxQueue.ins.FindSuitableBox(screw.GetColor());
            if (box != null && box.TryAddScrew(screw))
            {
                continue;
            }

            // Không match box → add vào hidden
            hidden.Add(screw);
        }

        // Add hidden screws vào ScrewManager
        if (hidden.Count > 0)
        {
            LevelManager.ins.layerManager.RemoveScrewsOnDict(hidden);
            LevelManager.ins.ScrewManager.AddHiddenScrews(hidden);
        }

        MissionManager.ins.ProcessUseItem(ItemType.Magnet, 1);

        sys.SetExecuting(false);
        sys.SetSelected(false);
        sys.itemPerformed.Invoke(true);
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