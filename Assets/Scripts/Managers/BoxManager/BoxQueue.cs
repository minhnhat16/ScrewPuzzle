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
    private readonly HashSet<BoxSlot> _pendingSlotSet = new HashSet<BoxSlot>(); // track slot đang chờ spawn, tránh enqueue trùng
    private bool _isSpawning = false;

    /// <summary>
    /// Chỉ align khi không có box nào đang trong animation spawn.
    /// Tránh layout giật cục khi box đang bay vào từ ngoài màn hình.
    /// </summary>
    private void AlignIfNotSpawning()
    {
        if (_isSpawning || _pendingSpawnSlots.Count > 0) return;
        _layout.AlignSlots(slots, totalWidth);
    }

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
        _pendingSlotSet.Clear();
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
        _pendingSlotSet.Clear();
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
            totalGold = WalletManager.ins.Get(Currency.Gold),
            currentTicket = WalletManager.ins.Get(Currency.Ticket),
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

        // SpawnInitial dùng snap = true, không cần spawn queue.
        // Dùng GetNext() thay vì PickNextBox() — tránh smart pick filter
        // block slot khi tất cả màu còn lại đã trùng với active boxes.
        foreach (var slot in slots)
        {
            if (slot.isLocked) continue;
            if (_sequence == null || !_sequence.HasNext()) break;
            var box = _sequence.GetNext();
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

            // Mỗi tầng gọi GetColorCounts() fresh để tránh stale snapshot
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

        // FIX: Fallback chỉ dequeue 1 lần duy nhất, chấp nhận kể cả trùng màu
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

        // Fallback: tất cả màu đang active → bỏ filter
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
        // FIX: điều kiện cũ "b.RemainingCapacity == b.RemainingCapacity" luôn true
        // → dùng GetNext() trực tiếp
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

        // Lấy slot của box này TRƯỚC khi RemoveBox — dùng chính slot đó để spawn box mới
        // Tránh bug: FirstOrDefault luôn trả slot 1 khi nhiều box clear cùng lúc
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
        MissionManager.ins.ProcessBoxClosed(box.Color, 1);
        if (_sequence.HasNext())
        {
            // Enqueue đúng slot vừa bị clear — không tìm freeSlot mới
            // Khi 4 box clear cùng lúc, mỗi NotifyBoxFull enqueue đúng slot của nó
            if (slot != null)
                RequestSpawn(slot);
            else
                CheckWinIfNeeded();
        }
        else
        {
            AlignIfNotSpawning();
            if (_activeBoxes.Count == 0)
                LevelManager.ins.CheckWinCondition();
        }

        try { box.OnReset(); } catch (Exception) { }
        box.gameObject.SetActive(false);
    }

    // ─── Spawn Queue System ────────────────────────────────────────

    /// <summary>
    /// Enqueue một slot cần spawn box. Serialize tất cả yêu cầu spawn,
    /// tránh 2 box bị dequeue cùng lúc khi NotifyBoxFull và UnlockNextSlot chạy đồng thời.
    /// </summary>
    private void RequestSpawn(BoxSlot slot)
    {
        if (_pendingSlotSet.Contains(slot))
        {
            Debug.LogWarning($"[BoxQueue] RequestSpawn — slot already pending, skip duplicate.");
            return;
        }
        _pendingSpawnSlots.Enqueue(slot);
        _pendingSlotSet.Add(slot);
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

        // ── Batch: dequeue TẤT CẢ pending slots cùng lúc, spawn parallel ──
        // Gom hết slot hợp lệ + pick box cho từng slot trước khi animate bất kỳ cái nào
        var batch = new List<(Box box, BoxSlot slot)>();

        while (_pendingSpawnSlots.Count > 0 && _sequence.HasNext())
        {
            var slot = _pendingSpawnSlots.Dequeue();
            _pendingSlotSet.Remove(slot); // đã dequeue → bỏ khỏi guard set

            if (slot.isContainingBox)
            {
                Debug.Log("[BoxQueue] TryProcessSpawnQueue — slot already occupied, skip.");
                continue;
            }

            var box = PickNextBox();
            if (box == null)
            {
                box = _sequence.GetNext();
                if (box == null)
                {
                    Debug.LogWarning("[BoxQueue] TryProcessSpawnQueue — GetNext() null, skip slot.");
                    continue;
                }
                Debug.LogWarning($"[BoxQueue] TryProcessSpawnQueue — fallback GetNext color={box.Color}.");
            }

            batch.Add((box, slot));
        }

        if (batch.Count == 0)
        {
            CheckWinIfNeeded();
            return;
        }

        // Spawn tất cả box trong batch cùng lúc (parallel animation)
        // KHÔNG align trước spawn — slot position chưa ổn định khi có nhiều box
        // clear cùng lúc. Align duy nhất 1 lần SAU KHI tất cả animation xong
        // và không còn pending spawn nào nữa.
        _isSpawning = true;
        int remaining = batch.Count;

        foreach (var (box, slot) in batch)
        {
            SpawnBoxIntoSlotInternal(box, slot, () =>
            {
                remaining--;
                if (remaining > 0) return; // chờ các box khác trong batch

                _isSpawning = false;

                // Chỉ align khi không còn pending nào — tức là tất cả slot
                // đã được fill hoặc không có box mới → layout thực sự ổn định
                if (_pendingSpawnSlots.Count == 0)
                    _layout.AlignSlots(slots, totalWidth);

                CheckWinIfNeeded();
                TryProcessSpawnQueue();
            });
        }
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

    // Giữ lại SpawnBoxIntoSlot cho SpawnInitial (snap) và UnlockNextSlot nếu cần gọi trực tiếp
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
            // Dùng spawn queue → tự động serialize với NotifyBoxFull
            // AlignSlots sẽ được gọi trong TryProcessSpawnQueue onDone sau khi box animate xong
            RequestSpawn(lockedSlot);
        }
        else
        {
            // Không có box spawn → align ngay
            AlignIfNotSpawning();
        }
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

        screw.MarkDetachedFromBoard();
        screw.SetActive(false);
        _hiddenByColor[color].Add(screw);
    }

    // ─── Misc ──────────────────────────────────────────────────────

    public void RemoveBoxByColor(ColorEnum targetColor, int count)
    {
        if (count <= 0) return;
        var toRemove = _activeBoxes.Where(b => b.Color == targetColor).Take(count).ToList();
        foreach (var box in toRemove) RemoveBoxInternal(box);
        AlignIfNotSpawning();
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
        if (_isSpawning || _pendingSpawnSlots.Count > 0)
            return true;

        var boxes = _sequence.GetAllBox();
        return _activeBoxes.Any(b => b != null && b.IsBusy)
            || boxes.Any(b => b != null && b.gameObject.activeInHierarchy && b.IsBusy);
    }

    public void SetDifficultyBias(float bias)
    {
        difficultyBias = Mathf.Clamp01(bias);
        Debug.Log($"[BoxQueue] DifficultyBias set to {difficultyBias:P0}");
    }
}
