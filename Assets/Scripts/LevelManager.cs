using ConfigFile;
using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Pools;
using Ingame.Screw;
using Level;
using Managers;
using PoolManager;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

public class LevelManager : SingletonMono<LevelManager>, IResetable, ILevelManager
{
    [SerializeField] private BoxQueue boxManager;
    public LayerManager layerManager;
    [SerializeField] private bool isInitDone;
    public int currentLevelID;

    public Dictionary<string, Level.Level> Levels = new Dictionary<string, Level.Level>();
    private List<BoxConfig> boxconfigsLevel = new List<BoxConfig>();
    public List<Level.Level> levelConfig = new List<Level.Level>();
    public Level.Level currentLevel;

    [SerializeField] private GameObject screwManagerPrefb;
    private ScrewManager screwManager;

    [SerializeField] private BaseLevelObject currentLevelObject;

    private PartSpriteService spriteService;

    [SerializeField]
    private ArrayScrew arrayScrew;

    public bool IsInitDone
    {
        get => isInitDone;
        set => isInitDone = value;
    }
    public ScrewManager ScrewManager { get => screwManager; set => screwManager = value; }

    public int CurrentLevelId => throw new NotImplementedException();

    public override void Awake()
    {
        base.Awake();
        this.screwManager = GetComponent<ScrewManager>();
        spriteService = new PartSpriteService();
    }

    public void Start()
    {
    }

    public void Init(Action callback)
    {
        isInitDone = false;
        StartCoroutine(LoadConfigFromFile());
        StartCoroutine(LoadLevelOnFile(() => { isInitDone = true; callback?.Invoke(); })); 

    }
    public IEnumerator LoadConfigFromFile(Action callback = null)
    {
        isInitDone = false;
        yield return new WaitForSeconds(1f);

        // Path should not include "Resources" or file extension
        string resourcePath = "Config/Level"; // Assuming files are in Resources/ConfigFile/Level

        // Load all assets of type BoxConfig from the specified folder
        var boxConfigs = Resources.LoadAll<BoxConfig>(resourcePath);

        if (boxConfigs.Length > 0)
        {
            foreach (var boxConfig in boxConfigs)
            {
                boxconfigsLevel.Add(boxConfig);
            }
        }
        Debug.Log($"Loaded {boxconfigsLevel.Count} BoxConfig assets from {resourcePath}");
        // Call the callback if it's not null
        callback?.Invoke();
    }

    public IEnumerator LoadLevelOnFile(Action callback = null)
    {
        var assets = ResourceManager.ins.GetAllCachedAssets();
        // Đảm bảo Addressables đã init & preload
        yield return assets;

        Levels.Clear();


        foreach (var pair in assets)
        {
            if (pair.Value is Level.Level level)
            {
                string id = level.levelId.ToString();
                Levels[id] = level;
            }
        }

        levelConfig = new List<Level.Level>(Levels.Values);

        Debug.Log($"[Level] Loaded remote levels: {levelConfig.Count}");

        callback?.Invoke();
    }


    public void LoadLevel(int levelID, Action callback = null)
    {
        //boxManager.activeBoxCount = 2;
        currentLevelID = levelID;
        arrayScrew.ShowArrayScrew();
        OnReset();
        arrayScrew.HoldAlignment();

        List<Func<IEnumerator>> preTasks = new List<Func<IEnumerator>>()
        {
            () => TaskLoadSpriteIngame(),
            () => TaskLoadObjectFromLevel(levelID),
        };
        TaskManager.ins.AddTask(preTasks);
        LoadSceneManager.ins.LoadSceneByName("InGame", null);
    }
    public IEnumerator TaskLoadSpriteIngame()
    {

        Debug.Log("current level " + currentLevelID);
        int id = currentLevelID == 0 ? 0 : currentLevelID;
        Task loadPSB = ResourceManager.ins.LoadPSB($"{id}"); // ❗ KHÔNG .psb

        while (!loadPSB.IsCompleted)
            yield return null;

        if (loadPSB.IsFaulted)
        {
            Debug.LogError(loadPSB.Exception);
            yield break;
        }

        Debug.Log("Load psb SUCCESS");
    }
    public IEnumerator TaskLoadObjectFromLevel(int levelID)
    {
        yield return StartCoroutine(LoadGameObjectFromLevel(levelID, () =>
          {
              long userGold = GameManager.instance.GetPlayerGold();
              GamePlayViewParam param = new();
              param.totalGold = userGold;
              ViewManager.Instance.SwitchView(ViewIndex.GameView, param);
              var newPlayer = DataAPIController.instance.IsNewPlayer();
              if (newPlayer)
              {
                  TutorialManager.ins.StartTutorial();
              }
          }));
    }
    public Dictionary<int, int> GetScrewCountByColor()
    {
        Dictionary<int, int> screwColorDict = new Dictionary<int, int>();

        // Iterate through each screw and count by color
        foreach (var screw in currentLevel.screws)
        {
            if (screwColorDict.ContainsKey(screw.idColor))
            {
                screwColorDict[screw.idColor]++;
            }
            else
            {
                screwColorDict[screw.idColor] = 1; // First occurrence of this color
            }
        }

        return screwColorDict;
    }

    // Method to calculate screws divisible by 3 and remainder
    public void CalculateScrewsDivisibleBy3()
    {
        var screwColorDict = GetScrewCountByColor();

        foreach (var kvp in screwColorDict)
        {
            int colorId = kvp.Key;
            int totalScrews = kvp.Value;

            int screwsDivisibleBy3 = totalScrews / 3;
            int remainderScrews = totalScrews % 3;

            Debug.Log(
                $"Color ID: {colorId}, Total Screws: {totalScrews}, Divisible by 3: {screwsDivisibleBy3}, Remainder: {remainderScrews}");
        }
    }

    public IEnumerator LoadGameObjectFromLevel(int levelId, Action callback = null)
    {
        currentLevelID = levelId;

        Debug.Log("Step 1: Initializing Level Object");
        // Step 1: Initialize Level Object
        yield return InitializeLevelObject();
        //Debug.Log("Step 1 complete");

        Debug.Log("Step 2: Retrieving Level Data");
        // Step 2: Retrieve Level Data
        var levelData = GetLevelData(levelId);
        if (levelData == null)
        {
            //Debug.LogWarning("Level data is null. Exiting coroutine.");
            yield break;
        }
        currentLevel = levelData;

        Debug.Log("Step 3: Initializing Box Queue");
        InitializeBoxQueue(levelData);

        Debug.Log("Step 4: Loading Layers");
        yield return LoadLayers(levelData);

        Debug.Log("Step 5: Loading Screw Manager and Screws");
        yield return LoadScrewManagerAndScrews(levelData);

        Debug.Log("Step 6: Activating All Parts");
        yield return ActivateAllParts();

        bool specialClaim = DataAPIController.instance.GetSpecial().claimed;
        if (levelId > 0 && specialClaim)
        {
            InitSpecial();
        }

        StartCoroutine(layerManager.ChangePartState());
        boxManager.Initialize(levelData.levelId == 0);
        Debug.Log("Level loading complete.");
        callback?.Invoke();
        //Debug.Log($"Level data with ID {levelId} loaded.");
    }

    public void InitSpecial()
    {
        //int requireSpecial = IngameController.ins.;

        //layerManager.visibilityController.ApplyLayerVisibility();

        //SideMission mission =
        //    SideMissionManager.ins.GenerateColorMission(
        //        currentLevel,
        //        requireSpecial);

        //IngameController.ins.SetSideMission(mission);

        //boxManager.EnableSpecialMode(mission);
    }
    private IEnumerator InitializeLevelObject()
    {
        var levelObject = LevelObjectPool.Instance.pool.SpawnNonGravity();
        currentLevelObject = levelObject;
        this.layerManager = currentLevelObject.GetComponent<LayerManager>();
        levelObject.transform.SetParent(transform);
        levelObject.transform.localPosition = Vector3.zero;

        var layerManager = levelObject.GetComponent<LayerManager>();
        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
            layerManager.ClearPartDict();
        }

        yield return new WaitForEndOfFrame();
    }
    // Fix for UNT0008: Unity objects should not use null propagation.
    // Replace all usages of ?. (null propagation) on UnityEngine.Object-derived types with explicit null checks.

    // 1. In LoadConfigFromFile and LoadLevelOnFile, the callback?.Invoke() is safe because Action is not a Unity object.
    // 2. In LoadGameObjectFromLevel, GetLevelData, and other methods, check for ?. usage on UnityEngine.Object types.

    public Level.Level GetLevelData(int levelId)
    {
        Level.Level levelData;
        Levels.TryGetValue(levelId.ToString(), out levelData);
        if (levelData == null)
        {
            Debug.LogWarning($"Level with ID {levelId} not found!");
        }
        return levelData;
    }
    private void InitializeBoxQueue(Level.Level levelData)
    {
        var boxConfig = levelData.boxConfig;
        boxManager.LoadLevelBoxes(boxConfig.GetAllRecord());
        boxManager.Initialize(levelData.levelId == 0);
    }
    private IEnumerator LoadLayers(Level.Level levelData)
    {
        int layerID = 1;
        List<BaseLayer> listBaseLayer = new();

        var layerScrewsDict = new Dictionary<int, List<ScrewController>>();
        var queue = new Queue<BaseLayer>();
        foreach (var layerData in levelData.layers)
        {
            var layerComponent = CreateLayer(layerID++);
            foreach (var partData in layerData.parts)
            {
                yield return LoadPart(layerComponent, partData, layerManager);
            }
            layerComponent.IsLayerClear = false;
            layerComponent.RegisterPartListener();
            listBaseLayer.Add(layerComponent);

            queue.Enqueue(layerComponent);
            Debug.Log($"Layer {layerData.layerId} loaded with {layerData.parts.Count} parts.");
            layerScrewsDict.Add(layerData.layerId, new List<ScrewController>());
        }
        layerManager.screwDict = layerScrewsDict;



        Debug.Log($"All layers loaded. Total layers: {layerScrewsDict.Count}. Keys: {string.Join(", ", layerScrewsDict.Keys.Select(k => k.ToString()))}");



        layerManager.Layers = listBaseLayer;
        layerManager.CoverDictToList();
        LayerVisibilityController vs = layerManager.visibilityController;
        vs.PreViewMin = 0;
        vs.RePreviewMax = listBaseLayer.Count - 3;
        vs.layerQueue = queue;
        Debug.Log("Applying layer visibility settings..." + listBaseLayer.Count);

    }
    private BaseLayer CreateLayer(int layerID)
    {
        var layerComponent = LayerPool.Instance.pool.SpawnNonGravity();
        layerComponent.name = $"Layer {layerID}";
        layerComponent.transform.SetParent(currentLevelObject.transform);
        layerComponent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return layerComponent;
    }
    private IEnumerator LoadPart(BaseLayer layerComponent, BodyPartScriptable partData, LayerManager layerManager)
    {
        var partComponent = PartPool.Instance.pool.SpawnNonGravity();
        var partGameObject = partComponent.gameObject;
        int layerIndex = LayerMask.NameToLayer(layerComponent.name);

        partComponent = partGameObject.GetComponent<BasePart>();

        if (partGameObject != null && partComponent != null)
        {
            SetupPartTransform(partComponent, partData, layerComponent.transform);
            partComponent.uniqueID = partData.partName;
            partComponent.name = partData.partName;
            layerManager.AddPart(partComponent);

            string layerName = layerComponent.name; ;
            SetupPartSprite(partComponent, partData, layerName);
            //if (TryHexToColor(partData.colorString, out Color color))
            //{
            //    color.a = 0.5f;

            //    Debug.Log("Color in part " + color.a);
            //    partComponent.Renderer.color = color;
            //}
            SetupPartPhysics(partComponent, partData);
            layerComponent.Parts.Add(partComponent);

        }

        Debug.Log($"Loading part {partData.partName} at position {partData.partPosition}," +
            $" and layer {partData.layer}, and real layer {partComponent.Renderer.sortingLayerName}");

        yield return null;
    }

    private void SetupPartTransform(BasePart part, BodyPartScriptable data, Transform parent)
    {
        part.transform.SetParent(parent.transform);
        part.transform.SetLocalPositionAndRotation(data.partPosition, data.partRotation);
        part.transform.localScale = data.partLocalScale;
    }
    private void SetupPartSprite(BasePart part, BodyPartScriptable data, string layerName)
    {
        part.gameObject.layer = LayerMask.NameToLayer(layerName);
        string fixedName = layerName.Replace(" ", "_");

        var sprite = spriteService.GetPartSprite(6, data.spriteName, fixedName, false);

        Debug.Log($"  Loading part '{data.partName}' with sprite '{data.spriteName}', sprite {sprite == null}");

        var outline = spriteService.GetPartSprite(6, data.spriteName, fixedName, true);

        part.Renderer.sprite = sprite;
        part.Outline.sprite = outline;
    }
    private void SetupPartPhysics(BasePart part, BodyPartScriptable data)
    {
        part.GenerateColliderFromSprite();
        part.SetSortingLayer(data.layer);
        part.Body.bodyType = RigidbodyType2D.Static;

    }
    private IEnumerator LoadScrewManagerAndScrews(Level.Level levelData)
    {
        var screwManagerGameObject = Instantiate(screwManagerPrefb, currentLevelObject.transform) as GameObject;
        screwManagerGameObject.transform.SetPositionAndRotation(new Vector3(0, -5, 0), Quaternion.identity);
        ScrewManager = screwManagerGameObject.GetComponent<ScrewManager>();
        ScrewManager.OnScrewRemoved += HandleScrewRemoved;
        foreach (var screwData in levelData.screws)
        {
            yield return LoadScrew(screwData);
        }

        // Move tutorial registration to a dedicated coroutine for clarity and testability
        yield return StartCoroutine(RegisterTutorialTargetsIfNewPlayer());
    }
    /// <summary>
    /// Register tutorial targets if the current player is new.
    /// Extracted from LoadScrewManagerAndScrews to keep responsibilities small and make behavior explicit.
    /// </summary>
    private IEnumerator RegisterTutorialTargetsIfNewPlayer()
    {
        if (!DataAPIController.instance.IsNewPlayer())
            yield break;

        Debug.Log("[LevelManager] Registering tutorial targets for new player");

        HashSet<ScrewController> used = new();

        // small delay to let screws settle in scene
        yield return null;
        var blue1 = FindScrewBy(ColorEnum.Blue, used);
        if (blue1 != null)
        {
            used.Add(blue1);
            TutorialTargetRegistry.Register("screw_blue_1", blue1.transform);
        }
        else
        {
            Debug.LogWarning("[LevelManager] tutorial: blue1 not found");
        }

        yield return null;
        var blue2 = FindScrewBy(ColorEnum.Blue, used);
        if (blue2 != null)
        {
            used.Add(blue2);
            TutorialTargetRegistry.Register("screw_blue_2", blue2.transform);
        }
        else
        {
            Debug.LogWarning("[LevelManager] tutorial: blue2 not found");
        }

        yield return null;
        var green1 = FindScrewBy(ColorEnum.Brown);
        if (green1 != null)
        {
            TutorialTargetRegistry.Register("screw_green_1", green1.transform);
        }
        else
        {
            Debug.LogWarning("[LevelManager] tutorial: green1 not found");
        }

        Debug.Log("[LevelManager] Tutorial targets registration finished");
    }
    public ScrewController FindScrewBy(
    ColorEnum color,
    HashSet<ScrewController> exclude = null
)
    {
        if (layerManager == null)
            return null;

        int topLayer = layerManager.GetTopVisibleLayer();

        if (!layerManager.screwDict.TryGetValue(topLayer, out var screws))
            return null;

        foreach (var screw in screws)
        {
            screw.IsClicked = true;
            if (screw == null) continue;
            if (exclude != null && exclude.Contains(screw)) continue;
            if (screw.IsInHold) continue;
            if (screw.IsMoving) continue;
            if (screw.GetColor() != color) continue;

            screw.IsClicked = false;
            return screw;
        }

        return null;
    }

    private IEnumerator LoadScrew(ScrewScriptable screwData)
    {
        var screw = ScrewPool.Instance.Pool.SpawnNonGravity();
        if (screw == null)
        {
            Debug.LogError("[LevelManager] SpawnNonGravity returned null screw");
            yield break;
        }

        var screwGameObject = screw.gameObject;
        screw.OnReset();

        if (ScrewManager == null)
        {
            Debug.LogError("[LevelManager] ScrewManager is null when spawning screw");
            yield break;
        }

        screwGameObject.transform.SetParent(ScrewManager.transform);
        screwGameObject.transform.SetLocalPositionAndRotation(screwData.screwPosition, Quaternion.identity);

        screw.ChangeScrewColor(screwData.idColor);

        var hingeConnection = screwData.hinge;
        if (currentLevelObject == null)
        {
            Debug.LogError("[LevelManager] currentLevelObject is null when creating hinge");
            yield break;
        }

        LayerManager lm = currentLevelObject.GetComponent<LayerManager>();
        BasePart connectedPart = lm != null ? lm.GetPartByKey(hingeConnection.bodyPartUniqueID) : null;

        Rigidbody2D connectedRigidBody = connectedPart != null ? connectedPart.GetComponent<Rigidbody2D>() : null;

        var hinge = screw.CreateHinge(connectedRigidBody, hingeConnection);
        if (hinge == null)
            Debug.LogWarning("[LevelManager] CreateHinge returned null for screw " + screw.name);

        // register hinge even if connectedPart is null (ScrewManager may handle nulls internally)
        ScrewManager.AddHingeConnection(hinge, connectedPart);

        // Determine layer index and register screw in layer dict if possible
        if (lm != null && connectedPart != null)
        {
            int partLayer = lm.GetPartByKey(screwData.hinge.bodyPartUniqueID).PartLayer() - 10;

            // Set render sorting via ScrewRender component instead of assigning to a getter/method
            var sr = screw.GetComponent<ScrewRender>();
            if (sr != null)
            {
                sr.SetSortingOrderAndLayer(partLayer, "Screw");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] ScrewRender missing on {screw.name}");
            }

            if (!lm.screwDict.ContainsKey(partLayer))
                lm.screwDict[partLayer] = new List<ScrewController>();

            lm.screwDict[partLayer].Add(screw);
            Debug.Log($"[LevelManager] Appending screw '{screw.name}' to layer {partLayer}");
        }
        else
        {
            Debug.Log($"[LevelManager] Screw '{screw.name}' has no connected part; skipping layer dict insert");
        }

        // Always add screw to manager so it's tracked even if not attached to a part
        ScrewManager.AddScrew(screw);

        // Initialize screw (may include animations/setup)
        yield return screw.Init();
        yield return null;
    }

    private IEnumerator ActivateAllParts()
    {
        var parts = currentLevelObject.GetComponentsInChildren<BasePart>();
        foreach (var part in parts)
        {
            part.Body.bodyType = RigidbodyType2D.Dynamic;
            part.Body.gravityScale = 0f;
            var partLayer = part.PartLayer();
            part.SetIgnoreColliderLayer(true, partLayer, partLayer);
            yield return null;
        }
        yield return new WaitForEndOfFrame();
    }

    private void HandleScrewRemoved(ScrewController removedScrew)
    {

    }
    public void OnReset()
    {
        IngameController.ins.RestartLevel(currentLevelID);
        if (transform.childCount > 0)
        {
            LayerManager layerManager = transform.GetChild(0).GetComponent<LayerManager>();
            layerManager.Reset();
            LevelObjectPool.Instance.pool.ReturnToPool(currentLevelObject);
        }
        boxManager.ResetQueue();
        ArrayScrew.Instance.OnReset();
        SpecialBoxManager.ins.OnReset();
        screwManager.Reset();

        BoxPool.Instance.ReturnAll();
        currentLevelObject = null;
    }
    public bool TryHexToColor(string hex, out Color color)
    {
        color = default;

        if (hex.Length == 8)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out color))
            {
                return true;
            }
        }
        return false;
    }

    public bool ConvertColorToRainbow(ColorEnum color, int requiredCount = 3)
    {
        var sm = screwManager;

        var screws = sm.Screws.Where(s => s.GetColor() == color)
                        .ToList();

        if (screws.Count < requiredCount)
            return false;

        // Lấy đúng số screw cần convert
        var group = screws.Take(requiredCount).ToList();


        // Xóa các screw cũ
        foreach (var s in group)
        {
            s.ChangeScrewColor(ColorEnum.Rainbow);

        }
        return true;
    }

    internal void RemovePart(BasePart bp)
    {
        List<ScrewController> screwList = new List<ScrewController>();
        layerManager.RemovePart(bp.uniqueID);
    }

    public void RemovePartItem(BasePart bp)
    {
        List<ScrewController> listScrews = layerManager.GetScrewByPart(bp);
        boxManager.ProcessScrews(listScrews);
        layerManager.RemoveScrewsOnDict(listScrews);
        bp.gameObject.SetActive(false);
        bp.OnStateChanged.Invoke(true, bp);
    }

    public void LoadLevel(int levelId)
    {
        throw new NotImplementedException();
    }

    public void ResetLevel()
    {
        throw new NotImplementedException();
    }
}