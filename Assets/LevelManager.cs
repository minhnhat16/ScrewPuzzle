using ConfigFile;
using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using Managers;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private bool isInitDone;

    public Dictionary<string, Level.Level> Levels = new Dictionary<string, Level.Level>();
    public List<BoxConfig> boxconfigsLevel = new List<BoxConfig>();
    public List<Level.Level> levelConfig = new List<Level.Level>();
    public Level.Level currentLevel;

    public int currentLevelID;
    [SerializeField] private BaseLevelObject currentLevelObject;
    [SerializeField] private GameObject screwManagerPrefb;
    [SerializeField] private ScrewManager screwManager;

    public bool IsInitDone
    {
        get => isInitDone;
        set => isInitDone = value;  
    }
    public ScrewManager ScrewManager { get => screwManager; set => screwManager = value; }

    public void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
        }

        Instance = this;
    }

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        StartCoroutine(LoadLevelOnFile());
        StartCoroutine(LoadConfigFromFile(() => isInitDone = true));
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
        else
        {
            //Debug.LogError($"No BoxConfig assets found at path: {resourcePath}");
        }

        // Call the callback if it's not null
        callback?.Invoke();
    }

    public IEnumerator LoadLevelOnFile(Action callback = null)
    {
        yield return new WaitForSeconds(1f);

        // Path should not include "Resources" or file extension
        string resourcePath = "Levels"; // Assuming files are in Resources/ConfigFile/Level

        // Load all assets of type BoxConfig from the specified folder
        var levels = Resources.LoadAll<Level.Level>(resourcePath);

        if (levels.Length > 0)
        {
            foreach (var level in levels)
            {
                string strIDLevel = level.levelId.ToString();
                Levels.TryAdd(strIDLevel, level);
            }

            levelConfig = new List<Level.Level>(Levels.Values);
        }
        else
        {
            Debug.LogError($"No GameObject assets found at path: {resourcePath}");
        }

        // Call the callback if it's not null
        callback?.Invoke();
    }

    public void LoadLevel(int levelID, Action callback = null)
    {
        Debug.LogWarning("Start loading level ");
        var boxManager = BoxQueue.Instance;
        var arrayScrew = ArrayScrew.Instance;
        arrayScrew.ShowArrayScrew() ;
        Reset();
        arrayScrew.HoldAlignment();
        LoadSceneManager.instance.LoadSceneByName("InGame", () =>
        {
            Debug.LogWarning("Load scence done  ");

            IngameController.Instance.Init(() =>
                {
                    StartCoroutine(LoadGameObjectFromLevel(levelID, () =>
                    {
                        int userGold = GameManager.instance.GetPlayerGold();
                        GamePlayViewParam param = new();
                        param.totalGold = userGold;
                        ViewManager.Instance.SwitchView(ViewIndex.GamePlayView,param);
                    }));
                });
        });
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

        // B??c 1: Kh?i t?o Level Object
        yield return InitializeLevelObject();

        // B??c 2: L?y d? li?u Level
        var levelData = GetLevelData(levelId);
        if (levelData == null) yield break;
        currentLevel = levelData;

        // B??c 3: T?i Box Configuration
        InitializeBoxQueue(levelData);

        // B??c 4: T?i các Layers
        yield return LoadLayers(levelData);

        // B??c 5: T?i Screw Manager và các Screw
        yield return LoadScrewManagerAndScrews(levelData);

        // B??c 6: Kích ho?t Parts
        yield return ActivateAllParts();

        callback?.Invoke();
        Debug.Log($"Level data with ID {levelId} loaded.");
    }
    private IEnumerator InitializeLevelObject()
    {
        var levelObject = LevelObjectPool.Instance.pool.SpawnNonGravity();
        currentLevelObject = levelObject;
        levelObject.transform.SetParent(transform);
        levelObject.transform.localPosition = Vector3.zero;

        // Xóa các ??i t??ng c? trong levelObject
        var layerManager = levelObject.GetComponent<LayerManager>();
        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
            layerManager.ClearPartDict();
        }

        // ??i cu?i frame ?? ??m b?o t?t c? ??i t??ng b? xóa
        yield return new WaitForEndOfFrame();
    }
    private Level.Level GetLevelData(int levelId)
    {
        Level.Level levelData = Levels.GetValueOrDefault(levelId.ToString());
        if (levelData == null)
        {
            Debug.LogWarning($"Level with ID {levelId} not found!");
        }
        return levelData;
    }
    private void InitializeBoxQueue(Level.Level levelData)
    {
        BoxQueue.Instance.LoadBoxConfigRecord(levelData.boxConfig);
        BoxQueue.Instance.Init();
    }
    private IEnumerator LoadLayers(Level.Level levelData)
    {
        int layerID = 1;
        List<BaseLayer> listBaseLayer = new();
        var layerManager = currentLevelObject.GetComponent<LayerManager>();

        foreach (var layerData in levelData.layers)
        {
            var layerComponent = CreateLayer(layerID++);
            foreach (var partData in layerData.parts)
            {
                yield return LoadPart(layerComponent, partData, layerManager);
            }

            listBaseLayer.Add(layerComponent);
        }

        layerManager.Layers = listBaseLayer;
        layerManager.CoverDictToList();
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
        partComponent = partGameObject.GetComponent<BasePart>();

        if (partGameObject != null && partComponent != null)
        {
            partGameObject.transform.SetParent(layerComponent.transform);
            partGameObject.transform.SetLocalPositionAndRotation(partData.partPosition, partData.partRotation);
            partGameObject.transform.localScale = partData.partLocalScale;
            partComponent.Body.bodyType = RigidbodyType2D.Static;
            partComponent.uniqueID = partData.partName;
            layerManager.AddPart(partComponent);

            partGameObject.layer = LayerMask.NameToLayer(layerComponent.name);
            var sprite = SpriteLibControl.Instance.GetSpriteByName(partData.spriteName);
            partComponent.Renderer.sprite = sprite;

            if (TryHexToColor(partData.colorString, out Color color))
            {
                partComponent.Renderer.color = color;
            }

            partComponent.GenerateColliderFromSprite();
            partComponent.SetSortingLayer(LayerMask.LayerToName(partGameObject.layer));
        }

        yield return null;
    }
    private IEnumerator LoadScrewManagerAndScrews(Level.Level levelData)
    {
        var screwManagerGameObject = Instantiate(screwManagerPrefb, currentLevelObject.transform) as GameObject;
        screwManagerGameObject.transform.SetPositionAndRotation(new Vector3(0, -5, 0), Quaternion.identity);
        ScrewManager = screwManagerGameObject.GetComponent<ScrewManager>();
        ScrewManager.hingeConnections = new();
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
        var screwComp = screwGameObject.GetComponent<Screw>();
        screwGameObject.transform.SetParent(ScrewManager.transform);
        screwGameObject.transform.SetLocalPositionAndRotation(screwData.screwPosition, Quaternion.identity);

        screw.Color = (ColorEnum)screwData.idColor;
        screw.ChangeScrewColorByEnum(screw.Color);
        screw.ResetRender();

        foreach (var hingeConnection in screwData.hingeConnections)
        {
            var connectedPart = currentLevelObject.GetComponent<LayerManager>().GetPartByKey(hingeConnection.bodyPartUniqueID);
            var hinge = screw.CreateHinge(connectedPart.GetComponent<Rigidbody2D>());
            ScrewManager.AddHingeConnection(hinge, connectedPart);
        }

        ScrewManager.AddScrew(screw);
        yield return screw.Init();
        yield return null;
    }
    private IEnumerator ActivateAllParts()
    {
        var layerManager = currentLevelObject.GetComponent<LayerManager>();
        foreach (var part in layerManager.Parts)
        {
            part.Body.bodyType = RigidbodyType2D.Static;
            part.Body.gravityScale = 0;
           var partLayer =  part.PartLayer();
            part.SetIgnoreColliderLayer(true, partLayer, partLayer);
            yield return null;
        }
    }

    private void HandleScrewRemoved(Screw removedScrew)
    {
        Debug.Log($"Screw {removedScrew.name} has been removed. Handling additional logic...");
    }
    public void Reset()
    {
        if (transform.childCount > 0)
        {
            LayerManager layerManager = transform.GetChild(0).GetComponent<LayerManager>();
            layerManager.Reset();
            LevelObjectPool.Instance.pool.ReturnToPool(currentLevelObject);
        }
        BoxQueue.Instance.ClearConfigRecords();
        BoxQueue.Instance.ClearCurrentBoxes();
        ArrayScrew.Instance.ClearAllScrewsOnArray();
        screwManager.Reset();

        currentLevelObject = null;
    }
    public bool TryHexToColor(string hex, out Color color)
    {
        color = default;

        if (hex.Length == 8)
        { // Check if it's in RRGGBBAA format
            // Parse R, G, B, and A components
            if (ColorUtility.TryParseHtmlString("#" + hex, out color))
            {
                return true;
            }
        }
        Debug.LogError("Hex string must be 8 characters in RRGGBBAA format");
        return false;
    }
}