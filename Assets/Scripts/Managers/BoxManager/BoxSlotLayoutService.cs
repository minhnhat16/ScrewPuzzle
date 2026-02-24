using DG.Tweening;
using Ingame;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BoxSlotLayoutService : IBoxSlotLayoutService
{
    private readonly float _moveDuration;
    private readonly Ease _ease;

    public BoxSlotLayoutService(float moveDuration = 0.3f, Ease ease = Ease.OutQuad)
    {
        _moveDuration = moveDuration;
        _ease = ease;
    }

    public void AlignBoxes(IReadOnlyList<Box> boxes, IReadOnlyList<BoxSlot> slots)
    {
        int count = Mathf.Min(boxes.Count, slots.Count);

        for (int i = 0; i < count; i++)
        {
            MoveBoxToSlot(boxes[i], slots[i]);
        }
    }

    public void MoveBoxToSlot(Box box, BoxSlot slot, Action onComplete = null)
    {
        if (box == null || slot == null)
            return;

        box.isMoving = true;

        box.transform
            .DOMove(CalculateSlotPosition(slot), _moveDuration)
            .SetEase(_ease)
            .OnComplete(() =>
            {
                box.isMoving = false;
                onComplete?.Invoke();
            });
    }

    public Vector3 CalculateSlotPosition(BoxSlot slot)
    {
        return slot.transform.position;
    }
}