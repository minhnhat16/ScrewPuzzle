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
        var activeSlots = slots.ToList();
        if (activeSlots.Count == 0) return;

        float spacing = Mathf.Max(0.7f, totalWidth / (activeSlots.Count + 1));
        float startX = -spacing * (activeSlots.Count - 1) / 2f;

        for (int i = 0; i < activeSlots.Count; i++)
        {
            var slot = activeSlots[i];
            Vector3 current = slot.transform.localPosition;
            Vector3 target = new Vector3(startX + spacing * i, current.y, current.z);

            if (duration <= 0f)
            {
                slot.transform.localPosition = target;
                if (slot.screwBox != null && !slot.screwBox.IsMoving)
                    slot.screwBox.transform.position = slot.transform.position;
            }
            else
            {
                slot.transform
                    .DOLocalMoveX(target.x, duration)
                    .SetEase(_ease)
                    .OnUpdate(() =>
                    {
                        // Chỉ kéo box theo slot nếu box đã settled (không đang spawn vào)
                        if (slot.screwBox != null && !slot.screwBox.IsMoving)
                            slot.screwBox.transform.position = slot.transform.position;
                    });
            }
        }
    }

    public void MoveBoxToSlot(Box box, BoxSlot slot, Action onComplete = null)
    {
        if (box == null || slot == null) return;

        Debug.Log($"[BoxSlotLayoutService] Moving box '{box.name}' to slot '{slot.name}' at position {CalculateSlotPosition(slot)}");
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