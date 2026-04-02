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

        // Tạo cờ để đợi ClearToHidden
        bool isCleared = false;
        
        // Gọi thẳng ClearToHidden để tái sử dụng luồng an toàn quản lý xoá array
        yield return ArrayScrew.ins.StartCoroutine(ArrayScrew.ins.ClearToHidden((success) => 
        {
            isCleared = success;
        }));

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