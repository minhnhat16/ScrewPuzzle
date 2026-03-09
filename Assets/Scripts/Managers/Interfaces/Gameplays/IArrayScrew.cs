using Enums;
using Ingame.Screw;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IArrayScrew
{
    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    int ActiveHoldCount { get; }
    bool IsFull { get; }
    bool HasAny();

    // ─────────────────────────────────────────
    // Events
    // ─────────────────────────────────────────

    event Action OnArrayFull;

    // ─────────────────────────────────────────
    // Screw operations
    // ─────────────────────────────────────────

    void AddScrew(ScrewController screw);
    void RemoveScrew(ScrewController screw);
    void RemoveScrews(IEnumerable<ScrewController> screws);
    void Clear();
    IEnumerator ClearToHidden();

    // ─────────────────────────────────────────
    // Hold operations
    // ─────────────────────────────────────────

    void AddOneHold();
    void ShowArrayActive(int activeCount);

    // ─────────────────────────────────────────
    // Queries
    // ─────────────────────────────────────────

    ColorEnum GetDominantColor();
    UnityEngine.Vector3 GetLastHoldPosition();
    List<ScrewController> TakeByColor(ColorEnum color, int maxCount);
    HashSet<ColorEnum> GetHeldColors();
    Dictionary<ColorEnum, int> GetHeldColorCounts();
}