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
/// BoxQueue:
///  - Implement IContainerQueue  (Core layer — routing, lifecycle)
///  - Implement ILevelBoxQueue   (Game layer — level load, screw routing, reset)
///
/// LevelManager chỉ thấy ILevelBoxQueue.
/// IngameController chỉ thấy IContainerQueue.
/// Không ai cần BoxQueue.ins nữa.
/// </summary>
public class BoxQueue : MonoBehaviour, ILevelBoxQueue
{
    // ─── Singleton (backward compat — chỉ dùng để Bootstrapper lấy ref) ──
    public static BoxQueue ins { get; private set; }

    private SideMission _currentMission;
    public bool HasSpecialBox => _currentMission != null;

    // ─── State ─────────────────────────────────────────────────────
    public int ActiveBoxCount => _activeBoxes.Count;
    public int ActiveCount => _activeBoxes.Count;
    public bool HaseMovingContainer => _activeBoxes.Any(b => b.IsMoving);

    private IBoxFactory _factory;
    private IBoxSequenceService _sequence;
    private IBoxSlotLayoutService _layout;

    private List<Box> _activeBoxes = new();
    private List<BoxConfigRecord> _configRecords = new();
    [SerializeField] private List<BoxSlot> slots;
    private readonly Dictionary<ColorEnum, List<ScrewController>> _hiddenByColor = new();

    // ─── Events — IBoxQueue ────────────────────────────────────────
    public event Action<Box> OnBoxFull;
    public event Action<Box> OnBoxSpawned;
    public event Action<Box> OnBoxRemoved;
    public event Action<SideMission> OnSpecialModeStarted;

    // ─── Events — IContainerQueue ──────────────────────────────────
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

    // ─── Unity ─────────────────────────────────────────────────────
    private void Awake()
    {
        ins = this;
    }

    // ─── Setup ─────────────────────────────────────────────────────
    public void Setup(IBoxFactory factory, IBoxSequenceService sequence, IBoxSlotLayoutService layout)
    {
        _factory = factory;
        _sequence = sequence;
        _layout = layout;
    }

    // ─── ILevelBoxQueue: Level Lifecycle ───────────────────────────

    /// <summary>
    /// Load BoxConfig → convert records → build sequence.
    /// Gọi từ InitBoxQueueStep trước Initialize().
    /// </summary>
    public void LoadBoxConfigRecord(BoxConfig boxConfig)
    {
        if (boxConfig == null)
        {
            Debug.LogWarning("[BoxQueue] BoxConfig is null.");
            return;
        }

        _configRecords = boxConfig.records?.ToList() ?? new List<BoxConfigRecord>();

        if (_factory == null)
        {
            Debug.LogError("[BoxQueue] Not setup — call Setup() before LoadBoxConfigRecord().");
            return;
        }

        var boxes = _factory.CreateBoxes(_configRecords);
        _sequence.Load(boxes);

        foreach (var box in boxes)
            box.OnBoxFull += NotifyBoxFull;

        Debug.Log($"[BoxQueue] Loaded {_configRecords.Count} box config records.");
    }

    public void ClearConfigRecords()
    {
        _configRecords.Clear();
    }

    public void ClearCurrentBoxes()
    {
        foreach (var box in _activeBoxes)
            box.OnBoxFull -= NotifyBoxFull;
        _activeBoxes.Clear();
    }

    /// <summary>
    /// Full reset — clear boxes, config, hidden screws.
    /// Gọi từ LevelManager.OnReset().
    /// </summary>
    public void OnReset()
    {
        ClearCurrentBoxes();
        ClearConfigRecords();
        _hiddenByColor.Clear();
        _currentMission = null;
    }

    // ─── ILevelBoxQueue: Screw Routing ─────────────────────────────

    /// <summary>
    /// Nhận screws từ board, group theo màu, route vào box phù hợp.
    /// Nếu không có box → screw ở lại ArrayScrew (hàng chờ).
    /// → Gọi từ LevelManager.RemovePartItem()
    /// </summary>
    public void TryMoveScrewsGroupedByColor(List<ScrewController> screws, bool fromBoard)
    {
        if (screws == null || screws.Count == 0) return;

        foreach (var group in screws
                     .Where(s => s != null)
                     .GroupBy(s => s.GetColor()))
        {
            var box = FindSuitableBox(group.Key);
            if (box != null)
                box.TryAddScrews(group.ToList());
            else
            {
                // Không có box → thông báo để LevelManager reset combo
                LevelManager.ins?.OnScrewQueued();
            }
        }
    }

    // ─── IContainerQueue: Lifecycle ────────────────────────────────

    public void Initialize(bool isTutorial) => SpawnInitial();
    void IContainerQueue.Initialize(bool isTutorial) => Initialize(isTutorial);

    public void ResetQueue() => ClearCurrentBoxes();
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

    // ─── IContainerQueue: Routing ──────────────────────────────────

    IMatchContainer IContainerQueue.FindSuitable(string tag) => FindSuitableBox(tag);

    bool IContainerQueue.AddItemToContainer(IMatchItem item, IMatchContainer container)
    {
        if (item is not ScrewController screw) return false;
        if (container is not Box box) return false;
        return AddScrewToBox(screw, box);
    }

    void IContainerQueue.NotifyCompleted(IMatchContainer container)
    {
        if (container is Box box) NotifyBoxFull(box);
    }

    void IContainerQueue.UnlockNext() => UnlockNext();
    bool IContainerQueue.HasLocked() => HasLockedBox();

    // ─── Find Box ──────────────────────────────────────────────────

    public Box FindSuitableBox(ColorEnum color)
    {
        return _activeBoxes.FirstOrDefault(b =>
            !b.IsLocked && !b.IsFull && !b.IsMoving &&
            (b.Color == color || b.Color == ColorEnum.Rainbow));
    }

    public Box FindSuitableBox(string tag)
    {
        return _activeBoxes.FirstOrDefault(b =>
            !b.IsLocked && !b.IsFull && !b.IsMoving &&
            (b.Color.ToString().ToLower() == tag || b.Color == ColorEnum.Rainbow));
    }

    // ─── AddScrewToBox ─────────────────────────────────────────────

    public bool AddScrewToBox(ScrewController screw, Box box)
    {
        if (screw == null || box == null) return false;
        if (box.IsLocked || box.IsFull) return false;
        return box.TryAddScrew(screw);
    }

    // ─── Box Lifecycle ─────────────────────────────────────────────

    public void NotifyBoxFull(Box box)
    {
        if (!_activeBoxes.Contains(box)) return;
        _activeBoxes.Remove(box);
        box.OnBoxFull -= NotifyBoxFull;
        OnBoxFull?.Invoke(box);
        _onContainerCompleted?.Invoke(box);

        // Gọi LevelManager để tính điểm / combo
        LevelManager.ins?.OnBoxCleared();

        TrySpawnNext();
        _layout.AlignBoxes(_activeBoxes, slots);
    }

    private void ActivateBox(Box box)
    {
        _activeBoxes.Add(box);
        box.OnBoxFull += NotifyBoxFull;
        OnBoxSpawned?.Invoke(box);
        _onContainerSpawned?.Invoke(box);
        TryResolveHiddenForBox(box);
    }

    private void TrySpawnNext()
    {
        if (!_sequence.HasNext()) return;
        ActivateBox(_sequence.GetNext());
    }

    // ─── Hidden Screw ──────────────────────────────────────────────

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
        foreach (var screw in hiddenList.ToList())
        {
            if (box.IsFull) break;
            hiddenList.Remove(screw);
            screw.SetActive(true);
            box.TryAddScrew(screw);
        }

        if (hiddenList.Count == 0) _hiddenByColor.Remove(color);
    }

    // ─── Misc ──────────────────────────────────────────────────────

    public void RemoveBoxByColor(ColorEnum targetColor, int count)
    {
        if (count <= 0) return;
        var toRemove = _activeBoxes.Where(b => b.Color == targetColor).Take(count).ToList();
        foreach (var box in toRemove) RemoveBoxInternal(box);
        FillToSlotCapacity();
        _layout.AlignBoxes(_activeBoxes, slots);
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

    public bool HasLockedBox() => _activeBoxes.Any(b => b.IsLocked);
    public void UnlockNext() { /* TODO */ }

    public void EnableSpecialMode(SideMission mission)
    {
        if (mission == null) return;
        _currentMission = mission;
        OnSpecialModeStarted?.Invoke(mission);
    }
}