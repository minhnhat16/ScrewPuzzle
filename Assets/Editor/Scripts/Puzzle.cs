#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PuzzleCellConfig))]
public class PuzzleCellConfigEditor : Editor
{
    private int patternFilter = 1001;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Pattern Tools", EditorStyles.boldLabel);

        // ===== INPUT =====
        patternFilter = EditorGUILayout.IntField("Pattern Id", patternFilter);

        GUILayout.Space(5);

        // ===== BUTTONS =====
        if (GUILayout.Button("Sort Pattern Cells"))
        {
            SortPattern(patternFilter);
        }

        if (GUILayout.Button("Preview Pattern Grid (Console)"))
        {
            PreviewPattern(patternFilter);
        }

        if (GUILayout.Button("Validate Pattern (5x5)"))
        {
            ValidatePattern(patternFilter);
        }
    }

    // ===============================
    // SORT
    // ===============================
    private void SortPattern(int patternId)
    {
        var config = (PuzzleCellConfig)target;
        var records = config.GetAllRecord();
        var sorted = records
            .Where(c => c.PatternId == patternId)
            .OrderByDescending(c => c.Y)
            .ThenBy(c => c.X)
            .ToList();

        int index = 0;
        for (int i = 0; i < records.Count; i++)
        {
            if (config.GetAllRecord()[i].PatternId == patternId)
            {
                records[i] = sorted[index++];
            }
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();

        Debug.Log($"[PuzzleCellConfig] Sorted pattern {patternId}");
    }

    // ===============================
    // PREVIEW GRID
    // ===============================
    private void PreviewPattern(int patternId)
    {
        var config = (PuzzleCellConfig)target;

        var cells = config.GetAllRecord()
            .Where(c => c.PatternId == patternId)
            .ToList();

        if (cells.Count == 0)
        {
            Debug.LogWarning($"No cells for pattern {patternId}");
            return;
        }

        int maxX = cells.Max(c => c.X);
        int maxY = cells.Max(c => c.Y);

        string output = $"Pattern {patternId}\n";

        for (int y = maxY; y >= 0; y--)
        {
            for (int x = 0; x <= maxX; x++)
            {
                var cell = cells.FirstOrDefault(c => c.X == x && c.Y == y);
                output += cell != null
                    ? cell.BlockId.ToString().PadLeft(3)
                    : "  .";
            }
            output += "\n";
        }

        Debug.Log(output);
    }

    // ===============================
    // VALIDATE
    // ===============================
    private void ValidatePattern(int patternId)
    {
        var config = (PuzzleCellConfig)target;

        var cells = config.GetAllRecord()
            .Where(c => c.PatternId == patternId)
            .ToList();

        if (cells.Count != 25)
        {
            Debug.LogError(
                $"[Pattern {patternId}] Invalid cell count: {cells.Count} (expected 25)"
            );
            return;
        }

        var duplicate = cells
            .GroupBy(c => (c.X, c.Y))
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate != null)
        {
            Debug.LogError(
                $"[Pattern {patternId}] Duplicate cell at ({duplicate.Key.X},{duplicate.Key.Y})"
            );
            return;
        }

        Debug.Log($"[Pattern {patternId}] VALID 5x5 grid ✔");
    }
 
}

#if UNITY_EDITOR
public static class PuzzleCellConfigExporter
{
    [MenuItem("Tools/Puzzle/Export PuzzleCellConfig To CSV")]
    public static void Export()
    {
        var config = Resources.Load<PuzzleCellConfig>("Config/PuzzleCellConfig");
        if (config == null)
        {
            UnityEngine.Debug.LogError("PuzzleCellConfig not found");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("id,patternId,x,y,blockId,screwRequired");

        foreach (var c in config.GetAllRecord())
        {
            sb.AppendLine(
                $"{c.Id},{c.PatternId},{c.X},{c.Y},{c.BlockId},{c.ScrewRequired}"
            );
        }

        string path = "Assets/PuzzleCellConfig.csv";
        File.WriteAllText(path, sb.ToString());

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log($"Exported CSV to {path}");
    }
}
#endif

#endif
