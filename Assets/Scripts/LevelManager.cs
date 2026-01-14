using ConfigFile;
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
using System.Linq;
using UnityEngine;

public class LevelManager : SingletonMono<LevelManager>, IResetable
{
    public LayerManager layerManager;
    [SerializeField] private bool isInitDone;
    public int currentLevelID;

    public Dictionary<string, Level.Level> Levels = new Dictionary<string, Level.Level>();
    private List<BoxConfig> boxconfigsLevel = new List<BoxConfig>();
    public List<Level.Level> levelConfig = new List<Level.Level>();
    public Level.Level currentLevel;

    [SerializeField] private GameObject screwManagerPrefb;
    [SerializeField] private ScrewManager screwManager;

    [SerializeField] private BaseLevelObject currentLevelObject;

    private PartSpriteService spriteService;
    public bool IsInitDone
    {
        get => isInitDone;
        set => isInitDone = value;
    }
    public ScrewManager ScrewManager { get => screwManager; set => screwManager = value; }

    public override void Awake()
    {
        base.Awake();
        this.screwManager = GetComponent<ScrewManager>();
        spriteService = new PartSpriteService();
    }

    public void Start()
    {
    }

    public void Init()
    {
        isInitDone = false;
        StartCoroutine(LoadConfigFromFile());
        StartCoroutine(LoadLevelOnFile(() => isInitDone = true));

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
        Debug.LogWarning("Start loading level ");
        var boxManager = BoxQueue.ins;
        var arrayScrew = ArrayScrew.Instance;
        IngameController.ins.IsGameOver = false;
        boxManager.activeBoxCount = 2;
        arrayScrew.ShowArrayScrew();
        OnReset();
        arrayScrew.HoldAlignment();


        List<Func<IEnumerator>> preTasks = new List<Func<IEnumerator>>()
        {
            () => IngameController.ins.LoadIngameAssetCoroutine(),
            () => TaskLoadObjectFromLevel(levelID),
        };
        TaskManager.ins.AddTask(preTasks);
        LoadSceneManager.ins.LoadSceneByName("InGame", null);
    }
    public IEnumerator TaskLoadObjectFromLevel(int levelID)
    {
        yield return StartCoroutine(LoadGameObjectFromLevel(levelID, () =>
          {
              long userGold = GameManager.instance.GetPlayerGold();
              GamePlayViewParam param = new();
              param.totalGold = userGold;
              ViewManager.Instance.SwitchView(ViewIndex.GameView, param);
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
        //Debug.Log("Step 2 complete");

        Debug.Log("Step 3: Initializing Box Queue");
        // Step 3: Load Box Configuration
        InitializeBoxQueue(levelData);
        //Debug.Log("Step 3 complete");

        Debug.Log("Step 4: Loading Layers");
        // Step 4: Load Layers
        yield return LoadLayers(levelData);
        //Debug.Log("Step 4 complete");

        Debug.Log("Step 5: Loading Screw Manager and Screws");
        // Step 5: Load Screw Manager and Screws
        yield return LoadScrewManagerAndScrews(levelData);
        //Debug.Log("Step 5 complete");

        Debug.Log("Step 6: Activating All Parts");
        // Step 6: Activate Parts
        yield return ActivateAllParts();
        //Debug.Log("Step 6 complete");

        InitSpecial();
        BoxQueue.ins.InitBoxToSlot();
        Debug.Log("Level loading complete.");
        callback?.Invoke();
        //Debug.Log($"Level data with ID {levelId} loaded.");
    }
    public void InitSpecial()
    {
        int requireSpecial = IngameController.ins.requireCount;
        layerManager.visibilityController.ApplyLayerVisibility();
        SideMission mission = SideMissionManager.ins.GenerateColorMission(currentLevel, BoxQueue.ins, requireSpecial);
        IngameController.ins.SetSideMission(mission); // tạo hàm này

        BoxQueue.ins.hasSpecialBox = mission != null;


        Debug.Log("Has special box: " + BoxQueue.ins.hasSpecialBox);
        if (mission != null)
        {
            ConvertColorToRainbow((ColorEnum)mission.targetColorID);
            BoxQueue.ins.InitRainbowBoxes(mission.requiredCount / 3, (ColorEnum)mission.targetColorID);
        }
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

    private Level.Level GetLevelData(int levelId)
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
        BoxQueue.ins.LoadBoxConfigRecord(levelData.boxConfig);
        BoxQueue.ins.Init();
    }
    private IEnumerator LoadLayers(Level.Level levelData)
    {
        int layerID = 1;
        List<BaseLayer> listBaseLayer = new();

        var layerScrewsDict = new Dictionary<int, List<Screw>>();
        var queue = new Queue<BaseLayer>();
        foreach (var layerData in levelData.layers)
        {
            var layerComponent = CreateLayer(layerID++);
            foreach (var partData in layerData.parts)
            {
                yield return LoadPart(layerComponent, partData, layerManager);
            }
            layerComponent.RegisterPartListener();
            listBaseLayer.Add(layerComponent);

            queue.Enqueue(layerComponent);
            Debug.Log($"Layer {layerData.layerId} loaded with {layerData.parts.Count} parts.");
            layerScrewsDict.Add(layerData.layerId, new List<Screw>());
        }
        layerManager.screwDict = layerScrewsDict;



        Debug.Log($"All layers loaded. Total layers: {layerScrewsDict.Count}. Keys: {string.Join(", ", layerScrewsDict.Keys.Select(k => k.ToString()))}");



        layerManager.Layers = listBaseLayer;
        layerManager.CoverDictToList();
        layerManager.visibilityController.RePreviewMax = listBaseLayer.Count;
        layerManager.visibilityController.layerQueue = queue;
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
            SetupPartTransform(partComponent, partData,layerComponent.transform);
            partComponent.uniqueID = partData.partName;
            partComponent.name = partData.partName;
            layerManager.AddPart(partComponent);

            string layerName = layerComponent.name; ;
            SetupPartSprite(partComponent, partData, layerName);
            if (TryHexToColor(partData.colorString, out Color color))
            {
                partComponent.Renderer.color = color;
            }
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
        var sprite = spriteService.GetPartSprite(currentLevelID, data.spriteName, false);

        Debug.Log($"  Loading part '{data.partName}' with sprite '{data.spriteName}', sprite {sprite == null}");
        var outline = spriteService.GetPartSprite(currentLevelID, data.spriteName, true);

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
    }

    private IEnumerator LoadScrew(ScrewScriptable screwData)
    {
        var screw = ScrewPool.Instance.Pool.SpawnNonGravity();
        var screwGameObject = screw.gameObject;
        screw.OnReset();
        screwGameObject.transform.SetParent(ScrewManager.transform);
        screwGameObject.transform.SetLocalPositionAndRotation(screwData.screwPosition, Quaternion.identity);
        screw.Color = (ColorEnum)screwData.idColor;
        screw.ChangeScrewColor(screw.Color);

        var hingeConnection = screwData.hinge;
        if (currentLevelObject == null) yield break;
        LayerManager lm = currentLevelObject.GetComponent<LayerManager>();
        BasePart connectedPart = null;
        if (lm != null)
        {
            connectedPart = lm.GetPartByKey(hingeConnection.bodyPartUniqueID);
        }

        Debug.Log("Connected part " + connectedPart);

        Rigidbody2D connectedRigidBody = null;
        if (connectedPart != null)
        {
            connectedRigidBody = connectedPart.GetComponent<Rigidbody2D>();
        }

        var hinge = screw.CreateHinge(connectedRigidBody, hingeConnection);

        ScrewManager.AddHingeConnection(hinge, connectedPart);

        if (currentLevelObject != null && lm != null && connectedPart != null)
        {
            var partLayer = lm.GetPartByKey(screwData.hinge.bodyPartUniqueID).PartLayer() - 10;
            screw.sortingOrder = partLayer;
            if (lm.screwDict.ContainsKey(partLayer))
            {
                lm.screwDict[partLayer].Add(screw);
                Debug.Log("Appending key at " + partLayer + " screw" + screw.name);
            }

            ScrewManager.AddScrew(screw);
            yield return screw.Init();
            yield return null;
        }
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

    private void HandleScrewRemoved(Screw removedScrew)
    {

    }
    public void OnReset()
    {
        if (transform.childCount > 0)
        {
            LayerManager layerManager = transform.GetChild(0).GetComponent<LayerManager>();
            layerManager.Reset();
            LevelObjectPool.Instance.pool.ReturnToPool(currentLevelObject);
        }
        BoxQueue.ins.ClearConfigRecords();
        BoxQueue.ins.ClearCurrentBoxes();
        BoxQueue.ins.OnReset();
        ArrayScrew.Instance.OnReset();
        SpecialBoxManager.ins.OnReset();
        screwManager.Reset();

        ThreeHoldBoxPool.Instance.ReturnAll();
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

        var screws = sm.Screws.Where(s => s.Color == color)
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
        List<Screw> screwList = new List<Screw>();
        layerManager.RemovePart(bp.uniqueID);
    }

    public void RemovePartItem(BasePart bp)
    {
        List<Screw> listScrews = layerManager.GetScrewByPart(bp);
        var boxQueue = BoxQueue.ins; ;
        boxQueue.TryMoveScrewsGroupedByColor(listScrews, true);
        layerManager.RemoveScrewsOnDict(listScrews);
        layerManager.RemovePart(bp.uniqueID);
        bp.gameObject.SetActive(false);

    }


}