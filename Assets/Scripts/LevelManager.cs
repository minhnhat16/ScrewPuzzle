using ConfigFile;
using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ┌─────────────────────────────────────────────────────────────────┐
/// │                    LEVEL MANAGER (Refactored)                   │
/// ├─────────────────────────────────────────────────────────────────┤
/// │ Vai trò: COORDINATOR / FACADE                                   │
/// │ - Không tự làm gì nặng                                          │
/// │ - Dựng pipeline → chạy pipeline → expose public API            │
/// │                                                                 │
/// │ SOLID đã áp dụng:                                               │
/// │  SRP: mỗi step/service có 1 trách nhiệm                        │
/// │  OCP: thêm step mới không cần sửa class này                    │
/// │  DIP: phụ thuộc interface (IBoxQueue, IIngameController...)     │
/// └─────────────────────────────────────────────────────────────────┘
/// </summary>
public class LevelManager : SingletonMono<LevelManager>, IResetable
{
    // ─── Inspector Fields ──────────────────────────────────────────
    [SerializeField] private GameObject screwManagerPrefab;

    // ─── Public State ──────────────────────────────────────────────
    public int currentLevelID { get; private set; }
    public Level.Level currentLevel { get; private set; }
    public LayerManager layerManager { get; private set; }
    public ScrewManager ScrewManager { get; private set; }
    public bool IsInitDone { get; private set; }

    // Expose level list cho UI (LevelView dùng)
    public List<Level.Level> levelConfig => _repository.LevelList;

    // ─── Services (khởi tạo trong Awake, không dùng singleton trực tiếp) ──
    private LevelDataRepository _repository;
    private ScrewColorAnalyzer _colorAnalyzer;
    private IPartSpawnService _partSpawnService;
    private IScrewSpawnService _screwSpawnService;

    // ─── Unity Lifecycle ───────────────────────────────────────────

    public override void Awake()
    {
        base.Awake();

        // Khởi tạo services tại đây — không phụ thuộc singleton bên ngoài
        _repository = new LevelDataRepository();
        _colorAnalyzer = new ScrewColorAnalyzer();
        _partSpawnService = new PartSpawnService(new PartSpriteService());
        _screwSpawnService = new ScrewSpawnService();
    }

    // ─── Public API ────────────────────────────────────────────────

    /// <summary>Gọi khi game khởi động để load data từ file.</summary>
    public void Init()
    {
        IsInitDone = false;
        StartCoroutine(_repository.LoadAll(() => IsInitDone = true));
    }

    /// <summary>Load và spawn level theo ID.</summary>
    public void LoadLevel(int levelID, Action callback = null)
    {
        StartCoroutine(LoadLevelCoroutine(levelID, callback));
    }

    /// <summary>Reset toàn bộ level hiện tại.</summary>
    public void OnReset()
    {
        if (layerManager != null)
        {
            layerManager.Reset();
            var levelObj = layerManager.GetComponent<BaseLevelObject>();
            if (levelObj != null)
                LevelObjectPool.Instance.pool.ReturnToPool(levelObj);
        }

        BoxQueue.ins.ClearConfigRecords();
        BoxQueue.ins.ClearCurrentBoxes();
        BoxQueue.ins.OnReset();
        ArrayScrew.Instance.OnReset();
        SpecialBoxManager.ins.OnReset();
        ScrewManager?.Reset();
        ThreeHoldBoxPool.Instance.ReturnAll();

        layerManager = null;
        currentLevel = null;
    }

    /// <summary>Phân tích màu screw của level hiện tại (debug / editor tool).</summary>
    public void LogScrewColorReport()
    {
        if (currentLevel == null) return;
        _colorAnalyzer.LogDivisibilityReport(currentLevel);
    }

    /// <summary>Lấy số screw theo màu của level hiện tại.</summary>
    public Dictionary<int, int> GetScrewCountByColor()
    {
        return currentLevel != null
            ? _colorAnalyzer.GetColorCount(currentLevel)
            : new Dictionary<int, int>();
    }

    // ─── Core Coroutine ────────────────────────────────────────────

    private IEnumerator LoadLevelCoroutine(int levelID, Action callback)
    {
        currentLevelID = levelID;

        // Build context
        var ctx = new LevelContext { LevelId = levelID };

        // Build pipeline — thêm/bỏ step không ảnh hưởng gì khác (OCP)
        var pipeline = new LevelLoadPipeline()
            .AddStep(new InitLevelObjectStep(transform))
            .AddStep(new ResolveLevelDataStep(_repository.Levels))
            .AddStep(new InitBoxQueueStep(BoxQueue.ins))
            .AddStep(new LoadLayersStep(_partSpawnService))
            .AddStep(new LoadScrewsStep(screwManagerPrefab, _screwSpawnService))
            .AddStep(new ActivatePartsStep())
            .AddStep(new InitSpecialMissionStep(IngameController.ins, SideMissionManager.ins, BoxQueue.ins))
            .AddStep(new FinalizeStep(BoxQueue.ins));

        yield return pipeline.Run(ctx, () =>
        {
            // Cập nhật state sau khi pipeline chạy xong
            currentLevel = ctx.LevelData;
            layerManager = ctx.LayerManager;
            ScrewManager = ctx.ScrewManager;

            Debug.Log($"[LevelManager] Level {levelID} loaded successfully.");
            callback?.Invoke();
        });

        // Nếu pipeline thất bại
        if (!ctx.IsSuccess)
            Debug.LogError($"[LevelManager] Failed to load level {levelID}: {ctx.ErrorMessage}");
    }

    // ─── Game Events ───────────────────────────────────────────────

    /// <summary>
    /// Gọi khi screw bị xóa (drop board, match box...).
    /// Tách ra để dễ mở rộng sau: analytics, sfx, score...
    /// </summary>
    public void RemovePartItem(BasePart bp)
    {
        var screws = layerManager.GetScrewByPart(bp);
        BoxQueue.ins.TryMoveScrewsGroupedByColor(screws, true);
        layerManager.RemoveScrewsOnDict(screws);
        layerManager.RemovePart(bp.uniqueID);
        bp.gameObject.SetActive(false);
    }

    public bool TryHexToColor(string hex, out Color color)
    {
        color = default;
        if (hex?.Length == 8)
            return ColorUtility.TryParseHtmlString("#" + hex, out color);
        return false;
    }
}