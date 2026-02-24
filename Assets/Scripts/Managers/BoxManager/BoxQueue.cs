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

    public int ActiveBoxCount => throw new NotImplementedException();

    private IBoxFactory _factory;
    private IBoxSequenceService _sequence;
    private IBoxSlotLayoutService _layout;

    private List<Box> _activeBoxes = new();
    [SerializeField] private List<BoxSlot> slots;
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
            box.SetActive(false);

        _activeBoxes.Clear();
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

    private Box FindSuitableBox(ColorEnum color)
    {
        return _activeBoxes
            .FirstOrDefault(b =>
                !b.IsLocked &&
                !b.IsBoxFull &&
                (b.Color == color || b.Color == ColorEnum.Rainbow));
    }
    private void ActivateBox(Box box)
    {
        box.SetActive(true);
        _activeBoxes.Add(box);
    }

    public void EnableSpecialMode(SideMission mission)
    {
        if (mission == null)
            return;
        _currentMission = mission;

        OnSpecialModeStarted?.Invoke(mission);
    }


    public void LoadLevelBoxes(List<BoxConfigRecord> records)
    {
        throw new NotImplementedException();
    }

    public void UnlockNextBox()
    {
        throw new NotImplementedException();
    }
}