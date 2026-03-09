using Ingame;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBoxSlotLayoutService
{
    /// <summary>
    /// Tái căn chỉnh vị trí các slot active (không bị lock) theo spacing đều,
    /// sau đó tween box trong mỗi slot về đúng vị trí slot của nó.
    /// </summary>
    void AlignSlots(IReadOnlyList<BoxSlot> slots, float totalWidth, float duration = 0.3f);

    /// <summary>
    /// Tween một box cụ thể đến vị trí slot chỉ định.
    /// </summary>
    void MoveBoxToSlot(Box box, BoxSlot slot, Action onComplete = null);

    Vector3 CalculateSlotPosition(BoxSlot slot);
}