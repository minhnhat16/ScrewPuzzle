using Ingame;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBoxSlotLayoutService
{
    void AlignBoxes(IReadOnlyList<Box> boxes, IReadOnlyList<BoxSlot> slots);
    void MoveBoxToSlot(Box box, BoxSlot slot, Action onComplete = null);
    Vector3 CalculateSlotPosition(BoxSlot slot);
}