using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Enum;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using UnityEditor; // Ensure this is used only in the Editor context

public class GameObjectToLevelConverter : MonoBehaviour
{
    public int nextLevelId = 0;  // Static variable to track the next level ID
    public Level.Level levelData;  // Reference to the Level ScriptableObject
    public GameObject levelObject;  // The parent GameObject that holds all the parts, layers, screws
    public List<Level.Level> allLevels;  // List of all available levels

  private void Start()
  {
      // Load all Level assets from the Resources/Levels folder
      allLevels = new List<Level.Level>(Resources.LoadAll<Level.Level>("Levels"));
      
      // Ensure allLevels is not empty
      if (allLevels != null && allLevels.Count > 0)
      {
          // Find the highest level ID in the list of levels
          nextLevelId = 0;
          foreach (var level in allLevels)
          {
              if (level.levelId >= nextLevelId)
              {
                  nextLevelId = level.levelId + 1;
              }
          }
      }
      else
      {
          // If no levels are present, start with the first ID
          nextLevelId = 1;
      }
  }

    public void SaveGameObjectToLevel()
    {
        // Create a new Level ScriptableObject instance
        Level.Level newLevelData = ScriptableObject.CreateInstance<Level.Level>();

        // Assign a unique ID to the level
        newLevelData.levelId = nextLevelId++;

        newLevelData.layers = new List<LayerData>();
        
        var layers = levelObject.GetComponentsInChildren<BaseLayer>();
        
        // Loop through all layers
        foreach (var layerTransform in layers)
        {
            LayerData layerData = new LayerData
            {
                layerId = layerTransform.GetInstanceID(),
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
                    partName = partTransform.gameObject.name,
                    partPosition = partTransform.transform.localPosition,
                    partRotation = partTransform.transform.localRotation,
                    partLocalScale = partTransform.transform.localScale,
                    layer = baseLayerName,
                    colorString =  partTransform.Renderer.color.ToString(),
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
            foreach (var hinge in hinges)
            {
                HingeConnection hingeConnection = new HingeConnection()
                {
                    hingePosition = hinge.transform.localPosition,
                    bodyPartName = screw.HingeController.BodyConnect[i].name,
                    bodyPartHingePosition = hinge.connectedBody.transform.localPosition,
                };
                
                listHingeObject.Add(hingeConnection);
            }
            screwData.hingeConnections = listHingeObject;
            newLevelData.screws.Add(screwData);
        }

        // Save the new level data as a ScriptableObject in the Resources folder
        string directoryPath = "Assets/Resources/Levels"; // Set the directory path
        if (!Directory.Exists(directoryPath)) // Check if the directory exists
        {
            Directory.CreateDirectory(directoryPath); // Create the directory if it doesn't exist
        }

        string assetPath = Path.Combine(directoryPath, $"Level_{newLevelData.levelId}.asset"); // Set the path where you want to save the asset
        
        // Check if the asset already exists
        if (AssetDatabase.LoadAssetAtPath<Level.Level>(assetPath) != null)
        {
            // If it exists, you can choose to either overwrite it or handle it differently
            Debug.LogWarning($"Asset already exists at {assetPath}. Overwriting the existing asset.");
            AssetDatabase.DeleteAsset(assetPath); // Remove the existing asset if you want to overwrite it
        }

        AssetDatabase.CreateAsset(newLevelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // Refresh the AssetDatabase to see the changes

        Debug.Log("Level data saved as ScriptableObject: " + assetPath);
    }

      public void LoadGameObjectFromLevel(int levelId)
    {
        // Clear existing GameObjects in levelObject to prepare for new loading
        foreach (Transform child in levelObject.transform)
        {
            Destroy(child.gameObject);
        }

        // Find the level data with the given ID
        Level.Level levelData = allLevels.Find(level => level.levelId == levelId);
        if (levelData == null)
        {
            Debug.LogWarning($"Level with ID {levelId} not found!");
            return;
        }

        // Loop through all layers in the level data
        foreach (var layerData in levelData.layers)
        {
            // Create a new GameObject for the layer
            GameObject layerGameObject = new GameObject($"Layer_{layerData.layerId}");
            layerGameObject.transform.SetParent(levelObject.transform);
            // Loop through all parts in the current layer
            foreach (var partData in layerData.parts)
            {
                // Instantiate a new GameObject for the part
                GameObject partGameObject = new GameObject(partData.partName);
                partGameObject.transform.SetParent(layerGameObject.transform);
                partGameObject.transform.localPosition = partData.partPosition;
                // Optionally, add your BasePart component to the partGameObject
                BasePart basePart = partGameObject.AddComponent<BasePart>();
                // Initialize any additional properties of basePart if needed
            }
        }

        // Load screws
        foreach (var screwData in levelData.screws)
        {
            // Instantiate a new GameObject for the screw
            GameObject screwGameObject = new GameObject($"Screw_{screwData.idScrew}");
            screwGameObject.transform.SetParent(levelObject.transform);
            screwGameObject.transform.localPosition = screwData.screwPosition;

            // Optionally, add your Screw component to the screwGameObject
            Screw screwComponent = screwGameObject.AddComponent<Screw>();
            screwComponent.Color = (ColorEnum)screwData.idColor; // Assuming ScrewColor is your enum
            // Initialize any additional properties of screwComponent if needed

            // Handle hinge connections
            foreach (var hingeConnection in screwData.hingeConnections)
            {
                // Create hinge or joint connection logic
                HingeJoint2D hinge = screwGameObject.AddComponent<HingeJoint2D>();
                hinge.connectedBody = FindConnectedBody(hingeConnection.bodyPartName); // Implement this method to find the connected body
                hinge.anchor = hingeConnection.hingePosition; // Set hinge anchor position
            }
        }

        Debug.Log($"Level data with ID {levelId} loaded.");
    }

    private Rigidbody2D FindConnectedBody(string bodyPartName)
    {
        // Implement logic to find the connected Rigidbody2D by its name
        GameObject bodyObject = GameObject.Find(bodyPartName);
        return bodyObject != null ? bodyObject.GetComponent<Rigidbody2D>() : null;
    }
}
