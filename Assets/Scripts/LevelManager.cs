using ConfigFile;
using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using LevelSystem.Core;
using LevelSystem.Steps;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonMono<LevelManager>, IResetable, ILevelManager
{
    // ─── Inspector Fields ──────────────────────────────────────────
    [SerializeField] private GameObject screwManagerPrefab;
    [SerializeField] private ScrewGameBootstrapper bootstrapper;

    [Header("Dependencies — inject, không dùng singleton trực tiếp")]
    [Tooltip("BoxQueue trong scene — tự động tìm nếu để trống")]
    [SerializeField] private BoxQueue boxQueueRef;

    [Header("Score Config")]
    [Tooltip("Base score per box clear (3 screws matched)")]
    [SerializeField] private int scorePerBoxClear = 100;
    [Tooltip("Bonus score per board layer dropped")]
    [SerializeField] private int scoreBoardDropBonus = 50;
    [Tooltip("Score multiplier applied when combo >= 2")]
    [SerializeField] private float comboMultiplier = 1.5f;

    // ─── Public State ──────────────────────────────────────────────
    public int CurrentLevelId { get; private set; }
    public Level.Level currentLevel { get; private set; }
    public LayerManager layerManager { get; private set; }
        public ScrewManager ScrewManager { get; set; }
    public bool IsInitDone { get; private set; }

    public List<Level.Level> levelConfig => _repository.LevelList;

    // ─── Gameplay Stats ────────────────────────────────────────────
    public int CurrentScore { get; private set; }
    public int BoardDropCount { get; private set; }
    public int CurrentCombo { get; private set; }
    public LevelDifficulty Difficulty { get; private set; }

    // ─── Events ────────────────────────────────────────────────────
    public event Action<int> OnScoreChanged;
    public event Action<int, int> OnBoardDropped;
    public event Action<int> OnComboChanged;

    // ─── Injected dependency (DIP) ─────────────────────────────────
    /// <summary>
    /// Interface được inject — LevelManager không biết BoxQueue cụ thể.
    /// Set từ Inject() hoặc tự resolve từ boxQueueRef trong Awake.
    /// </summary>
    private ILevelBoxQueue _boxQueue;

    // ─── Services ──────────────────────────────────────────────────
    private LevelDataRepository _repository;
    private ScrewColorAnalyzer _colorAnalyzer;
    private IPartSpawnService _partSpawnService;
    private IScrewSpawnService _screwSpawnService;

    // ─── Unity Lifecycle ───────────────────────────────────────────

    public override void Awake()
    {
        base.Awake();

        _repository = new LevelDataRepository();
        _colorAnalyzer = new ScrewColorAnalyzer();
        _partSpawnService = new PartSpawnService(new PartSpriteService());
        _screwSpawnService = new ScrewSpawnService();

        // Resolve BoxQueue: ưu tiên inject từ ngoài, fallback Inspector ref, fallback FindAny
        if (_boxQueue == null && boxQueueRef != null)
            _boxQueue = boxQueueRef;

        _boxQueue ??= FindAnyObjectByType<BoxQueue>();

        if (_boxQueue == null)
            Debug.LogError("[LevelManager] ILevelBoxQueue not found! Call Inject() or assign boxQueueRef.");

        // REMOVED: _boxQueue.Setup() — ScrewGameBootstrapper.InitializeForLevel() đã gọi Setup() rồi.
        // Gọi 2 lần sẽ thay thế _sequence bằng instance mới rỗng.
    }

    // ─── Dependency Injection ──────────────────────────────────────

    /// <summary>
    /// Inject BoxQueue từ Bootstrapper (preferred) thay vì dùng Inspector.
    /// Gọi trước Init() và LoadLevel().
    /// </summary>
    public void Inject(ILevelBoxQueue boxQueue)
    {
        _boxQueue = boxQueue;
    }

    // ─── Public API ────────────────────────────────────────────────

    public void Init(Action callback)
    {
        IsInitDone = false;
        StartCoroutine(_repository.LoadAll(() =>
        {
            IsInitDone = true;
            callback.Invoke();
        }));
    }
    public void ReLoadLevel()
    {
        Dispose();
        if (CurrentLevelId > 0)
            LoadLevel(CurrentLevelId);
    }
    public void LoadLevel(int levelID, Action callback = null)
    {
        Debug.Log($"[LevelManager] Starting to load level {levelID}...");
        StartCoroutine(LoadLevelCoroutine(levelID, callback));
    }

    public void OnReset()
    {
        CurrentScore = 0;
        BoardDropCount = 0;
        CurrentCombo = 0;
        Difficulty = LevelDifficulty.Normal;

        if (layerManager != null)
        {
            layerManager.Reset();
            var levelObj = layerManager.GetComponent<BaseLevelObject>();
            if (levelObj != null)
                LevelObjectPool.Instance.pool.ReturnToPool(levelObj);
        }

        // Dùng _boxQueue interface — không gọi BoxQueue.ins
        _boxQueue?.ClearConfigRecords();
        _boxQueue?.ClearCurrentBoxes();
        _boxQueue?.OnReset();

        SpecialBoxManager.ins.OnReset();
        ScrewManager.Reset();

        // Reset ArrayScrew: clear held screws + restore default slots
        ArrayScrew.ins.OnReset();

        layerManager = null;
        currentLevel = null;
    }

    public void LogScrewColorReport()
    {
        if (currentLevel == null) return;
        _colorAnalyzer.LogDivisibilityReport(currentLevel);
    }

    public Dictionary<int, int> GetScrewCountByColor()
    {
        return currentLevel != null
            ? _colorAnalyzer.GetColorCount(currentLevel)
            : new Dictionary<int, int>();
    }

    // ─── Core Coroutine ────────────────────────────────────────────

    private IEnumerator LoadLevelCoroutine(int levelID, Action callback)
    {
        // ── REMOVED: InitializeForLevel() call — already called from LevelStartService ──
        // ScrewGameBootstrapper.ins.InitializeForLevel() is called once from LevelStartService,
        // not here. Calling it here causes double initialization.

        CurrentLevelId = levelID;

        CurrentScore = 0;
        BoardDropCount = 0;
        CurrentCombo = 0;

        var ctx = new LevelContext { LevelId = levelID };

        // Pipeline — tất cả steps nhận _boxQueue qua constructor (DIP)
        var pipeline = new LevelLoadPipeline()
            .AddStep(new LoadPsbStep())                  // ← load PSB trước khi spawn parts
            .AddStep(new InitLevelObjectStep(transform))
            .AddStep(new ResolveLevelDataStep(_repository.Levels))
            .AddStep(new InitBoxQueueStep(_boxQueue))
            .AddStep(new LoadLayersStep(_partSpawnService))
            .AddStep(new LoadScrewsStep(screwManagerPrefab, _screwSpawnService))
            .AddStep(new ActivatePartsStep())
            .AddStep(new InitSpecialMissionStep(IngameController.ins, SideMissionManager.ins, _boxQueue))
            .AddStep(new ActivatePartsStep())
            .AddStep(new FinalizeStep(_boxQueue));

        yield return pipeline.Run(ctx, () =>
        {
            currentLevel = ctx.LevelData;
            layerManager = ctx.LayerManager;
            ScrewManager = ctx.ScrewManager;
            Difficulty = ResolveDifficulty(currentLevel);

            this.layerManager = layerManager;
            Debug.Log($"[LevelManager] Level {levelID} loaded. Difficulty: {Difficulty}");
            callback?.Invoke();
        });

        if (!ctx.IsSuccess)
            Debug.LogError($"[LevelManager] Failed to load level {levelID}: {ctx.ErrorMessage}");
    }

    // ─── Game Events ───────────────────────────────────────────────
    // <summary>
    /// Breaker item: remove part + tất cả screw gắn với nó.
    /// Screw nào có box cùng màu đang active → route vào box.
    /// Screw nào không match → ẩn vào ScrewManager hidden (không vào ArrayScrew).
    /// </summary>
    public void RemovePartItem(BasePart bp)
    {
        if (layerManager == null || bp == null) return;

        var screws = layerManager.GetScrewByPart(bp);
        if (screws == null || screws.Count == 0)
        {
            // Không có screw → chỉ remove part
            layerManager.RemovePart(bp.uniqueID);
            bp.gameObject.SetActive(false);
            return;
        }

        var routed = new List<ScrewController>();
        var hidden = new List<ScrewController>();

        foreach (var screw in screws)
        {
            if (screw == null) continue;

            // Tìm box phù hợp màu đang active
            var box = BoxQueue.ins.FindSuitableBox(screw.GetColor());
            if (box != null && box.TryAddScrew(screw))
            {
                routed.Add(screw);
            }
            else
            {
                // Không có box match → ẩn vào hidden, không vào ArrayScrew
                screw.SetActive(false);
                hidden.Add(screw);
            }
        }

        if (hidden.Count > 0)
        {
            ScrewManager.AddHiddenScrews(hidden);
            Debug.Log($"[LevelManager] Breaker: {hidden.Count} screw(s) hidden (no matching box).");
        }

        if (routed.Count > 0)
            Debug.Log($"[LevelManager] Breaker: {routed.Count} screw(s) routed to box.");

        layerManager.RemoveScrewsOnDict(screws);
        layerManager.RemovePart(bp.uniqueID);
        bp.gameObject.SetActive(false);
    }

    /// <summary>
    /// Gọi khi BoxQueue clear 1 box (3 screws match).
    /// Tăng combo, tính điểm.
    /// </summary>
    public void OnBoxCleared()
    {
        CurrentCombo++;
        float mult = CurrentCombo >= 2 ? comboMultiplier : 1f;
        int gained = Mathf.RoundToInt(scorePerBoxClear * mult);
        AddScore(gained);
        OnComboChanged?.Invoke(CurrentCombo);
        Debug.Log($"[LevelManager] Box cleared | Combo:{CurrentCombo} | +{gained}pts");
    }

    /// <summary>
    /// Win khi: sequence hết box VÀ không còn box active nào trên slot.
    /// </summary>
    public void CheckWinCondition()
    {
        Debug.Log("[LevelManager] CheckWinCondition called.");
        IngameController.ins.OnLevelCompleted.Invoke(true);
    }

    /// <summary>
    /// Gọi khi board layer drop.
    /// → Gọi từ LayerManager khi layer hết screw.
    /// </summary>
    public void OnBoardLayerDropped(int layerIndex)
    {
        BoardDropCount++;
        AddScore(scoreBoardDropBonus);
        OnBoardDropped?.Invoke(BoardDropCount, layerIndex);
        Debug.Log($"[LevelManager] Board drop | Layer:{layerIndex} | Drops:{BoardDropCount}");
    }

    /// <summary>
    /// Gọi khi screw vào queue không match được.
    /// Reset combo.
    /// </summary>
    public void OnScrewQueued()
    {
        if (CurrentCombo <= 0) return;
        CurrentCombo = 0;
        OnComboChanged?.Invoke(CurrentCombo);
        Debug.Log("[LevelManager] Combo reset.");
    }

    public bool TryHexToColor(string hex, out Color color)
    {
        color = default;
        if (hex?.Length == 8)
            return ColorUtility.TryParseHtmlString("#" + hex, out color);
        return false;
    }

    // ─── Helpers ───────────────────────────────────────────────────

    private void AddScore(int amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    private LevelDifficulty ResolveDifficulty(Level.Level level)
    {
        if (level == null) return LevelDifficulty.Normal;
        var map = _colorAnalyzer.GetColorCount(level);
        int total = 0;
        foreach (var v in map.Values) total += v;
        int colors = map.Count;

        if (total <= 30 && colors <= 3) return LevelDifficulty.Easy;
        else if (total <= 60 && colors <= 5) return LevelDifficulty.Normal;
        else if (total <= 90 && colors <= 7) return LevelDifficulty.Hard;
        else return LevelDifficulty.Expert;
    }

    public Level.Level GetLevelData(int levelId)
    {
        return levelConfig[levelId];
    }

    public void Dispose()
    {
        OnReset();
        // Cleanup if needed
    }
}

// ─── Enum ──────────────────────────────────────────────────────────────────
public enum LevelDifficulty { Easy, Normal, Hard, Expert }