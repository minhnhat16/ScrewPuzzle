#if UNITY_EDITOR
using ConfigFile;
using EditorTools;
using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;
using BoxConfig = ConfigFile.BoxConfig;
using ColorUtility = UnityEngine.ColorUtility;

public class GameObjectToLevelConverter : SingletonMono<GameObjectToLevelConverter>
{
    public int currentLoadedLevel = 0;
    public int nextLevelId = 0; // Static variable to track the next level ID
    public Level.Level levelData; // Reference to the Level ScriptableObject
    public GameObject levelObject; // The parent GameObject that holds all the parts, layers, screws
    public List<Level.Level> allLevels; // List of all available levels
    public GameObject layerBase;
    public GameObject basePart;
    public GameObject screwLevelPrefab;


    public LayerManager lmanager;
    public override void Awake()
    {
        base.Awake();
        lmanager = levelObject.GetComponent<LayerManager>();
    }
    private void Start()
    {
        // Load all Level assets from the Resources/Levels folder
        LoadLevel();
    }

    public void LoadLevel()
    {
        allLevels = new List<Level.Level>(Resources.LoadAll<Level.Level>("Levels"));
        allLevels = allLevels.OrderBy(level => level.levelId).ToList();

        if (allLevels is { Count: > 0 })
        {
            // Initialize nextLevelId
            nextLevelId = 2;

            // Check for gaps in the consecutive sequence of level IDs
            for (int i = 1; i < allLevels.Count; i++)
            {
                if (allLevels[i].levelId != allLevels[i - 1].levelId + 1)
                {
                    // If we find a gap in the sequence, return the missing levelId
                    nextLevelId = allLevels[i].levelId - 1;
                    return;
                }

                nextLevelId = allLevels[i].levelId + 1; // Move to the next consecutive levelId
            }

            return;
        }
    }

    public void ReloadLevels()
    {
        LoadLevel(); // Reload the levels from the Resources folder or wherever they're stored
        Debug.Log("Levels reloaded.");
    }

    public void AddNewLevel(Level.Level newLevel)
    {
        // Assign a unique ID to the new level
        newLevel.levelId = nextLevelId;
        nextLevelId++; // Increment for the next new level

        // Add the new level to the list
        allLevels.Add(newLevel);
        // Save the levels back to a persistent storage
    }

    public void SaveGameObjectToLevel()
    {
        // Create a new Level ScriptableObject instance+
        Level.Level newLevelData = ScriptableObject.CreateInstance<Level.Level>();
        CreateLevelData(newLevelData);

        string directoryPath = "Assets/Resources/Levels"; // Set the directory path
        if (!Directory.Exists(directoryPath)) // Check if the directory exists
        {
            Directory.CreateDirectory(directoryPath); // Create the directory if it doesn't exist
        }

        string assetPath =
            Path.Combine(directoryPath,
                $"Level_{newLevelData.levelId}.asset"); // Set the path where you want to save the asset
        var currentLoadedLevel = AssetDatabase.LoadAssetAtPath<Level.Level>(assetPath);
        // Check if the asset already exists
        if (currentLoadedLevel != null)
        {
            // If it exists, you can choose to either overwrite it or handle it differently
            Debug.LogWarning($"Asset already exists at {assetPath}. Overwriting the existing asset.");
            AssetDatabase.DeleteAsset(assetPath); // Remove the existing asset if you want to overwrite it
        }

        allLevels.Add(currentLoadedLevel);
        allLevels[this.currentLoadedLevel] = newLevelData;
        newLevelData.boxConfig  = CreateBoxConfig(newLevelData.levelId,newLevelData);
        AssetDatabase.CreateAsset(newLevelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // Refresh the AssetDatabase to see the changes
        LoadLevel();
        Debug.Log("Level data saved as ScriptableObject: " + assetPath);
    }
    public void ResetScrew()
    {

    }
    public Level.Level CreateLevelData(Level.Level newLevelData)
    {

        newLevelData.levelId = nextLevelId++;

        newLevelData.layers = new List<LayerData>();
        var layerManager= levelObject.GetComponent<LayerManager>();
        layerManager.ActiveAllLayers();
        var layers = levelObject.GetComponentsInChildren<BaseLayer>(true);

        int idLayer = 0;
        // Loop through all layers
        foreach (var layerTransform in layers)
        {
            LayerData layerData = new LayerData
            {
                layerId = idLayer++,
                parts = new List<BodyPartScriptable>()
            };

            var parts = layerTransform.GetComponentsInChildren<BasePart>(true);
            int partIndex = 0;
            string baseLayerName = LayerMask.LayerToName(layerTransform.gameObject.layer);
            // Loop through all parts in the current layer
            foreach (BasePart partTransform in parts)
            {
                if (partTransform == null) continue; // Ensure partTransform is not null
                var parComponent = partTransform.GetComponent<BasePart>();
                BodyPartScriptable partData = new BodyPartScriptable
                {
                    
                    idBodyPart = partIndex++,
                    partName = parComponent.uniqueID,
                    partPosition = partTransform.transform.localPosition,
                    partRotation = partTransform.transform.localRotation,
                    partLocalScale = partTransform.transform.localScale,
                    spriteName = parComponent.Renderer.sprite.name,
                    layer = baseLayerName,
                    colorString = ColorUtility.ToHtmlStringRGBA(partTransform.Renderer.color),
                };
                layerData.parts.Add(partData);
            }

            newLevelData.layers.Add(layerData);
        }

        // Collect screw data
        var screws = levelObject.GetComponentsInChildren<ScrewController>();
        newLevelData.screws = new List<ScrewScriptable>();
        int idScrew = 0;
      
        foreach (var screw in screws)
        {
            //ScrewScriptable screwData = new ScrewScriptable()
            //{
            //    screwPosition = screw.transform.localPosition,
            //    idScrew = idScrew,
            //    idColor = (int)screw.Color,
            //    hinge =new HingeConnection()
            //};

            //var hinge = screw.HingeController.HingeJoint2D;
            //var listHingeObject = new List<HingeConnection>();
            //if (hinge == null) continue;
            //int idBody = 0;
          
            //var pos = hinge.transform.localPosition;
            //var body = $"{screw.HingeController.BodyConnect?.GetComponent<BasePart>().uniqueID}";
            //var hingPos = hinge.connectedBody.transform.localPosition;
            //HingeConnection hingeConnection = new HingeConnection()
            //{
            //    hingePosition = pos,
            //    bodyPartUniqueID = body,
            //    bodyPartHingePosition = hingPos,
            //};
            //idBody++;
            //listHingeObject.Add(hingeConnection);
            //idScrew++;

            //screwData.hinge = hingeConnection;
            //newLevelData.screws.Add(screwData);
        }
        // Save the new level data as a ScriptableObject in the Resources folder
        return newLevelData;
    }
    public void ResetLevelData(Level.Level levelData)
    {
        if (levelData == null)
        {
            Debug.LogWarning("The level data to reset is null.");
            return;
        }

        // Reset the level ID
        levelData.levelId = 0;

        // Clear all layers
        if (levelData.layers != null)
        {
            foreach (var layer in levelData.layers)
            {
                // Clear parts from each layer
                if (layer.parts != null)
                {
                    layer.parts.Clear();
                }
            }
            levelData.layers.Clear();
        }

        // Clear screws
        if (levelData.screws != null)
        {
            foreach (var screw in levelData.screws)
            {
                // Clear hinge connections for each screw
                if (screw.hinge != null)
                {
                    screw.hinge = null;
                }
            }
            levelData.screws.Clear();
        }

        // Optionally reset other properties or references if needed
        Debug.Log("Level data has been reset.");
    }

    public Dictionary<int, int> GetScrewCountByColor(Level.Level level)
    {
        Dictionary<int, int> screwColorDict = new Dictionary<int, int>();

        // Iterate through each screw and count by color
        foreach (var screw in level.screws)
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

    public List<BoxConfigRecord> CalculateScrewsDivisibleBy3AndSpawnBoxes(Level.Level level)
    {
        var screwColorDict = GetScrewCountByColor(level);
        List<BoxConfigRecord> boxRecords = new();
        foreach (var kvp in screwColorDict)
        {
            int colorId = kvp.Key;
            int totalScrews = kvp.Value;

            int screwsDivisibleBy3 = totalScrews / 3; // Full boxes
            int remainderScrews = totalScrews % 3; // Leftover screws

            Debug.Log(
                $"Color ID: {colorId}, Total Screws: {totalScrews}, Divisible by 3: {screwsDivisibleBy3}, Remainder: {remainderScrews}");

            // Spawn full boxes with 3 screws each
            for (int i = 0; i < screwsDivisibleBy3; i++)
            {
                BoxConfigRecord newRecord = new BoxConfigRecord();
                newRecord.NumberOfScrewHoles = 3;
                newRecord.BoxColor = (ColorEnum)colorId;
                boxRecords.Add(newRecord);
            }

            // Spawn a remainder box with 1 screw if there's a remainder
            if (remainderScrews == 2)
            {
                BoxConfigRecord newRecord = new BoxConfigRecord();
                newRecord.NumberOfScrewHoles =remainderScrews ;
                newRecord.BoxColor = (ColorEnum)colorId;
                boxRecords.Add(newRecord);
            }
            else if (remainderScrews == 1)
            {
                BoxConfigRecord newRecord = new BoxConfigRecord();
                newRecord.NumberOfScrewHoles = remainderScrews;
                newRecord.BoxColor = (ColorEnum)colorId;
                boxRecords.Add(newRecord);
            }
        }

        return boxRecords;
    }

    public BoxConfig CreateBoxConfig(int idLevel, Level.Level level)
    {
        // Create a new instance of the BoxConfig asset
        BoxConfig newConfig = ScriptableObject.CreateInstance<BoxConfig>();

        // Call the method to get records based on screw distribution
        var records = CalculateScrewsDivisibleBy3AndSpawnBoxes(level);

        // Define the path and generate the name with the counter (idLevel)
        string path = GameConstants.BOX_CONFIGS+ idLevel + ".asset";

        // Check if a BoxConfig asset already exists at the path
        BoxConfig existingConfig = AssetDatabase.LoadAssetAtPath<BoxConfig>(path);
        if (existingConfig != null)
        {
            // Delete the existing asset if it exists
            AssetDatabase.DeleteAsset(path);
            Debug.Log("Deleted existing BoxConfig at path: " + path);
        }

        // Add the records to the new config
        foreach (var record in records)
        {
            newConfig.records.Add(record);
        }

        // Create the asset at the specified path
        AssetDatabase.CreateAsset(newConfig, path);

        // Save the asset and refresh the AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Focus the Project window and highlight the new asset
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newConfig;

        return newConfig;
        //Debug.Log("Created new BoxConfig for level " + idLevel + " at path: " + path);
    }

    public void LoadLevel(int levelID)
    {
        StartCoroutine(LoadGameObjectFromLevel(levelID));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public IEnumerator LoadGameObjectFromLevel(int levelId)
    {
        var layerManager = levelObject.GetComponent<LayerManager>();
        currentLoadedLevel = levelId;
        var baseLevel = levelObject.GetComponent<BaseLevelObject>();
        // Clear existing GameObjects in levelObject to prepare for new loading

        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
            layerManager.ClearPartDict();
        }

        // Wait until the end of the frame to ensure all objects are destroyed
        yield return new WaitForEndOfFrame();
        // Find the level data with the given ID
        Level.Level levelData = allLevels.Find(level => level.levelId == levelId);
        if (levelData == null)
        {
            Debug.LogWarning($"Level with ID {levelId} not found!");
            yield break;
        }
        layerManager.screwDict = new Dictionary<int, List<ScrewController>>();
        //BoxQueue.ins.boxConfig = levelData.boxConfig;

        List<BaseLayer> listBaseLayer = new();
        // Loop through all layers in the level data
        foreach (var layerData in levelData.layers)
        {
            // Create a new GameObject for the layer

            layerManager.screwDict.Add(layerData.layerId, new List<ScrewController>());
            GameObject layerGameObject = Instantiate(layerBase.gameObject, Vector3.zero , Quaternion.identity);
            var layerName = $"Layer {layerData.layerId+1}";
            layerGameObject.name = layerName;
            layerGameObject.transform.SetParent(levelObject.transform);
            layerGameObject.layer = LayerMask.NameToLayer(layerName);
            var layerComponent = layerGameObject.GetComponent<BaseLayer>();
            // Loop through all parts in the current layer
            foreach (var partData in layerData.parts)
            {
                // Instantiate a new GameObject for the part
                GameObject partGameObject =
                    Instantiate(Resources.Load("Prefabs/PartLevelMaker"), layerGameObject.transform) as GameObject;

                if (partGameObject != null)
                {
                    partGameObject.transform.SetParent(layerGameObject.transform);
                    partGameObject.transform.SetPositionAndRotation(partData.partPosition , partData.partRotation);
                    partGameObject.transform.localScale = partData.partLocalScale;
                    var sprite = SpriteLibControl.Instance.GetSprite(layerData.layerId,SpriteGroup.Main, partData.spriteName);
                    var partComponent = partGameObject.GetComponent<BasePart>();
                    partComponent.uniqueID = partData.partName;
                    partGameObject.name = partData.partName;
                    partComponent.Renderer.sprite = sprite;
                    if (TryHexToColor(partData.colorString, out Color color))
                    {
                        Debug.Log($"Parsed Color: {color}");
                        partComponent.Renderer.color = color;
                    }
                    else
                    {
                        Debug.LogError("Invalid Hex Color String");
                    }
                    layerManager.AddPart(partComponent);
                    partComponent.ResetAndReapplyPolygonCollider();
                    var partLayer = LayerMask.LayerToName(layerGameObject.layer);
                    partComponent.SetSortingLayer(partLayer);

                    partComponent.gameObject.layer = layerGameObject.layer;
                }

                // Add a delay to visually see the progress or avoid blocking
                yield return null; // Wait for one frame before the next iteration
            }

            listBaseLayer.Add(layerComponent);
        }

        layerManager.Layers = listBaseLayer;
        layerManager.CoverDictToList();

        // Instantiate the screw manager
        var screwManager = Instantiate(Resources.Load("Prefabs/ScrewManager"), levelObject.transform) as GameObject;
        baseLevel.ScrewManager = screwManager.GetComponent<ScrewManager>();
        screwManager.transform.position = Vector3.zero;
        var screwTransform = screwManager.transform;


        var screwPrefab = Resources.Load("GameObject/ScrewLevelMaker");
        // Load screws
        foreach (var screwData in levelData.screws)
        {
            GameObject screwGameObject =
                Instantiate(screwPrefab, screwTransform) as GameObject;
            screwGameObject.transform.localPosition = screwData.screwPosition;


            Debug.Log("Screw is null " + screwData.idColor==null);
            ScrewLevelMaker screwComponent = screwGameObject.GetComponent<ScrewLevelMaker>();
            var color = (ColorEnum)screwData.idColor; // Assuming ScrewColor is your enum
            screwComponent.ChangeScrewColor(color);
            // Handle hinge connections

            var hingeConnection = screwData.hinge;
            // Debug.Log($"Level data with ID {levelId} loaded.");
            var connectedPart = layerManager.GetPartByKey(hingeConnection.bodyPartUniqueID);
            screwComponent.CreateHinge(connectedPart.GetComponent<Rigidbody2D>(), hingeConnection);

            Debug.Log(
                $"Hinge connected to part {connectedPart.PartLayer()} at position {connectedPart.uniqueID}");
         
            var connection = screwData.hinge;
            var part = layerManager.GetPartByKey(connection.bodyPartUniqueID);
            var partLayerID = part.PartLayer() - 10;

            Debug.Log($"Part Layer ID: {partLayerID} + {part.PartLayer()}");
            if (!layerManager.screwDict.ContainsKey(partLayerID))
            {
                layerManager.screwDict[partLayerID] = new List<ScrewController>();
            }

            if (!layerManager.screwDict[partLayerID].Contains(screwComponent))
            {
                layerManager.screwDict[partLayerID].Add(screwComponent);
            }
            yield return null; 
            StartCoroutine(screwComponent.Init());
            screwManager.GetComponent<ScrewManager>().AddScrew(screwComponent);
        }
        
        this.levelData = levelData;
        UpdateScrewTotal();
        Debug.Log($"Level data with ID {levelId} loaded.");
    }

    public void ClearLevel()
    {
        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void ResetScrewHinge()
    {
        var screwManager = levelObject.GetComponentInChildren<ScrewManager>();
        screwManager.Reset();
    }
    public void SpawnScrew()
    {

        //Debug.Log("Spawn Screw");
        //if (LevelMaker.instance.isInputData) return;
        //var screwManager = levelObject.GetComponentInChildren<ScrewManager>();
        //var screw = Instantiate(screwLevelPrefab,
        //    screwManager.transform);
        //var screwComp = screw.GetComponent<ScrewLevelMaker>();
        //screwComp.Color = (ColorEnum)LevelMaker.instance.currentScrewColorID;
        //screwComp.ChangeScrewColor(screwComp.Color);
        //StartCoroutine(screwComp.InitOnLevelMaker());
        //int layerID = LevelMaker.instance.layerDropdown.Value();
        //layerID -= 1;
        //UpdateScrewTotal();
        //Debug.Log("Screw Layer ID: " + layerID);

        //var list = lmanager.screwDict.GetValueOrDefault(layerID);
        //if(list == null)
        //{
        //    lmanager.screwDict[layerID] = new List<Screw>();
        //}
        //lmanager.screwDict[layerID].Add(screwComp);
        ////Debug.Assert(screw != null, nameof(screw) + " != null");
        //bool isMouseOnScreen = IsMouseOnScreen();
        //if (isMouseOnScreen)
        //{
        //    var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    screwComp.Position = new Vector3(mousePos.x, mousePos.y, screwComp.Position.z);
        //    return;
        //}

        //screw.transform.position = new Vector3(-5, 0, 0);
        //screwManager.AppendScrew(screwComp);

    }

    public void SpawnPart()
    {
        if (LevelMaker.instance.isInputData) return;
        Debug.Log("Spawn part");
        var layerToSpawn = GetLayerToSpawn();
        var parent = layerToSpawn == null ? levelObject.transform : layerToSpawn.transform;


        var part = Instantiate(basePart.gameObject, parent);
        if (part == null) return;
        //Debug.Assert(part != null, nameof(part) + " != null"  );

        part.layer = layerToSpawn.gameObject.layer;
        Debug.Log("PartLayer :" + part.gameObject.layer);
        bool isMouseOnScreen = IsMouseOnScreen();
        if (isMouseOnScreen)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            part.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
            return;
        }

        part.transform.position = new Vector3(0, 0, 0);
    }

    private BaseLayer GetLayerToSpawn()
    {
        var layerID = LevelMaker.instance.layerDropdown.Value();
        var layerManagerComponent = levelObject.GetComponent<LayerManager>();
        var listLayer = layerManagerComponent.Layers;
        if (layerID > listLayer.Count || layerID < 0)
        {
            Debug.LogWarning($"Layer {layerID} not valid, try to getlayer <= {listLayer.Count}");
            var newLayer = SpawnNewLayer(listLayer.Count);
            var newLayerComponent = newLayer.GetComponent<BaseLayer>();
            listLayer.Add(newLayerComponent);
            return newLayerComponent;
        }

        return listLayer[layerID - 1];
    }

    private GameObject SpawnNewLayer(int nextID)
    {
        GameObject layerGameObject = Instantiate(layerBase.gameObject, Vector3.zero, Quaternion.identity);
        layerGameObject.name = $"Layer {++nextID}";
        layerGameObject.transform.SetParent(levelObject.transform);
        layerGameObject.layer = LayerMask.NameToLayer($"{layerGameObject.name}");
        return layerGameObject;
    }

    public void ResetAllScrewsFlag()
    {
        var levelObj = levelObject.GetComponent<BaseLevelObject>();
        var screwMnger = levelObj.ScrewManager;
        var screws = screwMnger.Screws;

        foreach(var s in screws)
        {
            var sLv = (ScrewLevelMaker)s;
        }
    }
    private bool IsMouseOnScreen()
    {
        Vector3 mousePosition = Input.mousePosition;

        // Check if the mouse is within the screen bounds
        bool isWithinXBounds = mousePosition.x >= 0 && mousePosition.x <= Screen.width;
        bool isWithinYBounds = mousePosition.y >= 0 && mousePosition.y <= Screen.height;

        return isWithinXBounds && isWithinYBounds;
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

    public void RemoveAllScrew()
    {
        var screwManager = levelObject.GetComponentInChildren<ScrewManager>();
        var screws = screwManager.Screws;
        foreach (var s in screws) {
            var sm = s as ScrewLevelMaker;
            RemoveScrew(sm);
            Destroy(sm);
        }
    
    }

    public int GetScrewTotal(ColorEnum color)
    {
        var screwManager = levelObject.GetComponentInChildren<ScrewManager>();
        return screwManager.GetScrewTotalByColor(color);
    }

    public void UpdateScrewTotal()
    {
        var dropDown = LevelMaker.instance.colorDropDown;
        dropDown.UpdateAllScrewTotal();
    }
    internal void RemoveScrew(ScrewLevelMaker screwLevelMaker)
    {
        screwLevelMaker.ResetHinge();
        lmanager.RemoveScrew(screwLevelMaker);
        var screwManager = levelObject.GetComponentInChildren<ScrewManager>();
        screwManager.RemoveScrew(screwLevelMaker);
        Destroy(screwLevelMaker.gameObject);
    }
}
#endif