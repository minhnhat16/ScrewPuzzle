using ConfigFile;
using Core.Match;
using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Pools;
using Ingame.Screw;
using Level;
using Managers;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Quản lý load/unload level.
///
/// Responsibilities:
///   - Load config từ file (BoxConfig, Level data)
///   - Spawn objects: layer, part, screw, screwManager
///   - Khởi tạo BoxQueue, ArrayScrew khi level start
///   - Reset toàn bộ khi restart
///
/// KHÔNG làm:
///   - Game state (IngameController lo)
///   - Dialog/UI (GameFlowService lo)
///   - Match logic (MatchRouter lo)
/// </summary>
public class LevelManager : SingletonMono<LevelManager>, IResetable, ILevelManager
{
    // ─────────────────────────────────────────
    // Inspector
    // ─────────────────────────────────────────

    [Header("Box & Array")]
    [SerializeField] private BoxQueue boxManager;
    [SerializeField] private ArrayScrew arrayScrew;

    [Header("Scene")]
    [SerializeField] private GameObject screwManagerPrefab;

    [Header("Debug")]
    [SerializeField] private bool isInitDone;

    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    public int currentLevelID;
    public Level.Level currentLevel { get; private set; }

    public bool IsInitDone
    {
        get => isInitDone;
        set => isInitDone = value;
    }

    // ILevelManager
    public int CurrentLevelId => currentLevelID;

    // ─────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────

    public LayerManager layerManager { get; private set; }

    private ScrewManager _screwManager;
    private BaseLevelObject _currentLevelObject;
    private PartSpriteService _spriteService;

    private Dictionary<string, Level.Level> _levels = new();
    private List<BoxConfig> _boxConfigs = new();
    private List<Level.Level> _levelConfigs = new();

    // Expose cho backward compat
    public ScrewManager ScrewManager
    {
        get => _screwManager;
        private set => _screwManager = value;
    }

    // ─────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────

    public override void Awake()
    {
        base.Awake();
        _spriteService = new PartSpriteService();
    }

    // ─────────────────────────────────────────
    // ILevelManager — Init
    // ─────────────────────────────────────────

    public void Init(Action callback)
    {
        isInitDone = false;
        StartCoroutine(LoadConfigFromFile());
        StartCoroutine(LoadLevelOnFile(() =>
        {
            isInitDone = true;
            callback?.Invoke();
        }));
    }

    // ─────────────────────────────────────────
    // ILevelManager — Load
    // ─────────────────────────────────────────

    /// <summary>
    /// Entry point để load level từ menu hay restart.
    /// Setup scene tasks rồi trigger LoadSceneManager.
    /// </summary>
    public void LoadLevel(int levelID, Action callback = null)
    {
        currentLevelID = levelID;

        arrayScrew.SetGameActive(false);
        arrayScrew.SetupSlots(5);

        var preTasks = new List<Func<IEnumerator>>
        {
            () => TaskLoadSpriteIngame(),
            () => TaskLoadObjectFromLevel(levelID, callback),
        };

        TaskManager.ins.AddTask(preTasks);
        LoadSceneManager.ins.LoadSceneByName("InGame", null);
    }

    /// <summary>ILevelManager — dùng từ GameFlowService khi restart</summary>
    public void LoadLevel(int levelId) => LoadLevel(levelId, null);

    public void ResetLevel() => OnReset();

    // ─────────────────────────────────────────
    // Config Loading
    // ─────────────────────────────────────────

    public IEnumerator LoadConfigFromFile(Action callback = null)
    {
        isInitDone = false;
        yield return new WaitForSeconds(1f);

        var boxConfigs = Resources.LoadAll<BoxConfig>("Config/Level");
        foreach (var cfg in boxConfigs)
            _boxConfigs.Add(cfg);

        Debug.Log($"[LevelManager] Loaded {_boxConfigs.Count} BoxConfig assets");
        callback?.Invoke();
    }

    public IEnumerator LoadLevelOnFile(Action callback = null)
    {
        var assets = ResourceManager.ins.GetAllCachedAssets();
        yield return assets;

        _levels.Clear();

        foreach (var pair in assets)
        {
            if (pair.Value is Level.Level level)
                _levels[level.levelId.ToString()] = level;
        }

        _levelConfigs = new List<Level.Level>(_levels.Values);
        Debug.Log($"[LevelManager] Loaded {_levelConfigs.Count} levels");

        callback?.Invoke();
    }

    // ─────────────────────────────────────────
    // Level Data Query
    // ─────────────────────────────────────────

    public Level.Level GetLevelData(int levelId)
    {
        if (_levels.TryGetValue(levelId.ToString(), out var level))
            return level;

        Debug.LogWarning($"[LevelManager] Level {levelId} không tìm thấy.");
        return null;
    }

    // ─────────────────────────────────────────
    // Load Tasks (chạy trong TaskManager)
    // ─────────────────────────────────────────

    private IEnumerator TaskLoadSpriteIngame()
    {
        int id = currentLevelID;
        Task loadPSB = ResourceManager.ins.LoadPSB($"{id}");

        while (!loadPSB.IsCompleted)
            yield return null;

        if (loadPSB.IsFaulted)
        {
            Debug.LogError($"[LevelManager] Load PSB failed: {loadPSB.Exception}");
            yield break;
        }
    }

    private IEnumerator TaskLoadObjectFromLevel(int levelID, Action callback)
    {
        yield return StartCoroutine(LoadGameObjectFromLevel(levelID, () =>
        {
            long userGold = GameManager.instance.GetPlayerGold();
            ViewManager.Instance.SwitchView(ViewIndex.GameView, new GamePlayViewParam
            {
                totalGold = userGold
            });

            if (DataAPIController.instance.IsNewPlayer())
                TutorialManager.ins.StartTutorial();

            callback?.Invoke();
        }));
    }

    // ─────────────────────────────────────────
    // Core Level Loading Pipeline
    // ─────────────────────────────────────────

    public IEnumerator LoadGameObjectFromLevel(int levelId, Action callback = null)
    {
        currentLevelID = levelId;

        // 1. Spawn level container object
        yield return InitializeLevelObject();

        // 2. Lấy level data
        var levelData = GetLevelData(levelId);
        if (levelData == null) yield break;
        currentLevel = levelData;

        // 3. Setup BoxQueue cho level này
        InitializeBoxQueue(levelData);

        // 4. Spawn layers + parts
        yield return LoadLayers(levelData);

        // 5. Spawn ScrewManager + screws
        yield return LoadScrewManagerAndScrews(levelData);

        // 6. Activate physics
        yield return ActivateAllParts();

        // 7. Special mode nếu đã claim
        bool specialClaim = DataAPIController.instance.GetSpecial().claimed;
        if (levelId > 0 && specialClaim)
            InitSpecial();

        // 8. Finish
        StartCoroutine(layerManager.ChangePartState());
        Debug.Log("[LevelManager] Level load complete.");
        callback?.Invoke();
    }

    // ─────────────────────────────────────────
    // Step 1 — Level Object
    // ─────────────────────────────────────────

    private IEnumerator InitializeLevelObject()
    {
        var levelObject = LevelObjectPool.Instance.pool.SpawnNonGravity();
        _currentLevelObject = levelObject;

        layerManager = levelObject.GetComponent<LayerManager>();

        levelObject.transform.SetParent(transform);
        levelObject.transform.localPosition = Vector3.zero;

        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
            layerManager.ClearPartDict();
        }

        yield return new WaitForEndOfFrame();
    }

    // ─────────────────────────────────────────
    // Step 3 — BoxQueue
    // ─────────────────────────────────────────

    private void InitializeBoxQueue(Level.Level levelData)
    {
        boxManager.LoadLevelBoxes(levelData.boxConfig.GetAllRecord());
        // Initialize được gọi sau khi tất cả steps hoàn thành
        // (gọi ở cuối LoadGameObjectFromLevel thay vì đây để đảm bảo thứ tự)
    }

    // ─────────────────────────────────────────
    // Step 4 — Layers & Parts
    // ─────────────────────────────────────────

    private IEnumerator LoadLayers(Level.Level levelData)
    {
        var layers = new List<BaseLayer>();
        var screwDict = new Dictionary<int, List<ScrewController>>();
        var queue = new Queue<BaseLayer>();

        int layerID = 1;
        foreach (var layerData in levelData.layers)
        {
            var layer = CreateLayer(layerID++);

            foreach (var partData in layerData.parts)
                yield return LoadPart(layer, partData, layerManager);

            layer.IsLayerClear = false;
            layer.RegisterPartListener();

            layers.Add(layer);
            queue.Enqueue(layer);
            screwDict[layerData.layerId] = new List<ScrewController>();
        }

        layerManager.screwDict = screwDict;
        layerManager.Layers = layers;
        layerManager.CoverDictToList();

        var vs = layerManager.visibilityController;
        vs.PreViewMin = 0;
        vs.RePreviewMax = layers.Count - 3;
        vs.layerQueue = queue;
    }

    private BaseLayer CreateLayer(int layerID)
    {
        var layer = LayerPool.Instance.pool.SpawnNonGravity();
        layer.name = $"Layer {layerID}";
        layer.transform.SetParent(_currentLevelObject.transform);
        layer.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return layer;
    }

    private IEnumerator LoadPart(BaseLayer layer, BodyPartScriptable partData, LayerManager lm)
    {
        var part = PartPool.Instance.pool.SpawnNonGravity();
        part = part.gameObject.GetComponent<BasePart>();

        if (part == null) yield break;

        SetupPartTransform(part, partData, layer.transform);
        part.uniqueID = partData.partName;
        part.name = partData.partName;

        lm.AddPart(part);
        SetupPartSprite(part, partData, layer.name);
        SetupPartPhysics(part, partData);

        layer.Parts.Add(part);
        yield return null;
    }

    private void SetupPartTransform(BasePart part, BodyPartScriptable data, Transform parent)
    {
        part.transform.SetParent(parent);
        part.transform.SetLocalPositionAndRotation(data.partPosition, data.partRotation);
        part.transform.localScale = data.partLocalScale;
    }

    private void SetupPartSprite(BasePart part, BodyPartScriptable data, string layerName)
    {
        part.gameObject.layer = LayerMask.NameToLayer(layerName);
        string fixedName = layerName.Replace(" ", "_");

        part.Renderer.sprite = _spriteService.GetPartSprite(6, data.spriteName, fixedName, false);
        part.Outline.sprite = _spriteService.GetPartSprite(6, data.spriteName, fixedName, true);
    }

    private void SetupPartPhysics(BasePart part, BodyPartScriptable data)
    {
        part.GenerateColliderFromSprite();
        part.SetSortingLayer(data.layer);
        part.Body.bodyType = RigidbodyType2D.Static;
    }

    // ─────────────────────────────────────────
    // Step 5 — ScrewManager & Screws
    // ─────────────────────────────────────────

    private IEnumerator LoadScrewManagerAndScrews(Level.Level levelData)
    {
        var go = Instantiate(screwManagerPrefab, _currentLevelObject.transform);
        go.transform.SetPositionAndRotation(new Vector3(0, -5, 0), Quaternion.identity);

        _screwManager = go.GetComponent<ScrewManager>();
        _screwManager.OnScrewRemoved += HandleScrewRemoved;

        foreach (var screwData in levelData.screws)
            yield return LoadScrew(screwData);

        yield return StartCoroutine(RegisterTutorialTargetsIfNewPlayer());
    }

    private IEnumerator LoadScrew(ScrewScriptable screwData)
    {
        var screw = ScrewPool.Instance.Pool.SpawnNonGravity();
        if (screw == null)
        {
            Debug.LogError("[LevelManager] SpawnNonGravity trả về null screw");
            yield break;
        }

        screw.OnReset();
        screw.transform.SetParent(_screwManager.transform);
        screw.transform.SetLocalPositionAndRotation(screwData.screwPosition, Quaternion.identity);
        screw.ChangeScrewColor(screwData.idColor);

        // Tạo hinge
        var lm = _currentLevelObject.GetComponent<LayerManager>();
        var connectedPart = lm != null ? lm.GetPartByKey(screwData.hinge.bodyPartUniqueID) : null;
        var connectedRb = connectedPart != null ? connectedPart.GetComponent<Rigidbody2D>() : null;

        var hinge = screw.CreateHinge(connectedRb, screwData.hinge);
        if (hinge == null)
            Debug.LogWarning($"[LevelManager] CreateHinge null cho screw {screw.name}");

        _screwManager.AddHingeConnection(hinge, connectedPart);

        // Đăng ký vào layer dict
        if (lm != null && connectedPart != null)
        {
            int partLayer = lm.GetPartByKey(screwData.hinge.bodyPartUniqueID).PartLayer() - 10;

            var sr = screw.GetComponent<ScrewRender>();
            if (sr != null) sr.SetSortingOrderAndLayer(partLayer, "Screw");

            if (!lm.screwDict.ContainsKey(partLayer))
                lm.screwDict[partLayer] = new List<ScrewController>();
            lm.screwDict[partLayer].Add(screw);
        }

        _screwManager.AddScrew(screw);
        yield return screw.Init();
        yield return null;
    }

    // ─────────────────────────────────────────
    // Step 6 — Activate Physics
    // ─────────────────────────────────────────

    private IEnumerator ActivateAllParts()
    {
        foreach (var part in _currentLevelObject.GetComponentsInChildren<BasePart>())
        {
            part.Body.bodyType = RigidbodyType2D.Dynamic;
            part.Body.gravityScale = 0f;
            var l = part.PartLayer();
            part.SetIgnoreColliderLayer(true, l, l);
            yield return null;
        }
        yield return new WaitForEndOfFrame();
    }

    // ─────────────────────────────────────────
    // Step 7 — Special Mode
    // ─────────────────────────────────────────

    private void InitSpecial()
    {
        // TODO: khi SideMission được bật lại
        // var mission = SideMissionManager.ins.GenerateColorMission(currentLevel);
        // if (mission != null) boxManager.EnableSpecialMode(mission);
    }

    // ─────────────────────────────────────────
    // Tutorial
    // ─────────────────────────────────────────

    private IEnumerator RegisterTutorialTargetsIfNewPlayer()
    {
        if (!DataAPIController.instance.IsNewPlayer()) yield break;

        yield return null;
        var used = new HashSet<ScrewController>();

        RegisterTarget("screw_blue_1", FindScrewBy(ColorEnum.Blue, used), used);
        yield return null;
        RegisterTarget("screw_blue_2", FindScrewBy(ColorEnum.Blue, used), used);
        yield return null;
        RegisterTarget("screw_green_1", FindScrewBy(ColorEnum.Brown, null), null);
    }

    private void RegisterTarget(string key, ScrewController screw, HashSet<ScrewController> used)
    {
        if (screw == null)
        {
            Debug.LogWarning($"[LevelManager] Tutorial target '{key}' not found");
            return;
        }

        TutorialTargetRegistry.Register(key, screw.transform);
        used?.Add(screw);
    }

    public ScrewController FindScrewBy(ColorEnum color, HashSet<ScrewController> exclude = null)
    {
        if (layerManager == null) return null;

        int topLayer = layerManager.GetTopVisibleLayer();
        if (!layerManager.screwDict.TryGetValue(topLayer, out var screws)) return null;

        foreach (var screw in screws)
        {
            if (screw == null) continue;
            if (exclude != null && exclude.Contains(screw)) continue;
            if (screw.IsInHold) continue;
            if (screw.IsMoving) continue;
            if (screw.GetColor() != color) continue;
            return screw;
        }
        return null;
    }

    // ─────────────────────────────────────────
    // Reset
    // ─────────────────────────────────────────

    public void OnReset()
    {
        // Return level object về pool
        if (_currentLevelObject != null)
        {
            var lm = _currentLevelObject.GetComponent<LayerManager>();
            if (lm != null) lm.Reset();
            LevelObjectPool.Instance.pool.ReturnToPool(_currentLevelObject);
            _currentLevelObject = null;
        }

        // Reset các system
        boxManager.ResetQueue();
        arrayScrew.OnReset();
        SpecialBoxManager.ins.OnReset();

        if (_screwManager != null) _screwManager.Reset();

        BoxPool.Instance.ReturnAll();
    }

    // ─────────────────────────────────────────
    // Part Operations (dùng từ ItemController)
    // ─────────────────────────────────────────

    public void RemovePartItem(BasePart bp)
    {
        var listScrews = layerManager.GetScrewByPart(bp);
        boxManager.ProcessScrews(listScrews);
        layerManager.RemoveScrewsOnDict(listScrews);
        bp.gameObject.SetActive(false);
        bp.OnStateChanged.Invoke(true, bp);
    }

    internal void RemovePart(BasePart bp)
    {
        layerManager.RemovePart(bp.uniqueID);
    }

    // ─────────────────────────────────────────
    // Screw Utilities
    // ─────────────────────────────────────────

    public Dictionary<int, int> GetScrewCountByColor()
    {
        return currentLevel?.screws
            .GroupBy(s => s.idColor)
            .ToDictionary(g => g.Key, g => g.Count())
            ?? new Dictionary<int, int>();
    }

    public bool ConvertColorToRainbow(ColorEnum color, int requiredCount = 3)
    {
        var screws = _screwManager.Screws
            .Where(s => s.GetColor() == color)
            .Take(requiredCount)
            .ToList();

        if (screws.Count < requiredCount) return false;

        foreach (var s in screws)
            s.ChangeScrewColor(ColorEnum.Rainbow);

        return true;
    }

    // ─────────────────────────────────────────
    // Event Handlers
    // ─────────────────────────────────────────

    private void HandleScrewRemoved(ScrewController screw)
    {
        // Hook cho future logic (analytics, missions...)
    }

    // ─────────────────────────────────────────
    // Utilities
    // ─────────────────────────────────────────

    public bool TryHexToColor(string hex, out Color color)
    {
        color = default;
        if (hex.Length == 8 && ColorUtility.TryParseHtmlString("#" + hex, out color))
            return true;
        return false;
    }
}