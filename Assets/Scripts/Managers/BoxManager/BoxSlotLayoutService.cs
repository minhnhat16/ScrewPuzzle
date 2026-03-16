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

        // ── Lọc slot cần hiển thị: có box HOẶC đang locked ──
        // Slot trống (unlocked, không box) → bỏ qua hoàn toàn
        var visible = slots
            .Where(s => s.isContainingBox || s.isLocked)
            .ToList();

        if (visible.Count == 0) return;

        // ── Sắp xếp: slot có box trước, locked slot sau ──
        // Giữ thứ tự gốc trong từng nhóm (theo index trong danh sách slots gốc)
        var sorted = visible
             .OrderBy(s => s.isLocked && !s.isContainingBox ? 1 : 0)   // box slots first
             .ThenBy(s =>
             {
                 for (int i = 0; i < slots.Count; i++)
                     if (slots[i] == s) return i;
                 return int.MaxValue;
             })                                                          // preserve original order
             .ToList();

        int n = sorted.Count;
        float spacing = Mathf.Max(0.7f, totalWidth / (n + 1));
        float startX = -spacing * (n - 1) / 2f;

        for (int i = 0; i < n; i++)
        {
            var slot = sorted[i];
            float targetX = startX + spacing * i;
            Vector3 current = slot.transform.localPosition;

            if (duration <= 0f)
            {
                slot.transform.localPosition = new Vector3(targetX, current.y, current.z);
                SyncBoxToSlot(slot);
            }
            else
            {
                slot.transform.DOKill();
                slot.transform
                    .DOLocalMoveX(targetX, duration)
                    .SetEase(_ease)
                    .OnUpdate(() => SyncBoxToSlot(slot));
            }
        }
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