using DG.Tweening;
using Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxSlotLayoutService : IBoxSlotLayoutService
{
    private readonly Ease _ease;

    public BoxSlotLayoutService(Ease ease = Ease.OutQuad)
    {
        _ease = ease;
    }

    public void AlignSlots(IReadOnlyList<BoxSlot> slots, float totalWidth, float duration = 0.3f)
    {
        if (slots == null || slots.Count == 0) return;

        var visible = slots
            .Where(s => s.isContainingBox || s.isLocked)
            .ToList();

        if (visible.Count == 0) return;

        var sorted = visible
             .OrderBy(s => s.isLocked && !s.isContainingBox ? 1 : 0)
             .ThenBy(s =>
             {
                 for (int i = 0; i < slots.Count; i++)
                     if (slots[i] == s) return i;
                 return int.MaxValue;
             })
             .ToList();

        int n = sorted.Count;
        float spacing = Mathf.Max(0.7f, totalWidth / (n + 1));
        float startX = -spacing * (n - 1) / 2f;

        for (int i = 0; i < n; i++)
        {
            var slot = sorted[i];
            float targetX = startX + spacing * i;
            Vector3 current = slot.transform.localPosition;
            float targetY = GetTopAnchoredLocalY(slot.transform, 3.5f);

            if (duration <= 0f)
            {
                slot.transform.localPosition = new Vector3(targetX, targetY, current.z);
                SyncBoxToSlot(slot);
            }
            else
            {
                slot.transform.DOKill();
                slot.transform
                    .DOLocalMove(new Vector3(targetX, targetY, current.z), duration)
                    .SetEase(_ease)
                    .OnUpdate(() => SyncBoxToSlot(slot));
            }
        }
    }

    private static float GetTopAnchoredLocalY(Transform slotTransform, float topOffset)
    {
        if (CameraMain.instance == null || CameraMain.instance.main == null)
            return slotTransform.localPosition.y;

        float targetWorldY = CameraMain.instance.GetTop() - topOffset;

        Transform parent = slotTransform.parent;
        if (parent == null) return targetWorldY;

        Vector3 worldPos = slotTransform.position;
        worldPos.y = targetWorldY;

        return parent.InverseTransformPoint(worldPos).y;
    }

    private static void SyncBoxToSlot(BoxSlot slot)
    {
        if (slot.screwBox != null && !slot.screwBox.IsMoving)
            slot.screwBox.transform.position = slot.transform.position;
    }

    public void MoveBoxToSlot(Box box, BoxSlot slot, Action onComplete = null)
    {
        if (box == null || slot == null) return;

        box.transform
            .DOMove(CalculateSlotPosition(slot), 1f)
            .SetEase(_ease)
            .OnComplete(() => onComplete?.Invoke());
    }

    public Vector3 CalculateSlotPosition(BoxSlot slot)
    {
        return slot.transform.position;
    }
}