#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class BYDataTableMaker
{
    [MenuItem("Assets/BY/Create Binary file for Tab Delimeter(txt)", false, 1)]
    private static void CreateBinaryFile()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            TextAsset txtFile = (TextAsset)obj;
            Debug.Log(txtFile.text);
            string tableName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(txtFile));

            ScriptableObject scriptable = ScriptableObject.CreateInstance(tableName);
            if (scriptable == null)
                return;

            AssetDatabase.CreateAsset(scriptable, "Assets/Resources/Config/" + tableName + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BYDataBase bYDataBase = (BYDataBase)scriptable;
            bYDataBase.CreateBinaryFile(txtFile);
            EditorUtility.SetDirty(bYDataBase);
        }
    }
}
#endif