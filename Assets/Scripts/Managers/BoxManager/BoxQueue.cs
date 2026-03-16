using ConfigFile;
using Core.Match;
using Enums;
using Ingame;
using Ingame.Screw;
using System;
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
    public bool HaseMovingContainer => _activeBoxes.Any(b => b.IsMoving);

    private IBoxFactory _factory;
    private IBoxSequenceService _sequence;
    private IBoxSlotLayoutService _layout;
    private ITopLayerScrewProvider _topLayerProvider;
    private IArrayScrew _arrayScrew;

    private List<Box> _activeBoxes = new();
    private List<BoxConfigRecord> _configRecords = new();
    // Thêm vào BoxQueue — bên dưới phần Setup()
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
    [Tooltip("0 = dễ nhất (luôn dùng smart pick), 1 = khó nhất (luôn random).\n" +
         "Ở mức cao, các tầng ưu tiên (Array, TopLayer, LeastScrews) bị bỏ qua\n" +
         "theo xác suất → box spawn không match screw đang hold → game khó hơn.")]
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

        Debug.Log("Set up layout for box " + layout);
        _layout = layout;
    }

    public void SetTopLayerProvider(ITopLayerScrewProvider provider)
    {
        _topLayerProvider = provider;
    }

    public void SetArrayScrew(IArrayScrew arrayScrew)
    {
        _arrayScrew = arrayScrew;
    }

    // Replace usages of null-coalescing operator (??) with explicit null checks for Unity objects.
    // Example: _configRecords = boxConfig.records?.ToList() ?? new List<BoxConfigRecord>();
    // Fix: Use explicit null check for boxConfig.records.

    public void LoadBoxConfigRecord(BoxConfig boxConfig)
    {
        if (boxConfig == null)
        {
            Debug.LogWarning("[BoxQueue] BoxConfig is null.");
            return;
        }

        // Fix UNT0007: Unity objects should not use null coalescing.
        if (boxConfig.records != null)
            _configRecords = boxConfig.records.ToList();
        else
            _configRecords = new List<BoxConfigRecord>();

        var colors = _configRecords.Select(r => r.BoxColor).ToList();
        Debug.Log($"[BoxQueue] Loaded box config with {colors.Count} records. Colors: [{string.Join(", ", colors)}]");
        if (_factory == null)
        {
            Debug.LogError("[BoxQueue] Not setup — call Setup() before LoadBoxConfigRecord().");
            return;
        }

        var boxes = _factory.CreateBoxes(_configRecords);

        var colorList = boxes.Select(b => b == null ? "null" : b.Color.ToString()).ToList();
        Debug.Log("[BoxQueue] Created boxes from factory: " +
                  $"[{string.Join(", ", colorList)}]");
        _sequence.Load(boxes);

        Debug.Log($"[BoxQueue] Loaded {boxConfig.name} {_configRecords.Count} box config records.");
    }
    public void ClearConfigRecords() => _configRecords.Clear();

    public void ClearCurrentBoxes()
    {
        foreach (var box in _activeBoxes)
        {
            box.OnBoxFull -= NotifyBoxFull;
            box.OnReset();  // ← clear storage + null events + reset FSM
            box.gameObject.SetActive(false);
        }
        foreach (var slot in slots)
            slot.RemoveBox();
        _activeBoxes.Clear();

        // Clear hidden screws — tránh leak vào box mới sau reload
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
    }

    public void OnReset()
    {
        UnsubscribeSlotEvents();
        ClearCurrentBoxes();
        ClearConfigRecords();
        _hiddenByColor.Clear();
        _currentMission = null;
        _topLayerProvider = null;
        //_arrayScrew = null;
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

    private void HandleLockedSlotTapped(BoxSlot slot)
    {
        // Block unlock khi tutorial đang chạy
        if (TutorialManager.ins != null && TutorialManager.ins.IsBlockingInput)
        {
            Debug.Log("[BoxQueue] HandleLockedSlotTapped blocked — tutorial is active.");
            return;
        }

        if (DialogManager.ins == null) return;

        DialogManager.ins.ShowDialog(DialogIndex.ReviveDialog, new ReviveParam
        {
            isRevive = false,   // title = "Add One Box", watch text = "Free"
            totalGold = WalletManager.ins.Get(Currency.Ticket),
            currentTicket = 0,  // ẩn nút ticket
            onWatchAccepted = () =>
            {
                slot.UnlockSlot();
                if (_sequence.HasNext())
                    SpawnBoxIntoSlot(PickNextBox(), slot);
                _layout.AlignSlots(slots, totalWidth);
                Debug.Log($"[BoxQueue] Slot unlocked via dialog. Active boxes: {_activeBoxes.Count}");
            }
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

        // Subscribe sau khi lock state đã được set
        SubscribeSlotEvents();

        foreach (var slot in slots)
        {
            if (slot.isLocked) continue;
            if (_sequence == null || !_sequence.HasNext()) break;
            SpawnBoxIntoSlot(PickNextBox(), slot, snap: true);
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

    // ─── Smart Box Picking ─────────────────────────────────────────

    private Box PickNextBox()
    {
        var activeColors = _activeBoxes.Select(b => b.Color).ToHashSet();
        var sequenceCounts = _sequence.GetColorCounts();

        // ── Difficulty bias ────────────────────────────────────────
        // Roll một lần cho cả frame pick này.
        // Nếu roll < bias → bỏ qua toàn bộ smart tầng → thẳng fallback.
        // Kết quả: box spawn không match gì đang hold → player khó clear hơn.
        bool skipSmart = UnityEngine.Random.value < difficultyBias;

        if (!skipSmart)
        {
            var smart = TryPickFromArray(activeColors, sequenceCounts)
                     ?? TryPickFromTopLayer(activeColors, sequenceCounts)
                     ?? TryPickNonDuplicate(activeColors)
                     ?? TryPickMatchingActiveBoxWithLeastScrews();

            if (smart != null) return smart;
        }
        else
        {
            Debug.Log($"[BoxQueue] DifficultyBias ({difficultyBias:P0}) triggered — skipping smart pick.");
        }

        return PickFallback();
    }

    /// <summary>
    /// Set difficulty bias từ code (ví dụ: từ LevelConfig hoặc DifficultyManager).
    /// 0 = dễ nhất, 1 = khó nhất.
    /// </summary>
    public void SetDifficultyBias(float bias)
    {
        difficultyBias = Mathf.Clamp01(bias);
        Debug.Log($"[BoxQueue] DifficultyBias set to {difficultyBias:P0}");
    }

    /// <summary>
    /// Tầng 3.5: Khi tất cả màu trong sequence đều đang active,
    /// ưu tiên spawn box cùng màu với box active có ít screw nhất (RemainingCapacity cao nhất).
    /// Giúp player dễ complete box sắp đầy hơn.
    /// </summary>
    private Box TryPickMatchingActiveBoxWithLeastScrews()
    {
        if (_activeBoxes.Count == 0) return null;

        // Sắp xếp box active theo RemainingCapacity giảm dần
        // (RemainingCapacity cao = ít screw nhất = ưu tiên spawn thêm box cùng màu)
        var sortedByLeastScrews = _activeBoxes
            .Where(b => !b.IsLocked && !b.IsFull)
            .OrderByDescending(b => b.RemainingCapacity)
            .ToList();

        foreach (var activeBox in sortedByLeastScrews)
        {
            var box = _sequence.TryDequeueMatching(b => b.Color == activeBox.Color);
            if (box != null)
            {
                Debug.Log($"[BoxQueue] Tầng 3.5 (LeastScrews) — color={box.Color} " +
                          $"remaining={activeBox.RemainingCapacity}.");
                return box;
            }
        }

        return null;
    }

    /// <summary>
    /// Tầng 1: Ưu tiên box màu có nhiều screw nhất trong ArrayScrew, không trùng active.
    /// </summary>
    private Box TryPickFromArray(HashSet<ColorEnum> activeColors, Dictionary<ColorEnum, int> sequenceCounts)
    {
        if (_arrayScrew == null || !_arrayScrew.HasAny()) return null;

        var arrayCounts = _arrayScrew.GetHeldColorCounts();
        if (arrayCounts.Count == 0) return null;

        // Gốc là sequenceCounts: chỉ xét màu còn box trong queue
        // Sort theo số screw trong array giảm dần
        var candidates = sequenceCounts.Keys
            .Where(c => !activeColors.Contains(c) && arrayCounts.ContainsKey(c))
            .OrderByDescending(c => arrayCounts[c])
            .ToList();

        Debug.Log("[TryPickFromArray] Candidates after filtering active colors: " +
                  $"[{string.Join(", ", candidates)}], Array counts: " +
                  $"[{string.Join(", ", arrayCounts.Select(kv => $"{kv.Key}={kv.Value}"))}]");

        // Fallback: tất cả màu đều đang active → bỏ filter active
        if (candidates.Count == 0)
            candidates = sequenceCounts.Keys
                .Where(c => arrayCounts.ContainsKey(c))
                .OrderByDescending(c => arrayCounts[c])
                .ToList();

        foreach (var color in candidates)
        {
            var box = _sequence.TryDequeueMatching(b => b.Color == color);
            if (box != null)
            {
                Debug.Log($"[BoxQueue] Tầng 1 (Array) — color={box.Color} arrayCount={arrayCounts[color]}.");
                return box;
            }
        }

        return null;
    }

    /// <summary>
    /// Tầng 2: Ưu tiên box màu trùng screw ở top layers, không trùng active.
    /// Có xác suất roll theo topLayerMatchChance.
    /// </summary>
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

    /// <summary>
    /// Tầng 3: Bất kỳ box không trùng màu với box đang active.
    /// </summary>
    private Box TryPickNonDuplicate(HashSet<ColorEnum> activeColors)
    {
        var box = _sequence.TryDequeueMatching(b => !activeColors.Contains(b.Color));
        if (box != null)
            Debug.Log($"[BoxQueue] Tầng 3 (NonDuplicate) — color={box.Color}.");

        return box;
    }

    /// <summary>
    /// Tầng 4: Safety fallback — lấy theo thứ tự sequence, tránh block vô hạn.
    /// </summary>
    private Box PickFallback()
    {
        Debug.Log("[BoxQueue] Tầng 4 (Fallback) — sequence order.");
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
        slot.RemoveBox();

        _activeBoxes.Remove(box);
        box.OnBoxFull -= NotifyBoxFull;
        OnBoxFull?.Invoke(box);
        _onContainerCompleted?.Invoke(box);

        TutorialEventBus.Emit("on_box_full");
        LevelManager.ins.OnBoxCleared();

        if (_sequence.HasNext())
        {
            TrySpawnNextThenCheckWin();
        }
        else
        {
            // Không có box mới → align lại các slot còn lại (có box hoặc locked)
            _layout.AlignSlots(slots, totalWidth);

            if (_activeBoxes.Count == 0)
            {
                LevelManager.ins.CheckWinCondition();
            }
        }

        try { box.OnReset(); } catch (Exception) { }
        box.gameObject.SetActive(false);
    }

    /// <summary>
    /// Spawn box mới vào slot trống, đợi animation move xong rồi mới check win.
    /// </summary>
    private void TrySpawnNextThenCheckWin()
    {
        if (!_sequence.HasNext()) return;

        var freeSlot = slots.FirstOrDefault(s => !s.isLocked && !s.isContainingBox);
        if (freeSlot == null) return;

        var nextBox = PickNextBox();
        if (nextBox == null) return;

        SpawnBoxIntoSlotWithCallback(nextBox, freeSlot, () =>
        {
            _layout.AlignSlots(slots, totalWidth);

            // Check win SAU khi box đã activate hoàn tất
            if (!_sequence.HasNext() && _activeBoxes.Count == 0)
            {
                Debug.Log("[BoxQueue] ✅ All boxes cleared — notifying win.");
                LevelManager.ins.CheckWinCondition();
            }
        });

        _layout.AlignSlots(slots, totalWidth);
    }

    private void SpawnBoxIntoSlotWithCallback(Box box, BoxSlot slot, Action onActivated)
    {
        Debug.Log("[BOX QUEUE] Spawning box of color " + box.Color + " into slot. Box null? " +
                  (box == null) + ", Slot locked? " + (slot.isLocked) + ", Total screw on box " + box.RemainingCapacity);
        if (box == null) { onActivated?.Invoke(); return; }

        slot.AddBox(box);
        box.gameObject.SetActive(true);

        ResolveAllHiddenForBox(box);

        Vector3 offScreenStart = GetOffScreenLeft(slot.transform.position.y);
        box.transform.position = offScreenStart;

        box.MoveTo(slot.transform.position, 0.4f, () =>
        {
            ActivateBox(box);
            onActivated?.Invoke();
        });
    }
    private void ActivateBox(Box box)
    {
        _activeBoxes.Add(box);
        box.OnBoxFull += NotifyBoxFull;
        OnBoxSpawned?.Invoke(box);
        _onContainerSpawned?.Invoke(box);

        // Ensure screws stored in box are active when box becomes active on screen
        try
        {
            box.OnActivated();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[BoxQueue] ActivateBox -> OnActivated failed: {ex.Message}");
        }

        TryTakeScrewsFromArray(box);
    }

    private void SpawnBoxIntoSlot(Box box, BoxSlot slot, bool snap = false)
    {
        Debug.Log("[BOX QUEUE] Spawning box of color " + box?.Color + " into slot. Box null? " +
                  (box == null) + ", Slot locked? " + slot?.isLocked + "Total screw in " + box.RemainingCapacity);
        if (box == null) return;

        slot.AddBox(box);
        box.gameObject.SetActive(true);

        // ── Resolve hidden screws TRƯỚC KHI move ──────────────────
        // Screw được add vào box ngay lập tức (không animation move)
        // Box sẽ mang screws theo khi di chuyển vào slot
        ResolveAllHiddenForBox(box);

        if (snap)
        {
            box.transform.position = slot.transform.position;
            ActivateBox(box);
        }
        else
        {
            Vector3 offScreenStart = GetOffScreenLeft(slot.transform.position.y);
            box.transform.position = offScreenStart;

            box.MoveTo(slot.transform.position, 0.4f, () => ActivateBox(box));
        }
    }

    /// <summary>
    /// Resolve hidden screws từ CẢ HAI nguồn:
    /// 1. BoxQueue._hiddenByColor (screw bị hide bởi BoxQueue)
    /// 2. ScrewManager._hiddenByColor (screw bị hide bởi Breaker item)
    /// Gọi TRƯỚC khi box bắt đầu move → screws di chuyển cùng box.
    /// </summary>
    private void ResolveAllHiddenForBox(Box box)
    {
        if (box == null || box.IsFull) return;

        var color = box.Color;

        // ── Nguồn 1: BoxQueue internal hidden ─────────────────────
        if (_hiddenByColor.TryGetValue(color, out var localList) && localList.Count > 0)
        {
            foreach (var screw in localList.ToList())
            {
                if (box.IsFull) break;
                localList.Remove(screw);
                screw.SetActive(true);
                // Immediate — không animate, screw di chuyển cùng box khi box move vào slot
                box.TryAddScrewImmediate(screw);
            }
            if (localList.Count == 0)
                _hiddenByColor.Remove(color);

            Debug.Log($"[BoxQueue] ResolveHidden (local): snapped screws color={color} to box.");
        }

        // ── Nguồn 2: ScrewManager hidden (từ Breaker item) ────────
        var sm = LevelManager.ins?.ScrewManager;
        if (sm != null && !box.IsFull)
        {
            int remaining = box.RemainingCapacity;
            var fromBreaker = sm.PopHiddenScrew(color, remaining);

            foreach (var screw in fromBreaker)
            {
                if (box.IsFull) break;
                screw.SetActive(true);
                // Immediate — không animate
                box.TryAddScrewImmediate(screw);
            }

            if (fromBreaker.Count > 0)
                Debug.Log($"[BoxQueue] ResolveHidden (breaker): snapped {fromBreaker.Count} " +
                          $"screw(s) color={color} to box.");
        }
    }
    /// <summary>
    /// Trả về vị trí world bên trái ngoài màn hình với y cho trước.
    /// </summary>
    private Vector3 GetOffScreenLeft(float worldY)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return new Vector3(-20f, worldY, 0f);

        // Viewport (0,0) = góc dưới trái, x = -0.2f để hoàn toàn ngoài màn
        Vector3 offScreen = cam.ViewportToWorldPoint(new Vector3(-0.2f, 0f, cam.nearClipPlane));
        return new Vector3(offScreen.x, worldY, 0f);
    }
    internal void TrySpawnNext()
    {
        if (!_sequence.HasNext()) return;

        var freeSlot = slots.FirstOrDefault(s => !s.isLocked && !s.isContainingBox);
        if (freeSlot == null) return;

        SpawnBoxIntoSlot(PickNextBox(), freeSlot);
        _layout.AlignSlots(slots, totalWidth);
    }

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
            SpawnBoxIntoSlot(PickNextBox(), lockedSlot);

        _layout.AlignSlots(slots, totalWidth);
    }

    // ─── Take Screws From Array ────────────────────────────────────

    private void TryTakeScrewsFromArray(Box box)
    {
       Debug.Log($"[BoxQueue] Attempting to take screws from ArrayScrew for box color {box.Color}. Array screw : {_arrayScrew == null}");
        if (_arrayScrew == null) return;
        if (box.IsFull || box.Color == ColorEnum.Rainbow) return;

        int available = box.RemainingCapacity;
        if (available <= 0) return;

        var screws = _arrayScrew.TakeByColor(box.Color, available);

        Debug.Log("[BoxQueue] Trying to take screws from ArrayScrew for box color " + box.Color + ". Available: " + available + ", Taken: " + (screws?.Count ?? 0));
        if (screws == null || screws.Count == 0) return;

        foreach (var screw in screws)
        {
            if (box.IsFull) break;
            box.TryAddScrew(screw);
        }

        Debug.Log($"[BoxQueue] Took {screws.Count} screw(s) of color {box.Color} from ArrayScrew into new box.");
    }

    // ─── Hidden Screw ──────────────────────────────────────────────
    private void HideScrew(ScrewController screw)
    {
        var color = screw.GetColor();
        if (!_hiddenByColor.ContainsKey(color))
            _hiddenByColor[color] = new List<ScrewController>();

        // Ensure this screw is removed from LayerManager's screwDict so visibility controller won't reactivate it
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
    /// <summary>
    /// Xóa box theo màu khỏi SEQUENCE (chưa spawn) — gọi trước hoặc sau SpawnInitial().
    /// An toàn hơn RemoveBoxByColor vì không động vào box đang active trên slot.
    /// </summary>
    public int RemoveFromSequenceByColor(ColorEnum targetColor, int count)
    {
        if (count <= 0) return 0;
        if (_sequence is BoxSequenceService seq)
            return seq.RemoveByColor(targetColor, count);

        Debug.LogWarning("[BoxQueue] RemoveFromSequenceByColor: _sequence không phải BoxSequenceService.");
        return 0;
    }
    private void FillToSlotCapacity()
    {
        var freeSlots = slots.Where(s => !s.isLocked && !s.isContainingBox).ToList();
        foreach (var slot in freeSlots)
        {
            if (!_sequence.HasNext()) break;
            SpawnBoxIntoSlot(PickNextBox(), slot);
        }
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
}