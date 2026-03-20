using ConfigFile;
using Core.Match;
using Enums;
using Ingame;
using Ingame.Pools;
using Ingame.Screw;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxQueue : SingletonMono<BoxQueue>, ILevelBoxQueue, IContainerQueue
{
    private SideMission _currentMission;
    public bool HasSpecialBox => _currentMission != null;

    // ─── State ─────────────────────────────────────────────────────
    public int ActiveBoxCount => _activeBoxes.Count;
    public int ActiveCount => _activeBoxes.Count;

    private IBoxFactory _factory;
    private IBoxSequenceService _sequence;
    private IBoxSlotLayoutService _layout;
    private ITopLayerScrewProvider _topLayerProvider;
    private IArrayScrew _arrayScrew;

    // ─── Spawn Queue (fix concurrent spawn) ────────────────────────
    private readonly Queue<BoxSlot> _pendingSpawnSlots = new Queue<BoxSlot>();
    private bool _isSpawning = false;

    [SerializeField] private List<Box> _activeBoxes = new();
    private List<BoxConfigRecord> _configRecords = new();

    public bool IsReady => _factory != null && _sequence != null && _layout != null;

    [SerializeField] private List<BoxSlot> slots;
    [SerializeField] private float totalWidth = 6f;

    [Header("Slot Lock Settings")]
    [Tooltip("Số slot mở sẵn khi bắt đầu level (tính từ slot 0). Các slot còn lại sẽ bị lock.")]
    [SerializeField][Min(1)] private int initialUnlockedSlots = 2;

    [Header("Smart Spawn Settings")]
    [Tooltip("Xác suất (0–1) ưu tiên spawn box màu trùng với screw ở top layers.\n0 = hoàn toàn ngẫu nhiên, 1 = luôn ưu tiên nếu có.")]
    [SerializeField][Range(0f, 1f)] private float topLayerMatchChance = 0.7f;
    [Tooltip("Số layer tính từ trên xuống để kiểm tra màu screw (1 = top only, 2 = top + second).")]
    [SerializeField][Range(1, 3)] private int smartSpawnLayerDepth = 2;

    [Header("Difficulty Settings")]
    [Tooltip("0 = dễ nhất (luôn dùng smart pick), 1 = khó nhất (luôn random).")]
    [SerializeField][Range(0f, 1f)] private float difficultyBias = 0f;

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

    [SerializeField] private List<Box> currentLevelBox;

    public bool HasNext() => _sequence.HasNext();

    // ─── Unity ─────────────────────────────────────────────────────

    private void OnDestroy()
    {
        UnsubscribeSlotEvents();
    }

    // ─── Setup ─────────────────────────────────────────────────────

    public void Setup(IBoxFactory factory, IBoxSequenceService sequence, IBoxSlotLayoutService layout)
    {
        _factory = factory;
        _sequence = sequence;
        _layout = layout;
        Debug.Log("Set up layout for box " + layout);
    }

    public void SetTopLayerProvider(ITopLayerScrewProvider provider) => _topLayerProvider = provider;
    public void SetArrayScrew(IArrayScrew arrayScrew) => _arrayScrew = arrayScrew;

    public void LoadBoxConfigRecord(BoxConfig boxConfig)
    {
        if (boxConfig == null)
        {
            Debug.LogWarning("[BoxQueue] BoxConfig is null.");
            return;
        }

        _configRecords = boxConfig.records != null ? boxConfig.records.ToList() : new List<BoxConfigRecord>();

        var colors = _configRecords.Select(r => r.BoxColor).ToList();
        Debug.Log($"[BoxQueue] Loaded box config with {colors.Count} records. Colors: [{string.Join(", ", colors)}]");

        if (_factory == null)
        {
            Debug.LogError("[BoxQueue] Not setup — call Setup() before LoadBoxConfigRecord().");
            return;
        }

        var boxes = _factory.CreateBoxes(_configRecords);
        var colorList = boxes.Select(b => b == null ? "null" : b.Color.ToString()).ToList();
        Debug.Log("[BoxQueue] Created boxes from factory: " + $"[{string.Join(", ", colorList)}]");

        _sequence.Load(boxes);
        currentLevelBox = _sequence.GetAllBox();
        Debug.Log($"[BoxQueue] Loaded {boxConfig.name} {_configRecords.Count} box config records.");
    }

    public void ClearConfigRecords() => _configRecords.Clear();

    public void ClearCurrentBoxes()
    {
        foreach (var box in _activeBoxes)
        {
            box.OnBoxFull -= NotifyBoxFull;
            box.OnReset();
            box.gameObject.SetActive(false);
        }
        foreach (var slot in slots)
            slot.RemoveBox();
        _activeBoxes.Clear();

        foreach (var kvp in _hiddenByColor)
        {
            if (kvp.Value == null) continue;
            foreach (var screw in kvp.Value)
            {
                if (screw != null)
                {
                    screw.ResetHoldState();
                    screw.SetActive(false);
                }
            }
        }
        _hiddenByColor.Clear();

        // Reset spawn queue
        _pendingSpawnSlots.Clear();
        _isSpawning = false;
    }

    public void OnReset()
    {
        UnsubscribeSlotEvents();
        ClearCurrentBoxes();
        ClearConfigRecords();
        _hiddenByColor.Clear();
        _currentMission = null;
        _topLayerProvider = null;
        _pendingSpawnSlots.Clear();
        _isSpawning = false;
    }

    // ─── Slot Event Subscription ───────────────────────────────────

    private void SubscribeSlotEvents()
    {
        foreach (var slot in slots)
            slot.OnLockedSlotTapped += HandleLockedSlotTapped;
    }

    private void UnsubscribeSlotEvents()
    {
        foreach (var slot in slots)
            slot.OnLockedSlotTapped -= HandleLockedSlotTapped;
    }

    private void HandleLockedSlotTapped(BoxSlot tappedSlot)
    {
        if (TutorialManager.ins != null && TutorialManager.ins.IsBlockingInput)
        {
            Debug.Log("[BoxQueue] HandleLockedSlotTapped blocked — tutorial is active.");
            return;
        }

        if (DialogManager.ins == null) return;

        DialogManager.ins.ShowDialog(DialogIndex.ReviveDialog, new ReviveParam
        {
            isRevive = false,
            totalGold = WalletManager.ins.Get(Currency.Ticket),
            currentTicket = 0,
            onWatchAccepted = () => UnlockNextSlot()
        });
    }

    // ─── ILevelBoxQueue: Screw Routing ─────────────────────────────

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
                LevelManager.ins?.OnScrewQueued();
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
            slots[i].SetLocked(i >= initialUnlockedSlots);

        SubscribeSlotEvents();

        // SpawnInitial dùng snap = true, không cần spawn queue
        foreach (var slot in slots)
        {
            if (slot.isLocked) continue;
            if (_sequence == null || !_sequence.HasNext()) break;
            var box = PickNextBox();
            if (box != null)
                SpawnBoxIntoSlotInternal(box, slot, null, snap: true);
        }

        _layout.AlignSlots(slots, totalWidth, duration: 0f);
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

    void IContainerQueue.UnlockNext() => UnlockNextSlot();
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
        if (_activeBoxes.Count == 0)
        {
            Debug.Log("Active box count 0");
            return null;
        }
        return _activeBoxes.FirstOrDefault(b =>
            !b.IsLocked && !b.IsFull && !b.IsMoving &&
            (b.Color.ToString().ToLower() == tag || b.Color == ColorEnum.Rainbow));
    }

    // ─── Smart Box Picking ─────────────────────────────────────────

    private Box PickNextBox()
    {
        var activeColors = _activeBoxes.Select(b => b.Color).ToHashSet();
        bool skipSmart = UnityEngine.Random.value < difficultyBias;

        if (!skipSmart)
        {
            Box smart = null;

            smart = TryPickFromArray(activeColors, _sequence.GetColorCounts());
            if (smart == null) smart = TryPickFromTopLayer(activeColors, _sequence.GetColorCounts());
            if (smart == null) smart = TryPickNonDuplicate(activeColors);
            if (smart == null) smart = TryPickMatchingActiveBoxWithLeastScrews();

            // FIX: Nếu smart trùng màu active → trả lại sequence, KHÔNG drop
            if (smart != null && activeColors.Contains(smart.Color) && smart.Color != ColorEnum.Rainbow)
            {
                Debug.LogWarning($"[BoxQueue] Smart pick color={smart.Color} trùng active → trả lại sequence.");
                _sequence.ReturnToFront(smart);
                smart = null;
            }

            currentLevelBox = _sequence.GetAllBox();
            if (smart != null) return smart;
        }
        else
        {
            Debug.Log($"[BoxQueue] DifficultyBias ({difficultyBias:P0}) triggered.");
        }

        var fallback = PickFallback();
        currentLevelBox = _sequence.GetAllBox();

        if (fallback != null && activeColors.Contains(fallback.Color) && fallback.Color != ColorEnum.Rainbow)
            Debug.LogWarning($"[BoxQueue] Fallback buộc spawn trùng màu={fallback.Color} — không còn lựa chọn.");

        return fallback;
    }

    private Box TryPickMatchingActiveBoxWithLeastScrews()
    {
        if (_activeBoxes.Count == 0) return null;

        var sortedByLeastScrews = _activeBoxes
            .Where(b => !b.IsLocked && !b.IsFull)
            .OrderByDescending(b => b.RemainingCapacity)
            .ToList();

        foreach (var activeBox in sortedByLeastScrews)
        {
            var box = _sequence.TryDequeueMatching(b => b.Color == activeBox.Color);
            if (box != null)
            {
                Debug.Log($"[BoxQueue] Tầng 3.5 (LeastScrews) — color={box.Color} remaining={activeBox.RemainingCapacity}.");
                return box;
            }
        }
        return null;
    }

    private Box TryPickFromArray(HashSet<ColorEnum> activeColors, Dictionary<ColorEnum, int> sequenceCounts)
    {
        if (_arrayScrew == null || !_arrayScrew.HasAny()) return null;

        var arrayCounts = _arrayScrew.GetHeldColorCounts();
        if (arrayCounts.Count == 0) return null;

        var candidates = sequenceCounts.Keys
            .Where(c => !activeColors.Contains(c) && arrayCounts.ContainsKey(c))
            .OrderByDescending(c => arrayCounts[c])
            .ToList();

        Debug.Log("[TryPickFromArray] Candidates: " +
                  $"[{string.Join(", ", candidates)}], Count: {candidates.Count}, ArrayCounts: " +
                  $"[{string.Join(", ", arrayCounts.Select(kv => $"{kv.Key}={kv.Value}"))}]");

        if (candidates.Count == 0)
            candidates = sequenceCounts.Keys
                .Where(c => arrayCounts.ContainsKey(c))
                .OrderByDescending(c => arrayCounts[c])
                .ToList();

        foreach (var color in candidates)
        {
            var remainBoxes = _sequence.GetAllBox().Where(b => b != null && b.Color == color).ToList();
            Debug.Log($"[BoxQueue] Remaining boxes in queue with color {color}: {remainBoxes.Count}");

            var box = _sequence.TryDequeueMatching(b => b.Color == color);
            if (box != null)
            {
                Debug.Log($"[BoxQueue] Tầng 1 (Array) — color={box.Color} arrayCount={arrayCounts[color]}.");
                return box;
            }
        }
        return null;
    }

    private Box TryPickFromTopLayer(HashSet<ColorEnum> activeColors, Dictionary<ColorEnum, int> sequenceCounts)
    {
        if (_topLayerProvider == null) return null;
        if (UnityEngine.Random.value > topLayerMatchChance) return null;

        var topColors = _topLayerProvider.GetTopLayerColors(smartSpawnLayerDepth);
        if (topColors.Count == 0) return null;

        var preferred = topColors
            .Where(c => !activeColors.Contains(c) && sequenceCounts.ContainsKey(c))
            .ToHashSet();

        var search = preferred.Count > 0
            ? preferred
            : topColors.Where(c => sequenceCounts.ContainsKey(c)).ToHashSet();

        var box = _sequence.TryDequeueMatching(b => search.Contains(b.Color));
        if (box != null)
            Debug.Log($"[BoxQueue] Tầng 2 (TopLayer) — color={box.Color}.");

        return box;
    }

    private Box TryPickNonDuplicate(HashSet<ColorEnum> activeColors)
    {
        var box = _sequence.TryDequeueMatching(b => !activeColors.Contains(b.Color));
        if (box != null)
            Debug.Log($"[BoxQueue] Tầng 3 (NonDuplicate) — color={box.Color}.");
        return box;
    }

    private Box PickFallback()
    {
        return _sequence.GetNext();
    }

    // ─── AddScrewToBox ─────────────────────────────────────────────

    public bool AddScrewToBox(ScrewController screw, Box box)
    {
        if (screw == null || box == null) return false;
        if (box.IsLocked || box.IsFull) return false;
        return box.TryAddScrew(screw);
    }

    // ─── Box Lifecycle ─────────────────────────────────────────────

    private void NotifyBoxFull(Box box)
    {
        if (!_activeBoxes.Contains(box)) return;

        var slot = slots.FirstOrDefault(s => s.CheckIsContainingThisBox(box));
        slot?.RemoveBox();

        _activeBoxes.Remove(box);
        box.OnBoxFull -= NotifyBoxFull;
        OnBoxFull?.Invoke(box);
        _onContainerCompleted?.Invoke(box);

        Debug.Log("[BoxQueue] Box full: color=" + box.Color +
                  ", activeBoxCount=" + _activeBoxes.Count +
                  ", sequenceRemaining=" + _sequence.GetColorCounts().Sum(kv => kv.Value));

        TutorialEventBus.Emit("on_box_full");
        LevelManager.ins.OnBoxCleared();

        if (_sequence.HasNext())
        {
            var freeSlot = slots.FirstOrDefault(s => !s.isLocked && !s.isContainingBox);
            if (freeSlot != null)
            {
                RequestSpawn(freeSlot);
            }
            else
            {
                // ─── FIX BUG 1: Chỉ check win nếu không còn box nào đang di chuyển vào slot ───
                // Nếu có box đang move, slot chưa thực sự trống — chờ animation xong sẽ trigger lại
                if (!HasMovingBox())
                {
                    Debug.Log("[BoxQueue] NotifyBoxFull — no free slot and no moving box, check win.");
                    CheckWinIfNeeded();
                }
                else
                {
                    Debug.Log("[BoxQueue] NotifyBoxFull — no free slot but box is moving, skip win check.");
                }
            }
        }
        else
        {
            _layout.AlignSlots(slots, totalWidth);
            if (_activeBoxes.Count == 0)
                LevelManager.ins.CheckWinCondition();
        }

        try { box.OnReset(); } catch (Exception) { }
        box.gameObject.SetActive(false);
    }

    // ─── Spawn Queue System ────────────────────────────────────────

    private void RequestSpawn(BoxSlot slot)
    {
        _pendingSpawnSlots.Enqueue(slot);
        Debug.Log($"[BoxQueue] RequestSpawn — slot enqueued, pending={_pendingSpawnSlots.Count}, isSpawning={_isSpawning}");
        TryProcessSpawnQueue();
    }

    private void TryProcessSpawnQueue()
    {
        if (_isSpawning) return;
        if (_pendingSpawnSlots.Count == 0) return;
        if (!_sequence.HasNext())
        {
            _pendingSpawnSlots.Clear();
            CheckWinIfNeeded();
            return;
        }

        var slot = _pendingSpawnSlots.Dequeue();

        // ─── FIX BUG 2: Slot bị occupied trong lúc chờ → tìm slot thay thế thay vì bỏ qua hoàn toàn ───
        if (slot.isContainingBox)
        {
            Debug.Log("[BoxQueue] TryProcessSpawnQueue — slot already occupied, looking for alternative.");
            var alternativeSlot = slots.FirstOrDefault(s => !s.isLocked && !s.isContainingBox);
            if (alternativeSlot != null)
            {
                Debug.Log("[BoxQueue] TryProcessSpawnQueue — found alternative slot, re-enqueue.");
                _pendingSpawnSlots.Enqueue(alternativeSlot);
            }
            else
            {
                Debug.Log("[BoxQueue] TryProcessSpawnQueue — no alternative slot available, will retry later.");
            }
            TryProcessSpawnQueue();
            return;
        }

        var box = PickNextBox();

        // ─── FIX BUG 3: Reset _isSpawning khi PickNextBox trả null để tránh kẹt flag ───
        if (box == null)
        {
            Debug.LogWarning("[BoxQueue] TryProcessSpawnQueue — PickNextBox() returned null! Releasing isSpawning flag.");
            _isSpawning = false; // CRITICAL: phải reset flag, không được để kẹt
            CheckWinIfNeeded();
            return;
        }

        _isSpawning = true;
        SpawnBoxIntoSlotInternal(box, slot, () =>
        {
            _isSpawning = false;
            _layout.AlignSlots(slots, totalWidth);
            CheckWinIfNeeded();
            TryProcessSpawnQueue();
        });
    }

    private void CheckWinIfNeeded()
    {
        if (!_sequence.HasNext() && _activeBoxes.Count == 0 && _pendingSpawnSlots.Count == 0)
        {
            Debug.Log("[BoxQueue] ✅ All boxes cleared — notifying win.");
            LevelManager.ins.CheckWinCondition();
        }
    }

    // ─── Internal Spawn (dùng chung cho mọi path) ──────────────────

    private void SpawnBoxIntoSlotInternal(Box box, BoxSlot slot, Action onDone, bool snap = false)
    {
        if (box == null) { onDone?.Invoke(); return; }

        Debug.Log($"[BoxQueue] SpawnBoxIntoSlotInternal — color={box.Color}, snap={snap}");

        slot.AddBox(box);
        box.gameObject.SetActive(true);
        ResolveAllHiddenForBox(box);

        if (snap)
        {
            box.transform.position = slot.transform.position;
            ActivateBox(box);
            _layout.AlignSlots(slots, totalWidth);
            onDone?.Invoke();
            return;
        }

        box.transform.position = GetOffScreenLeft(slot.transform.position.y);
        box.MoveTo(slot.transform.position, 0.4f, () =>
        {
            ActivateBox(box);
            ResolveAllHiddenForColor(box.Color);
            onDone?.Invoke();
        });
    }

    private void SpawnBoxIntoSlot(Box box, BoxSlot slot, bool snap = false)
    {
        SpawnBoxIntoSlotInternal(box, slot, null, snap);
    }

    private void ActivateBox(Box box)
    {
        _activeBoxes.Add(box);
        box.OnBoxFull += NotifyBoxFull;
        OnBoxSpawned?.Invoke(box);
        _onContainerSpawned?.Invoke(box);

        try { box.OnActivated(); }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BoxQueue] ActivateBox -> OnActivated failed: {ex.Message}");
        }

        TryTakeScrewsFromArray(box);
    }

    // ─── Unlock Slot ───────────────────────────────────────────────

    public void UnlockNextSlot()
    {
        var lockedSlot = slots.FirstOrDefault(s => s.isLocked);
        if (lockedSlot == null)
        {
            Debug.LogWarning("[BoxQueue] No locked slot to unlock.");
            return;
        }

        lockedSlot.UnlockSlot();

        if (_sequence.HasNext())
        {
            RequestSpawn(lockedSlot);
        }

        _layout.AlignSlots(slots, totalWidth);
    }

    // ─── Hidden / Resolve ──────────────────────────────────────────

    internal void ResolveAllHiddenForColor(ColorEnum color)
    {
        var sm = LevelManager.ins.ScrewManager;
        if (sm == null) return;

        var candidates = _activeBoxes
            .Where(b => !b.IsFull && !b.IsLocked && !b.IsMoving && b.Color == color)
            .OrderByDescending(b => b.RemainingCapacity)
            .ToList();

        if (candidates.Count == 0) return;

        foreach (var box in candidates)
        {
            if (box.IsFull) continue;
            int remaining = box.RemainingCapacity;
            var fromBreaker = sm.PopHiddenScrew(color, remaining);

            if (fromBreaker.Count > 0)
            {
                bool added = box.TryAddScrews(fromBreaker, true);
                Debug.Log($"Resolve hidden for box: added={added}, count={fromBreaker.Count}, box color={box.Color}");
                foreach (var screw in fromBreaker)
                {
                    sm.RemoveHidden(screw);
                    screw.SetActive(true);
                }
            }

            if (sm.GetHiddenScrew(color) == 0) break;
        }
    }

    internal void ResolveAllHiddenForBox(Box box)
    {
        if (box == null || box.IsFull || box.RemainingCapacity <= 0) return;

        var color = box.Color;
        var sm = LevelManager.ins.ScrewManager;
        if (sm != null && !box.IsFull)
        {
            int remaining = box.RemainingCapacity;
            var fromBreaker = sm.PopHiddenScrew(color, remaining);

            if (fromBreaker.Count > 0)
            {
                bool added = box.TryAddScrews(fromBreaker, true);
                Debug.Log($"Resolve hidden for box: added={added}, count={fromBreaker.Count}, box color={box.Color}");
                foreach (var screw in fromBreaker)
                {
                    sm.RemoveHidden(screw);
                    screw.SetActive(true);
                }
            }
        }
    }

    private Vector3 GetOffScreenLeft(float worldY)
    {
        Camera cam = Camera.main;
        if (cam == null) return new Vector3(-20f, worldY, 0f);
        Vector3 offScreen = cam.ViewportToWorldPoint(new Vector3(-0.2f, 0f, cam.nearClipPlane));
        return new Vector3(offScreen.x, worldY, 0f);
    }

    // ─── Take Screws From Array ────────────────────────────────────

    internal void TryTakeScrewsFromArray(Box box)
    {
        Debug.Log($"[BoxQueue] TryTakeScrewsFromArray — color={box.Color}, arrayNull={_arrayScrew == null}");
        if (_arrayScrew == null) return;
        if (box.IsFull || box.Color == ColorEnum.Rainbow) return;

        int available = box.RemainingCapacity;
        if (available <= 0) return;

        var screws = _arrayScrew.TakeByColor(box.Color, available);
        Debug.Log($"[BoxQueue] TakeByColor — color={box.Color}, available={available}, taken={screws?.Count ?? 0}");

        if (screws == null || screws.Count == 0) return;

        foreach (var screw in screws)
        {
            if (box.IsFull) break;
            box.TryAddScrew(screw);
        }

        Debug.Log($"[BoxQueue] Took {screws.Count} screw(s) of color {box.Color} from ArrayScrew.");
    }

    // ─── Hidden Screw ──────────────────────────────────────────────

    private void HideScrew(ScrewController screw)
    {
        var color = screw.GetColor();
        if (!_hiddenByColor.ContainsKey(color))
            _hiddenByColor[color] = new List<ScrewController>();

        try
        {
            LevelManager.ins.layerManager.RemoveScrewsOnDict(new List<ScrewController> { screw });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BoxQueue] HideScrew -> RemoveScrewsOnDict failed: {ex.Message}");
        }

        screw.SetActive(false);
        _hiddenByColor[color].Add(screw);
    }

    // ─── Misc ──────────────────────────────────────────────────────

    public void RemoveBoxByColor(ColorEnum targetColor, int count)
    {
        if (count <= 0) return;
        var toRemove = _activeBoxes.Where(b => b.Color == targetColor).Take(count).ToList();
        foreach (var box in toRemove) RemoveBoxInternal(box);
        _layout.AlignSlots(slots, totalWidth);
    }

    private void RemoveBoxInternal(Box box)
    {
        if (!_activeBoxes.Contains(box)) return;

        var slot = slots.FirstOrDefault(s => s.CheckIsContainingThisBox(box));
        slot?.RemoveBox();

        box.gameObject.SetActive(false);
        _activeBoxes.Remove(box);
        box.OnBoxFull -= NotifyBoxFull;
        OnBoxRemoved?.Invoke(box);
        _onContainerRemoved?.Invoke(box);
    }

    public int RemoveFromSequenceByColor(ColorEnum targetColor, int count)
    {
        if (count <= 0) return 0;
        if (_sequence is BoxSequenceService seq)
            return seq.RemoveByColor(targetColor, count);

        Debug.LogWarning("[BoxQueue] RemoveFromSequenceByColor: _sequence không phải BoxSequenceService.");
        return 0;
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
        var lm = LevelManager.ins?.layerManager;
        if (lm != null)
            lm.RemoveScrewOnDict(screw, screw.GetSortingOrder());

        var box = FindSuitableBox(screw.GetColor());
        if (box == null) { HideScrew(screw); return; }
        bool added = AddScrewToBox(screw, box);
        if (!added) HideScrew(screw);
    }

    public bool HasLockedBox() => _activeBoxes.Any(b => b.IsLocked);
    public void UnlockNext() => UnlockNextSlot();

    public void EnableSpecialMode(SideMission mission)
    {
        if (mission == null) return;
        _currentMission = mission;
        OnSpecialModeStarted?.Invoke(mission);
    }

    internal IEnumerable<Box> GetActiveBoxes() => _activeBoxes;

    public bool HasMovingBox()
    {
        var boxes = _sequence.GetAllBox();
        return _activeBoxes.Any(b => b.IsMoving) || boxes.Any(b => b != null && b.IsMoving);
    }

    public void SetDifficultyBias(float bias)
    {
        difficultyBias = Mathf.Clamp01(bias);
        Debug.Log($"[BoxQueue] DifficultyBias set to {difficultyBias:P0}");
    }
}