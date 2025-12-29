#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level.Level))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vẽ UI mặc định
        DrawDefaultInspector();

        Level.Level level = (Level.Level)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== Migration Tools ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Migrate Screws Hinges Data"))
        {
            Migrate(level);
        }
    }

    private void Migrate(Level.Level level)
    {
        if (level.screws == null)
        {
            Debug.LogWarning("Level has no screws to migrate!");
            return;
        }

        int migrated = 0;


        Debug.Log("Level screw " + level.screws.Count);   
        foreach (var screw in level.screws)
        {
            if (screw == null)
                continue;
         //   screw.hinge = screw.hingeConnections[0];
            migrated++;
           
        }

        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Migration] Completed! Migrated {migrated} screws.");
    }
}
#endif
