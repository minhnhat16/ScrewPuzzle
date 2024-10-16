using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enum;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using Unity.VisualScripting;
using UnityEditor;

public class GameObjectToLevelConverter : MonoBehaviour
{
    public int currentLoadedLevel = 0;
    public int nextLevelId = 0; // Static variable to track the next level ID
    public Level.Level levelData; // Reference to the Level ScriptableObject
    public GameObject levelObject; // The parent GameObject that holds all the parts, layers, screws
    public List<Level.Level> allLevels; // List of all available levels
    public GameObject layerBase;
    public GameObject basePart;
    public GameObject screwLevelPrefab;

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

            // If no gaps are found, nextLevelId will be set to one higher than the largest existing levelId
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
        allLevels[newLevelData.levelId] = newLevelData;
        AssetDatabase.CreateAsset(newLevelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // Refresh the AssetDatabase to see the changes
        LoadLevel();
        Debug.Log("Level data saved as ScriptableObject: " + assetPath);
    }

    public Level.Level CreateLevelData(Level.Level newLevelData)
    {
        newLevelData.levelId = nextLevelId++;

        newLevelData.layers = new List<LayerData>();

        var layers = levelObject.GetComponentsInChildren<BaseLayer>();

        int idLayer = 0;
        // Loop through all layers
        foreach (var layerTransform in layers)
        {
            LayerData layerData = new LayerData
            {
                layerId = idLayer++,
                parts = new List<BodyPartScriptable>()
            };

            var parts = layerTransform.GetComponentsInChildren<BasePart>();
            int partIndex = 0;
            string baseLayerName = LayerMask.LayerToName(layerTransform.gameObject.layer);
            // Loop through all parts in the current layer
            foreach (BasePart partTransform in parts)
            {
                if (partTransform == null) continue; // Ensure partTransform is not null

                BodyPartScriptable partData = new BodyPartScriptable
                {
                    idBodyPart = partIndex++,
                    partName = partTransform.GetComponent<BasePart>().uniqueID,
                    partPosition = partTransform.transform.localPosition,
                    partRotation = partTransform.transform.localRotation,
                    partLocalScale = partTransform.transform.localScale,
                    layer = baseLayerName,
                    colorString = partTransform.Renderer.color.ToString(),
                };
                layerData.parts.Add(partData);
            }

            newLevelData.layers.Add(layerData);
        }

        // Collect screw data
        var screws = levelObject.GetComponentsInChildren<Screw>();
        newLevelData.screws = new List<ScrewScriptable>();
        foreach (var screw in screws)
        {
            ScrewScriptable screwData = new ScrewScriptable()
            {
                screwPosition = screw.Position,
                idScrew = screw.GetInstanceID(),
                idColor = (int)screw.Color,
                hingeConnections = new List<HingeConnection>()
            };

            var hinges = screw.HingeController.HingeJoint2D;
            var listHingeObject = new List<HingeConnection>();
            int i = 0;
            /*var bodyConnected = screw.HingeController.BodyConnect[i];
            var bodyId = bodyConnected.GetComponent<BasePart>().uniqueID;*/
            foreach (var hinge in hinges)
            {
                HingeConnection hingeConnection = new HingeConnection()
                {
                    hingePosition = hinge.transform.localPosition,
                    bodyPartUniqueID = $"{screw.HingeController.BodyConnect[i].GetComponent<BasePart>().uniqueID}",
                    bodyPartHingePosition = hinge.connectedBody.transform.localPosition,
                };

                listHingeObject.Add(hingeConnection);
            }

            screwData.hingeConnections = listHingeObject;
            newLevelData.screws.Add(screwData);
        }

        // Save the new level data as a ScriptableObject in the Resources folder
        return newLevelData;
    }
    /*
    public void SaveGameObjectToLevel(bool isSaveAs, int? currentLevelId = null)
    {
        Level.Level newLevelData;

        // Check if we are saving as a new level or updating the current level
        if (!isSaveAs && currentLevelId.HasValue)
        {
            string path = $"Assets/Resources/Levels/Level_{currentLevelId}.asset";
            Debug.Log($"Loading asset from path: {path}");
            levelData = newLevelData = AssetDatabase.LoadAssetAtPath<Level.Level>(path);

            if (newLevelData == null)
            {
                Debug.LogWarning($"No level data found at {path}, creating a new one.");
                newLevelData = ScriptableObject.CreateInstance<Level.Level>();
                newLevelData.levelId = nextLevelId++;
            }
        }
        else
        {
            newLevelData = this.levelData;
        }


        // Reset layer and screw data
        newLevelData.layers = new List<LayerData>();
        newLevelData.screws = new List<ScrewScriptable>();

        // Save layer data (same logic as before)
        var layers = levelObject.GetComponentsInChildren<BaseLayer>();
        int idLayer = 0;
        foreach (var layerTransform in layers)
        {
            LayerData layerData = new LayerData
            {
                layerId = idLayer++,
                parts = new List<BodyPartScriptable>()
            };

            var parts = layerTransform.GetComponentsInChildren<BasePart>();
            int partIndex = 0;
            string baseLayerName = LayerMask.LayerToName(layerTransform.gameObject.layer);

            foreach (BasePart partTransform in parts)
            {
                if (partTransform == null) continue;

                BodyPartScriptable partData = new BodyPartScriptable
                {
                    idBodyPart = partIndex++,
                    partName = partTransform.GetComponent<BasePart>().uniqueID,
                    partPosition = partTransform.transform.localPosition,
                    partRotation = partTransform.transform.localRotation,
                    partLocalScale = partTransform.transform.localScale,
                    layer = baseLayerName,
                    colorString = partTransform.Renderer.color.ToString(),
                };

                layerData.parts.Add(partData);
            }

            newLevelData.layers.Add(layerData);
        }

        // Save screw data (same logic as before)
        var screws = levelObject.GetComponentsInChildren<Screw>();
        foreach (var screw in screws)
        {
            ScrewScriptable screwData = new ScrewScriptable()
            {
                screwPosition = screw.Position,
                idScrew = screw.GetInstanceID(),
                idColor = (int)screw.Color,
                hingeConnections = new List<HingeConnection>()
            };

            var hinges = screw.HingeController.HingeJoint2D;
            var listHingeObject = new List<HingeConnection>();
            int i = 0;

            foreach (var hinge in hinges)
            {
                HingeConnection hingeConnection = new HingeConnection()
                {
                    hingePosition = hinge.transform.localPosition,
                    bodyPartUniqueID = $"{screw.HingeController.BodyConnect[i].GetComponent<BasePart>().uniqueID}",
                    bodyPartHingePosition = hinge.connectedBody.transform.localPosition,
                };

                listHingeObject.Add(hingeConnection);
            }

            screwData.hingeConnections = listHingeObject;
            newLevelData.screws.Add(screwData);
        }

        // Save the new level data as a ScriptableObject in the Resources folder
        string directoryPath = "Assets/Resources/Levels/";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string assetPath = directoryPath + $"Level_{newLevelData.levelId}.asset";

        // If we are not doing "Save As", delete the existing asset (if it exists)
        if (!isSaveAs && currentLevelId.HasValue && AssetDatabase.LoadAssetAtPath<Level.Level>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
            Debug.Log($"Existing level {currentLevelId} is being overwritten.");
        }
        AssetDatabase.CreateAsset(newLevelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Level data saved: {assetPath}");
    }
    */

    public void LoadLevel(int levelID)
    {
        StartCoroutine(LoadGameObjectFromLevel(levelID));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public IEnumerator LoadGameObjectFromLevel(int levelId)
    {
        currentLoadedLevel = levelId;
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
        Level.Level levelData = allLevels.Find(level => level.levelId == levelId);
        if (levelData == null)
        {
            Debug.LogWarning($"Level with ID {levelId} not found!");
            yield break;
        }

        int layerID = 0;

        List<BaseLayer> listBaseLayer = new();
        // Loop through all layers in the level data
        foreach (var layerData in levelData.layers)
        {
            // Create a new GameObject for the layer
            GameObject layerGameObject = Instantiate(layerBase.gameObject, Vector3.zero, Quaternion.identity);
            layerGameObject.name = $"Layer{layerID++}";
            layerGameObject.transform.SetParent(levelObject.transform);
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
                    partGameObject.transform.localPosition = partData.partPosition;
                    var partComponent = partGameObject.GetComponent<BasePart>();
                    partComponent.uniqueID = partData.partName;
                    layerManager.AddPart(partComponent);
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
        screwManager.transform.position = Vector3.zero;
        var screwTransform = screwManager.transform;

        // Load screws
        foreach (var screwData in levelData.screws)
        {
            GameObject screwGameObject =
                Instantiate(Resources.Load("GameObject/ScrewLevelMaker"), screwTransform) as GameObject;
            screwGameObject.transform.localPosition = screwData.screwPosition;

            ScrewLevelMaker screwComponent = screwGameObject.GetComponent<ScrewLevelMaker>();
            screwComponent.Color = (ColorEnum)screwData.idColor; // Assuming ScrewColor is your enum

            // Handle hinge connections
            foreach (var hingeConnection in screwData.hingeConnections)
            {
                Debug.Log($"Level data with ID {levelId} loaded.");
                var connectedPart = layerManager.GetPartByKey(hingeConnection.bodyPartUniqueID);
                Debug.Log($"Connected part id {connectedPart.uniqueID}");
                screwComponent.CreateHinge(connectedPart.GetComponent<Rigidbody2D>());
            }

            // Add a delay after each screw is loaded
            yield return null; // Wait for one frame before loading the next screw
        }

        this.levelData = levelData;
        Debug.Log($"Level data with ID {levelId} loaded.");
    }

    public void ClearLevel()
    {
        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SpawnScrew()
    {
        if (LevelMaker.instance.isInputData) return;
        var screwManager = levelObject.GetComponentInChildren<ScrewManager>();
        var screw = Instantiate(screwLevelPrefab,
            screwManager.transform);
        var screwComp = screw.GetComponent<ScrewLevelMaker>();
        screwComp.Color =(ColorEnum)LevelMaker.instance.currentScrewColorID;
        screwComp.ChangeScrewColor(screwComp.Color);
        //Debug.Assert(screw != null, nameof(screw) + " != null");
        bool isMouseOnScreen = IsMouseOnScreen();
        if (isMouseOnScreen)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            screwComp.Position = new Vector3(mousePos.x, mousePos.y, 0);
            return;
        }

        screw.transform.position = new Vector3(-5, 0, 0);
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
        var layerID = LevelMaker.instance.GetLayerInputField();
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

    private bool IsMouseOnScreen()
    {
        Vector3 mousePosition = Input.mousePosition;

        // Check if the mouse is within the screen bounds
        bool isWithinXBounds = mousePosition.x >= 0 && mousePosition.x <= Screen.width;
        bool isWithinYBounds = mousePosition.y >= 0 && mousePosition.y <= Screen.height;

        return isWithinXBounds && isWithinYBounds;
    }
}