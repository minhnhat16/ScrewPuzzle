using UnityEngine;
using System.Collections.Generic;
using System.IO;
using ConfigFile;
using Enum;
using UnityEngine.Serialization;

namespace Level
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level")]
    public class Level : ScriptableObject
    {
        public int levelId;
        public int totalColorInLevel;
        public LayerEnum layer; 
        public BoxConfig boxConfig;
        public List<ScrewScriptable> screws;    
        public List<LayerData> layers;
        
        public Level() // Default constructor for creating an empty LevelData
        {
            screws = new List<ScrewScriptable>();
            layers = new List<LayerData>();
        }
        // Function to save the Level data to a JSON file
        public void SaveLevel(string filePath)
        {
            // Convert the Level object to a serializable class
            LevelData levelData = new LevelData(this);

            // Serialize to JSON
            string json = JsonUtility.ToJson(levelData, true);

            // Write the JSON to a file
            File.WriteAllText(filePath, json);
        }

        // Function to load Level data from a JSON file
        public void LoadLevel(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Read the JSON from the file
                string json = File.ReadAllText(filePath);

                // Deserialize the JSON back into a LevelData object
                LevelData levelData = JsonUtility.FromJson<LevelData>(json);

                // Restore the data to this Level instance
                ApplyData(levelData);
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
            }
        }

        // Apply the loaded data back to this Level instance
        private void ApplyData(LevelData data)
        {
            totalColorInLevel = data.totalColorInLevel;
            layer = data.layer;
            boxConfig = data.connectors;
            screws = data.screws;
        }
    }

    [System.Serializable]
    public class ScrewScriptable
    {
        public int idScrew;
        public int idColor;
        public Vector3 screwPosition;
        public List<HingeConnection> hingeConnections;
    }

    [System.Serializable]
    public class HingeConnection
    {
        public Vector3 hingePosition;
        public string bodyPartUniqueID;
        public Vector3 bodyPartHingePosition;
    }

    [System.Serializable]
    public class BodyPartScriptable
    {
        public int idBodyPart;
        public string partName;
        public string spriteName;
        public string layer;
        public string colorString;
        public Vector3 partPosition;
        public Vector3 partLocalScale;
        public Quaternion partRotation;

        // Convert Color to RGBA string
        public void SetColor(Color color)
        {
            colorString = $"R:{(int)(color.r * 255)}, G:{(int)(color.g * 255)}, B:{(int)(color.b * 255)}, A:{color.a}";
        }
        public Color GetColorFromString()
        {
            // Split the string into parts
            string[] parts = colorString.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        
            if (parts.Length == 4) // Check if we have exactly 4 components
            {
                int r = int.Parse(parts[0].Split(':')[1]);
                int g = int.Parse(parts[1].Split(':')[1]);
                int b = int.Parse(parts[2].Split(':')[1]);
                float a = float.Parse(parts[3].Split(':')[1]);

                return new Color(r / 255f, g / 255f, b / 255f, a);
            }

            // Fallback color if parsing fails
            return Color.white; 
        }
    }
    [System.Serializable]
    public class LayerData
    {
        public int layerId;
        public List<BodyPartScriptable> parts;
    }

    // A serializable class that holds all the data for saving and loading the level
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public Vector3[] hingesPosition;
        public int totalColorInLevel;
        public LayerEnum layer;
        public BoxConfig connectors;
        public List<ScrewScriptable> screws;
        public List<BodyPartScriptable> bodyParts;

        // Constructor to convert the Level object into LevelData for saving
        public LevelData(Level level)
        {
            totalColorInLevel = level.totalColorInLevel;
            layer = level.layer;
            connectors = level.boxConfig;
            screws = level.screws;
        }
    }
}
