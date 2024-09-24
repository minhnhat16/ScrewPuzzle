using ConfigFile;
using UnityEditor;
using UnityEngine;

namespace Managers
{
    public class BoxConfigCreator
    {
        private static int idCounter = 1;  // Counter to keep track of the number of created assets

        [MenuItem("Assets/Create/Config/BoxConfig with Auto ID")]
        public static void CreateBoxConfigWithAutoID()
        {
            // Create a new instance of the BoxConfig asset
            BoxConfig newConfig = ScriptableObject.CreateInstance<BoxConfig>();

            // Define the path and generate the name with the counter (id)
            string path = "Assets/Resources/Config/boxConfigLevel" + idCounter + ".asset";
        
            // Create the asset at the specified path
            AssetDatabase.CreateAsset(newConfig, path);

            // Save the asset and increment the counter for the next creation
            AssetDatabase.SaveAssets();
            idCounter++;

            // Focus the Project window and highlight the new asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newConfig;
        }
    }
}
