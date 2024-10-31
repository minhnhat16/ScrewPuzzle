using System;
using System.Collections;
using System.Collections.Generic;
using ConfigFile;
using Enum;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Managers;
using PoolManager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private ScrewManager ScrewManager;

    public bool IsInitDone
    {
        get => isInitDone;
        set => isInitDone = value;
    }

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
            Debug.LogError($"No BoxConfig assets found at path: {resourcePath}");
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


        LoadSceneManager.instance.LoadSceneByName("InGame", () =>
        {
            Debug.LogWarning("Load scence done  ");

            IngameController.Instance.Init(() =>
                {
                    StartCoroutine(LoadGameObjectFromLevel(levelID, () =>
                    {
                        ViewManager.Instance.SwitchView(ViewIndex.GamePlayView);
                    })) ;
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

    public IEnumerator LoadGameObjectFromLevel(int levelId,Action callback = null)
    {
        currentLevelID = levelId;
        //spawn level obj
        var levelObject = LevelObjectPool.Instance.pool.SpawnNonGravity();
        currentLevelObject = levelObject;
        levelObject.transform.SetParent(transform);
        levelObject.transform.localPosition = Vector3.zero;
        // Clear existing GameObjects in levelObject to prepare for new loading
        var layerManager = levelObject.GetComponent<LayerManager>();
        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
            layerManager.ClearPartDict();
        }

        // Wait until the end of the frame to ensure all objects are destroyed
        yield return new WaitForEndOfFrame();
        // Find the level data with the given ID
        Level.Level levelData = Levels.GetValueOrDefault(levelId.ToString());
        this.currentLevel = levelData;
        if (levelData == null)
        {
            Debug.LogWarning($"Level with ID {levelId} not found!");
            yield break;
        }

        int layerID = 1;

        BoxQueue.Instance.LoadBoxConfigRecord(levelData.boxConfig);
        BoxQueue.Instance.Init();

        List<BaseLayer> listBaseLayer = new();
        // Loop through all layers in the level data
        foreach (var layerData in levelData.layers)
        {
            // Create a new GameObject for the layer
            BaseLayer layerComponent = LayerPool.Instance.pool.SpawnNonGravity();
            var layerName = $"Layer {layerID++}";
            layerComponent.name = layerName;
            layerComponent.transform.SetParent(levelObject.transform);
            layerComponent.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var layerObj = layerComponent.gameObject;
            var layername = layerObj.layer = LayerMask.NameToLayer(layerName);
            // Loop through all parts in the current layer
            foreach (var partData in layerData.parts)
            {
                var partComponent = PartPool.Instance.pool.SpawnNonGravity();
                // Instantiate a new GameObject for the part
                GameObject partGameObject = partComponent.gameObject;
                partComponent = partGameObject.GetComponent<BasePart>();

                if (partGameObject != null && partComponent != null)
                {
                    partGameObject.transform.SetParent(layerComponent.transform);
                    partGameObject.transform.SetLocalPositionAndRotation(partData.partPosition, Quaternion.identity);
                    partComponent.Body.bodyType = RigidbodyType2D.Static;
                    partComponent.uniqueID = partData.partName;
                    layerManager.AddPart(partComponent);
                    var partLayer = LayerMask.LayerToName(layername);
                    partGameObject.layer = layername;
                    Debug.Log("sprite name " + partData.spriteName + "layer name " + layername);
                    var sprite = SpriteLibControl.Instance.GetSpriteByName(partData.spriteName);
                    /*if (sprite == null) Debug.LogWarning($" Sprite {partData.spriteName} null");*/
                    partComponent.Renderer.sprite = sprite;
                    partComponent.GenerateColliderFromSprite();
                    partComponent.SetSortingLayer(partLayer);
                }

                // Add a delay to visually see the progress or avoid blocking
                yield return null; // Wait for one frame before the next iteration
            }

            listBaseLayer.Add(layerComponent);
        }

        layerManager.Layers = listBaseLayer;
        layerManager.CoverDictToList();

        // Instantiate the screw manager
        var screwManagerGameObject = Instantiate(screwManagerPrefb, levelObject.transform) as GameObject;
        screwManagerGameObject.transform.SetPositionAndRotation(new Vector3(0, -5, 0), Quaternion.identity);
        ScrewManager = screwManagerGameObject.GetComponent<ScrewManager>();
        // Load screws
        foreach (var screwData in levelData.screws)
        {
            var screw = ScrewPool.Instance.Pool.SpawnNonGravity();
            GameObject screwGameObject = screw.gameObject;
            screwGameObject.transform.SetParent(screwManagerGameObject.transform);
            screwGameObject.transform.SetLocalPositionAndRotation(screwData.screwPosition, Quaternion.identity);
            var color = screw.Color = (ColorEnum)screwData.idColor; // Assuming ScrewColor is your enum
            screw.ChangeScrewColorByEnum(color);
            // Handle hinge connections
            foreach (var hingeConnection in screwData.hingeConnections)
            {
                Debug.Log($"Level data with ID {levelId} loaded.");
                var connectedPart = layerManager.GetPartByKey(hingeConnection.bodyPartUniqueID);
                Debug.Log($"Connected part id {connectedPart.uniqueID}");
                screw.CreateHinge(connectedPart.GetComponent<Rigidbody2D>());
            }

            ScrewManager.AddScrew(screw);
            StartCoroutine(screw.Init());

            // Add a delay after each screw is loaded
            yield return null; // Wait for one frame before loading the next screw
        }

        var allParts = layerManager.Parts;
        foreach (var part in allParts)
        {
            part.Body.bodyType = RigidbodyType2D.Dynamic;
            yield return null;
        }
        yield return null;
        callback?.Invoke();
        Debug.Log($"Level data with ID {levelId} loaded.");
    }

    public void Reset()
    {
        LayerManager layerManager = transform.GetChild(0).GetComponent<LayerManager>();
        BoxQueue.Instance.ClearConfigRecords();
        BoxQueue.Instance.ClearCurrentBoxes();
        layerManager.Reset();
        ScrewManager.Reset();
        LevelObjectPool.Instance.pool.ReturnToPool(currentLevelObject);
        currentLevelObject = null;
    }
}