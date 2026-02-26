using ConfigFile;
using Core.Match;
using Enums;
using Ingame;
using Ingame.Screw;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// BoxQueue sau khi update:
/// - Implement IContainerQueue (Core layer)
/// - Giữ nguyên IBoxQueue cũ để không break code hiện có
/// - Thêm FindSuitableBox(string tag) overload
/// - Fix AddScrewToBox trả về bool
/// </summary>
public class BoxQueue : MonoBehaviour, IContainerQueue
{
    private SideMission _currentMission;
    public bool HasSpecialBox => _currentMission != null;

    // ─────────────────────────────────────────
    // IBoxQueue + IContainerQueue shared state
    // ─────────────────────────────────────────

    public int ActiveBoxCount => _activeBoxes.Count;
    public int ActiveCount => _activeBoxes.Count;
    public bool HaseMovingContainer => _activeBoxes.Any(b => b.IsMoving);

    private IBoxFactory _factory;
    private IBoxSequenceService _sequence;
    private IBoxSlotLayoutService _layout;

    private List<Box> _activeBoxes = new();
    [SerializeField] private List<BoxSlot> slots;
    private readonly Dictionary<ColorEnum, List<ScrewController>> _hiddenByColor = new();

    // ─────────────────────────────────────────
    // Events — IBoxQueue
    // ─────────────────────────────────────────

    public event Action<Box> OnBoxFull;
    public event Action<Box> OnBoxSpawned;
    public event Action<Box> OnBoxRemoved;
    public event Action<SideMission> OnSpecialModeStarted;

    // Events — IContainerQueue (relay từ Box events)
    event Action<IMatchContainer> IContainerQueue.OnContainerCompleted
    {
        add => _onContainerCompleted += value;
        remove => _onContainerCompleted -= value;
    }
    event Action<IMatchContainer> IContainerQueue.OnContainerSpawned
    {
        add => _onContainerSpawned += value;
        remove => _onContainerSpawned -= value;
    }
    event Action<IMatchContainer> IContainerQueue.OnContainerRemoved
    {
        add => _onContainerRemoved += value;
        remove => _onContainerRemoved -= value;
    }

    private event Action<IMatchContainer> _onContainerCompleted;
    private event Action<IMatchContainer> _onContainerSpawned;
    private event Action<IMatchContainer> _onContainerRemoved;

    // ─────────────────────────────────────────
    // Setup
    // ─────────────────────────────────────────

    public void Setup(IBoxFactory factory, IBoxSequenceService sequence, IBoxSlotLayoutService layout)
    {
        _factory = factory;
        _sequence = sequence;
        _layout = layout;
    }

    public void LoadLevelBoxes(IEnumerable<BoxConfigRecord> records)
    {
        if (_factory == null) throw new Exception("BoxQueue not setup");

        var boxes = _factory.CreateBoxes(records);
        _sequence.Load(boxes);

        foreach (var box in boxes)
            box.OnBoxFull += NotifyBoxFull;
    }

    // ─────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────

    public void Initialize(bool isTutorial) => SpawnInitial();
    void IContainerQueue.Initialize(bool isTutorial) => Initialize(isTutorial);

    public void ResetQueue()
    {
        foreach (var box in _activeBoxes)
            box.OnBoxFull -= NotifyBoxFull;
        _activeBoxes.Clear();
    }
    void IContainerQueue.Reset() => ResetQueue();

    private void SpawnInitial()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!_sequence.HasNext()) break;
            ActivateBox(_sequence.GetNext());
        }
        _layout.AlignBoxes(_activeBoxes, slots);
    }

    // ─────────────────────────────────────────
    // FindSuitableBox — 2 overloads
    // ─────────────────────────────────────────

    /// <summary>IBoxQueue — dùng ColorEnum (backward compat)</summary>
    public Box FindSuitableBox(ColorEnum color)
    {
        return _activeBoxes.FirstOrDefault(b =>
            !b.IsLocked && !b.IsFull && !b.IsMoving &&
            (b.Color == color || b.Color == ColorEnum.Rainbow));
    }

    /// <summary>IContainerQueue — dùng string tag (Core layer)</summary>
    public Box FindSuitableBox(string tag)
    {
        return _activeBoxes.FirstOrDefault(b =>
            !b.IsLocked && !b.IsFull && !b.IsMoving &&
            (b.Color.ToString().ToLower() == tag || b.Color == ColorEnum.Rainbow));
    }

    IMatchContainer IContainerQueue.FindSuitable(string tag)
    {
        var box = FindSuitableBox(tag);
        return box; // Box implement IMatchContainer trực tiếp
    }

    // ─────────────────────────────────────────
    // AddScrewToBox — fix trả về bool
    // ─────────────────────────────────────────

    public bool AddScrewToBox(ScrewController screw, Box box)
    {
        if (screw == null || box == null) return false;
        if (box.IsLocked || box.IsFull) return false;
        return box.TryAddScrew(screw);
    }

    bool IContainerQueue.AddItemToContainer(IMatchItem item, IMatchContainer container)
    {
        if (item is not ScrewController screw) return false;
        if (container is not Box box) return false;
        return AddScrewToBox(screw, box);
    }

    // ─────────────────────────────────────────
    // Box lifecycle
    // ─────────────────────────────────────────

    public void NotifyBoxFull(Box box)
    {
        if (!_activeBoxes.Contains(box)) return;

        _activeBoxes.Remove(box);
        OnBoxFull?.Invoke(box);
        _onContainerCompleted?.Invoke(box);

        TrySpawnNext();
        _layout.AlignBoxes(_activeBoxes, slots);
    }

    void IContainerQueue.NotifyCompleted(IMatchContainer container)
    {
        if (container is Box box) NotifyBoxFull(box);
    }

    private void ActivateBox(Box box)
    {
        _activeBoxes.Add(box);
        OnBoxSpawned?.Invoke(box);
        _onContainerSpawned?.Invoke(box);
        TryResolveHiddenForBox(box);
    }

    private void TrySpawnNext()
    {
        if (!_sequence.HasNext()) return;
        ActivateBox(_sequence.GetNext());
    }

    // ─────────────────────────────────────────
    // Các method còn lại — giữ nguyên
    // ─────────────────────────────────────────

    public void UnlockNextBox() { /* TODO */ }
    bool IContainerQueue.HasLocked() => HasLockedBox();
    public bool HasLockedBox() => _activeBoxes.Any(b => b.IsLocked);

    public void EnableSpecialMode(SideMission mission)
    {
        if (mission == null) return;
        _currentMission = mission;
        OnSpecialModeStarted?.Invoke(mission);
    }

    public void RemoveBoxByColor(ColorEnum targetColor, int count)
    {
        if (count <= 0) return;

        var toRemove = _activeBoxes.Where(b => b.Color == targetColor).Take(count).ToList();
        foreach (var box in toRemove) RemoveBoxInternal(box);

        FillToSlotCapacity();
        _layout.AlignBoxes(_activeBoxes, slots);
    }

    public void ProcessScrews(IEnumerable<ScrewController> screws)
    {
        if (screws == null) return;

        foreach (var group in screws.Where(s => s != null).GroupBy(s => s.GetColor()))
        {
            var box = FindSuitableBox(group.Key);
            if (box != null) box.TryAddScrews(group.ToList());
        }
    }

    public void TryProcessItemScrew(ScrewController screw)
    {
        var box = FindSuitableBox(screw.GetColor());
        if (box == null) { HideScrew(screw); return; }
        AddScrewToBox(screw, box);
    }

    private void RemoveBoxInternal(Box box)
    {
        if (!_activeBoxes.Contains(box)) return;
        _activeBoxes.Remove(box);
        box.OnBoxFull -= NotifyBoxFull;
        OnBoxRemoved?.Invoke(box);
        _onContainerRemoved?.Invoke(box);
    }

    private void FillToSlotCapacity()
    {
        while (_activeBoxes.Count < slots.Count && _sequence.HasNext())
            ActivateBox(_sequence.GetNext());
    }

    private void HideScrew(ScrewController screw)
    {
        var color = screw.GetColor();
        if (!_hiddenByColor.ContainsKey(color))
            _hiddenByColor[color] = new List<ScrewController>();
        screw.SetActive(false);
        _hiddenByColor[color].Add(screw);
    }

    private void TryResolveHiddenForBox(Box box)
    {
        var color = box.Color;
        if (!_hiddenByColor.ContainsKey(color)) return;

        var hiddenList = _hiddenByColor[color];
        var copy = hiddenList.ToList();

        foreach (var screw in copy)
        {
            if (box.IsFull) break;
            hiddenList.Remove(screw);
            screw.SetActive(true);
            box.TryAddScrew(screw);
        }

        if (hiddenList.Count == 0) _hiddenByColor.Remove(color);
    }

    public void UnlockNext()
    {
    }
}