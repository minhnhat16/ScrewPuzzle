#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ConfigFile;
using Level;
using UnityEditor;
using UnityEngine;

public class LevelBoxConfigMapperWindow : EditorWindow
{
    private const string DefaultLevelsFolder = "Assets/Resources_moved/Levels";
    private const string DefaultBoxConfigsFolder = "Assets/Resources_moved/Config/BoxConfigs";

    private string _levelsFolder = DefaultLevelsFolder;
    private string _boxConfigsFolder = DefaultBoxConfigsFolder;
    private Vector2 _scroll;
    private readonly List<MapResult> _results = new();
    private bool _hasScanned;

    [MenuItem("Tools/Level/Map Box Configs To Levels")]
    private static void Open()
    {
        var window = GetWindow<LevelBoxConfigMapperWindow>("BoxConfig Mapper");
        window.minSize = new Vector2(720f, 420f);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Map BoxConfig -> Level", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Quet Level asset va BoxConfig asset theo ID, sau do gan Level.boxConfig tuong ung.",
            MessageType.Info);

        DrawFolderField("Levels Folder", ref _levelsFolder, DefaultLevelsFolder);
        DrawFolderField("BoxConfigs Folder", ref _boxConfigsFolder, DefaultBoxConfigsFolder);

        EditorGUILayout.Space(8f);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Preview Mapping", GUILayout.Height(28f)))
                Scan();

            using (new EditorGUI.DisabledScope(!_hasScanned || _results.Count == 0))
            {
                if (GUILayout.Button("Apply Mapping", GUILayout.Height(28f)))
                    ApplyMapping();
            }
        }

        EditorGUILayout.Space(8f);
        DrawSummary();
        DrawResults();
    }

    private void DrawFolderField(string label, ref string folderPath, string fallback)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            folderPath = EditorGUILayout.TextField(label, folderPath);
            if (GUILayout.Button("Use Default", GUILayout.Width(100f)))
                folderPath = fallback;
        }
    }

    private void DrawSummary()
    {
        if (!_hasScanned)
            return;

        int matched = _results.Count(r => r.Status == MapStatus.Matched);
        int missing = _results.Count(r => r.Status == MapStatus.MissingBoxConfig);
        int invalid = _results.Count(r => r.Status == MapStatus.InvalidLevelId);

        EditorGUILayout.LabelField(
            $"Levels: {_results.Count} | Matched: {matched} | Missing BoxConfig: {missing} | Invalid Id: {invalid}",
            EditorStyles.helpBox);
    }

    private void DrawResults()
    {
        if (!_hasScanned)
            return;

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (var result in _results)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"Level: {result.LevelAssetPath}");
                EditorGUILayout.LabelField($"Level Id: {result.LevelId}");
                EditorGUILayout.LabelField($"BoxConfig: {result.BoxConfigPath ?? "<missing>"}");
                EditorGUILayout.LabelField($"Status: {result.Status}");
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Scan()
    {
        _results.Clear();
        _hasScanned = true;

        var levelGuids = AssetDatabase.FindAssets("t:Level", new[] { _levelsFolder });
        var boxGuids = AssetDatabase.FindAssets("t:BoxConfig", new[] { _boxConfigsFolder });

        var boxById = new Dictionary<int, BoxConfig>();
        foreach (var guid in boxGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var boxConfig = AssetDatabase.LoadAssetAtPath<BoxConfig>(path);
            if (boxConfig == null) continue;

            int id = ExtractId(path);
            if (id < 0) continue;
            boxById[id] = boxConfig;
        }

        foreach (var guid in levelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var level = AssetDatabase.LoadAssetAtPath<Level.Level>(path);
            if (level == null) continue;

            int levelId = level.levelId >= 0 ? level.levelId : ExtractId(path);
            if (levelId < 0)
            {
                _results.Add(new MapResult(path, levelId, null, MapStatus.InvalidLevelId));
                continue;
            }

            if (boxById.TryGetValue(levelId, out var boxConfig))
            {
                _results.Add(new MapResult(path, levelId, AssetDatabase.GetAssetPath(boxConfig), MapStatus.Matched));
            }
            else
            {
                _results.Add(new MapResult(path, levelId, null, MapStatus.MissingBoxConfig));
            }
        }

        _results.Sort((a, b) => a.LevelId.CompareTo(b.LevelId));
    }

    private void ApplyMapping()
    {
        if (!_hasScanned)
            Scan();

        int changedCount = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var result in _results)
            {
                if (result.Status != MapStatus.Matched)
                    continue;

                var level = AssetDatabase.LoadAssetAtPath<Level.Level>(result.LevelAssetPath);
                var boxConfig = AssetDatabase.LoadAssetAtPath<BoxConfig>(result.BoxConfigPath);
                if (level == null || boxConfig == null)
                    continue;

                if (level.boxConfig == boxConfig)
                    continue;

                Undo.RecordObject(level, "Map BoxConfig To Level");
                level.boxConfig = boxConfig;
                EditorUtility.SetDirty(level);
                changedCount++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LevelBoxConfigMapper] Applied mapping to {changedCount} level(s).");
        Scan();
    }

    private static int ExtractId(string text)
    {
        if (string.IsNullOrEmpty(text))
            return -1;

        var match = Regex.Match(text, @"(\d+)(?!.*\d)");
        return match.Success && int.TryParse(match.Value, out int id) ? id : -1;
    }

    private enum MapStatus
    {
        Matched,
        MissingBoxConfig,
        InvalidLevelId
    }

    private readonly struct MapResult
    {
        public MapResult(string levelAssetPath, int levelId, string boxConfigPath, MapStatus status)
        {
            LevelAssetPath = levelAssetPath;
            LevelId = levelId;
            BoxConfigPath = boxConfigPath;
            Status = status;
        }

        public string LevelAssetPath { get; }
        public int LevelId { get; }
        public string BoxConfigPath { get; }
        public MapStatus Status { get; }
    }
}
#endif
