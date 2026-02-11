using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelMigrationWindow : EditorWindow
{
    private List<Level.Level> levels = new();

    [MenuItem("Tools/Level/Migration Window")]
    static void Open()
    {
        GetWindow<LevelMigrationWindow>("Level Migration");
    }

    private void DrawDropArea()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(
            0f, 60f,
            GUILayout.ExpandWidth(true));

        GUI.Box(dropArea, "Drag Level assets here", EditorStyles.helpBox);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is Level.Level level && !levels.Contains(level))
                        {
                            levels.Add(level);
                        }
                    }
                }
                evt.Use();
                break;
        }
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("Level Migration Tool", EditorStyles.boldLabel);

        DrawDropArea();
        if (GUILayout.Button("Migrate"))    
        {
            if (!EditorUtility.DisplayDialog(
                "Migrate Levels",
                $"Migrate {levels.Count} level(s)?",
                "Migrate",
                "Cancel"))
                return;

            foreach (var level in levels)
            {
                if (level != null)
                    Migrate(level);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Migration] Migrated {levels.Count} level(s)");
        }
        EditorGUILayout.Space();

        for (int i = 0; i < levels.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.ObjectField(levels[i], typeof(Level.Level), false);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                levels.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        GUI.enabled = levels.Count > 0;
       
        GUI.enabled = true;
    }

    private void Migrate(Level.Level level)
    {
        if (level == null)
            return;

        if (level.screws == null || level.screws.Count == 0)
        {
            Debug.LogWarning($"[Migration] {level.name}: no screws");
            return;
        }

        Undo.RecordObject(level, "Migrate Level Hinges");

        int migrated = 0;
        int skipped = 0;

        foreach (var screw in level.screws)
        {
            if (screw == null)
            {
                Debug.LogWarning($" screw has null 0");

                skipped++;
                continue;
            }

            // Đã migrate rồi → skip
            if (!string.IsNullOrEmpty(screw.hinge.bodyPartUniqueID))    
            {
                Debug.LogWarning($" screw has null body ");
                skipped++;
                continue;
            }

            if (screw.hingeConnections == null || screw.hingeConnections.Count == 0)
            {
                Debug.LogWarning($"[Migration] {level.name}: screw has no hingeConnections");
                skipped++;
                continue;
            }

            screw.hinge = screw.hingeConnections[0];
            migrated++;
        }

        if (migrated > 0)
        {
            EditorUtility.SetDirty(level);
        }

        Debug.Log($"[Migration] {level.name} | migrated={migrated}, skipped={skipped}");
    }


}
