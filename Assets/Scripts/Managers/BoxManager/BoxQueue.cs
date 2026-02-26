using ConfigFile;
using Enums;
using Ingame;
using Ingame.Screw;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxQueue : MonoBehaviour, IBoxQueue
{

    private SideMission _currentMission;

    public bool HasSpecialBox => _currentMission != null;

    public int ActiveBoxCount => _activeBoxes.Count;

    private IBoxFactory _factory;
    private IBoxSequenceService _sequence;
    private IBoxSlotLayoutService _layout;

    private List<Box> _activeBoxes = new();
    [SerializeField] private List<BoxSlot> slots;
    private readonly Dictionary<ColorEnum, List<ScrewController>> _hiddenByColor
    = new();

    public bool hasMovingBox => _activeBoxes.Any(b => b.isMoving);
    public event Action<Box> OnBoxFull;
    public event Action<Box> OnBoxSpawned;
    public event Action<Box> OnBoxRemoved;
    public event Action<SideMission> OnSpecialModeStarted;

    public void Setup(
    IBoxFactory factory,
    IBoxSequenceService sequence,
    IBoxSlotLayoutService layout)
    {
        _factory = factory;
        _sequence = sequence;
        _layout = layout;
    }
    public void LoadLevelBoxes(IEnumerable<BoxConfigRecord> records)
    {
        if (_factory == null)
            throw new Exception("BoxQueue not setup");

        var boxes = _factory.CreateBoxes(records);

        _sequence.Load(boxes);

        foreach (var box in boxes)
        {
            box.OnBoxFull += NotifyBoxFull;
        }
    }
    public void Initialize(bool isTutorial)
    {
        SpawnInitial();
    }

    private void SpawnInitial()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!_sequence.HasNext())
                break;

            var box = _sequence.GetNext();
            ActivateBox(box);
        }

        _layout.AlignBoxes(_activeBoxes, slots);
    }
    public void NotifyBoxFull(Box box)
    {
        if (!_activeBoxes.Contains(box))
            return;

        _activeBoxes.Remove(box);

        box.SetActive(false);

        OnBoxRemoved?.Invoke(box);

        TrySpawnNext();

        _layout.AlignBoxes(_activeBoxes, slots);
    }
    private void TrySpawnNext()
    {
        if (!_sequence.HasNext())
            return;

        var next = _sequence.GetNext();
        ActivateBox(next);
    }
    public void ResetQueue()
    { 
        foreach (var box in _activeBoxes)
        {
            box.OnBoxFull -= NotifyBoxFull;
            box.SetActive(false);
        }

        _activeBoxes.Clear();
    }
    public void TryProcessItemScrew(ScrewController screw)
    {
        var box = FindSuitableBox(screw.GetColor());

        if (box == null)
        {
            HideScrew(screw);
            return;
        }

        AddScrewToBox(screw, box);
    }
    public void AddScrewToBox(ScrewController screw, Box box)
    {
        if (screw == null || box == null)
            return;
        if (box.IsLocked || box.IsBoxFull)
            return;
        box.AddScrew(new List<ScrewController> { screw });
    }
    private void HideScrew(ScrewController screw)
    {
        var color = screw.GetColor();

        if (!_hiddenByColor.ContainsKey(color))
            _hiddenByColor[color] = new List<ScrewController>();

        screw.SetActive(false);

        _hiddenByColor[color].Add(screw);
    }

    private void ActivateBox(Box box)
    {
        box.SetActive(true);
        _activeBoxes.Add(box);

        OnBoxSpawned?.Invoke(box);

        TryResolveHiddenForBox(box);
    }

    private void TryResolveHiddenForBox(Box box)
    {
        var color = box.Color;

        if (!_hiddenByColor.ContainsKey(color))
            return;

        var hiddenList = _hiddenByColor[color];

        if (hiddenList.Count == 0)
            return;

        var copy = hiddenList.ToList();

        foreach (var screw in copy)
        {
            if (box.IsBoxFull)
                break;

            hiddenList.Remove(screw);

            screw.SetActive(true);

            box.AddScrew(new List<ScrewController> { screw });
        }

        if (hiddenList.Count == 0)
            _hiddenByColor.Remove(color);
    }
    public void ProcessScrews(IEnumerable<ScrewController> screws)
    {
        if (screws == null)
            return;

        var grouped = screws
            .Where(s => s != null)
            .GroupBy(s => s.GetColor());

        foreach (var group in grouped)
        {
            TryPlaceGroup(group.Key, group.ToList());
        }
    }

    private void TryPlaceGroup(ColorEnum color, List<ScrewController> screws)
    {
        var targetBox = FindSuitableBox(color);

        if (targetBox == null) return;

        targetBox.AddScrew(screws);
    }

    internal Box FindSuitableBox(ColorEnum color)
    {
        return _activeBoxes
            .FirstOrDefault(b =>
                !b.IsLocked &&
                !b.IsBoxFull &&
                (b.Color == color || b.Color == ColorEnum.Rainbow));
    }


    public void EnableSpecialMode(SideMission mission)
    {
        if (mission == null)
            return;
        _currentMission = mission;

        OnSpecialModeStarted?.Invoke(mission);
    }


    public void UnlockNextBox()
    {
    }

    public bool HasLockedBox()
    {
        return _activeBoxes.Any(b => b.IsLocked);
    }

    private void RemoveBoxInternal(Box box)
    {
        if (!_activeBoxes.Contains(box))
            return;

        _activeBoxes.Remove(box);

        box.OnBoxFull -= NotifyBoxFull;
        box.SetActive(false);

        OnBoxRemoved?.Invoke(box);
    }

    private void FillToSlotCapacity()
    {
        while (_activeBoxes.Count < slots.Count && _sequence.HasNext())
        {
            var next = _sequence.GetNext();
            ActivateBox(next);
        }
    }

    public void RemoveBoxByColor(ColorEnum targetColor, int count)
    {
        if (count <= 0)
            return;

        var removableBoxes = _activeBoxes
            .Where(b => b.Color == targetColor)
            .Take(count)
            .ToList();

        foreach (var box in removableBoxes)
        {
            RemoveBoxInternal(box);
        }

        FillToSlotCapacity();

        _layout.AlignBoxes(_activeBoxes, slots);
    }
    private void RemoveBox(Box box)
    {
        if (!_activeBoxes.Contains(box))
            return;

        _activeBoxes.Remove(box);

        box.OnBoxFull -= NotifyBoxFull;   // ⚠️ quan trọng
        box.SetActive(false);

        OnBoxRemoved?.Invoke(box);

        TrySpawnNext();
    }
    internal void CanAddScrew(ScrewController screw, Box suitableBox, out bool canAdd)
    {
        canAdd = false;

        if (suitableBox == null || screw == null)
            return;

        if (suitableBox.IsBoxFull)
            return;

        if (suitableBox.IsLocked)
            return;

        canAdd = true;
    }
}