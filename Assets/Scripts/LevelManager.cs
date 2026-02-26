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
    public ScrewManager ScrewManager { get; private set; }
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

        if (_boxQueue == null)
            _boxQueue = FindAnyObjectByType<BoxQueue>();

        if (_boxQueue == null)
            Debug.LogError("[LevelManager] ILevelBoxQueue not found! Call Inject() or assign boxQueueRef.");
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

    public void LoadLevel(int levelID, Action callback = null)
    {
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
        ScrewManager?.Reset();

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
        CurrentLevelId = levelID;

        CurrentScore = 0;
        BoardDropCount = 0;
        CurrentCombo = 0;

        var ctx = new LevelContext { LevelId = levelID };

        // Pipeline — tất cả steps nhận _boxQueue qua constructor (DIP)
        var pipeline = new LevelLoadPipeline()
            .AddStep(new InitLevelObjectStep(transform))
            .AddStep(new ResolveLevelDataStep(_repository.Levels))
            .AddStep(new InitBoxQueueStep(_boxQueue))            // inject ILevelBoxQueue
            .AddStep(new LoadLayersStep(_partSpawnService))
            .AddStep(new LoadScrewsStep(screwManagerPrefab, _screwSpawnService))
            .AddStep(new ActivatePartsStep())
            .AddStep(new InitSpecialMissionStep(IngameController.ins, SideMissionManager.ins, _boxQueue))
            .AddStep(new FinalizeStep(_boxQueue));               // inject ILevelBoxQueue as IContainerQueue

        yield return pipeline.Run(ctx, () =>
        {
            currentLevel = ctx.LevelData;
            layerManager = ctx.LayerManager;
            ScrewManager = ctx.ScrewManager;
            Difficulty = ResolveDifficulty(currentLevel);

            Debug.Log($"[LevelManager] Level {levelID} loaded. Difficulty: {Difficulty}");
            callback?.Invoke();
        });

        if (!ctx.IsSuccess)
            Debug.LogError($"[LevelManager] Failed to load level {levelID}: {ctx.ErrorMessage}");
    }

    // ─── Game Events ───────────────────────────────────────────────

    /// <summary>
    /// Player tap screw → xóa khỏi board.
    /// Flow: lấy screws → route vào BoxQueue → remove part → board drop check.
    /// </summary>
    public void RemovePartItem(BasePart bp)
    {
        if (layerManager == null || bp == null) return;

        var screws = layerManager.GetScrewByPart(bp);

        // Dùng _boxQueue interface — không gọi BoxQueue.ins.TryMoveScrewsGroupedByColor
        _boxQueue?.TryMoveScrewsGroupedByColor(screws, fromBoard: true);

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
}

// ─── Enum ──────────────────────────────────────────────────────────────────
public enum LevelDifficulty { Easy, Normal, Hard, Expert }