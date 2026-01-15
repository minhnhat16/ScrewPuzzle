#if UNITY_EDITOR
using System;
using System.Linq;
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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== Trim Character ===", EditorStyles.boldLabel);
        if (GUILayout.Button("Trim Character"))
        {
            TrimCharacter(level);
        }
    }

    private void TrimCharacter(Level.Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("[LevelEditor] TrimCharacter: level is null");
            return;
        }

        if (level.layers == null || level.layers.Count == 0)
        {
            Debug.LogWarning("[LevelEditor] TrimCharacter: no layers to process");
            return;
        }

        int changed = 0;

        foreach (var layer in level.layers)
        {
            if (layer == null || layer.parts == null) continue;

            foreach (var part in layer.parts)
            {
                if (part == null || string.IsNullOrEmpty(part.spriteName)) continue;

                string original = part.spriteName;

                // Split by '_' and remove any token that is exactly "0".
                // This avoids corrupting names like "icon_10" (token "10" stays intact).
                var tokens = original.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0) continue;

                var filtered = System.Linq.Enumerable.Where(tokens, t => t != "0").ToArray();
                string trimmed = string.Join("_", filtered);

                if (!string.Equals(trimmed, original, StringComparison.Ordinal))
                {
                    part.spriteName = trimmed;
                    changed++;
                    Debug.Log($"[LevelEditor] TrimCharacter: '{original}' -> '{trimmed}'");
                }
            }
        }

        if (changed > 0)
        {
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
            Debug.Log($"[LevelEditor] TrimCharacter: trimmed {changed} part sprite name(s).");
        }
        else
        {
            Debug.Log("[LevelEditor] TrimCharacter: nothing to trim.");
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
            screw.hinge = screw.hingeConnections[0];
            migrated++;
           
        }

        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Migration] Completed! Migrated {migrated} screws.");
    }
}
#endif
