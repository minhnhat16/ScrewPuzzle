#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class LevelAssetAssignerWindow : EditorWindow
{
    private string _selectedFolderPath = "";
    private string _groupName = "Levels";
    private int _levelPerChapter = 10;
    private Vector2 _scroll;
    private string[] _foundAssets = new string[0];

    [MenuItem("Tools/Level Asset Assigner")]
    public static void Open() => GetWindow<LevelAssetAssignerWindow>("Level Asset Assigner");

    private void OnGUI()
    {
        GUILayout.Label("Level Asset Assigner", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- Chọn folder ---
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Folder", _selectedFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Level Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path → relative (Assets/...)
                if (path.StartsWith(Application.dataPath))
                    _selectedFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                else
                    Debug.LogWarning("Folder phải nằm trong thư mục Assets!");

                ScanFolder();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // --- Config ---
        _groupName = EditorGUILayout.TextField("Addressable Group Name", _groupName);
        _levelPerChapter = EditorGUILayout.IntField("Levels per Chapter", _levelPerChapter);

        EditorGUILayout.Space();

        // --- Preview assets tìm được ---
        if (_foundAssets.Length > 0)
        {
            GUILayout.Label($"Found {_foundAssets.Length} assets:", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(200));
            foreach (var asset in _foundAssets)
                EditorGUILayout.LabelField("  • " + Path.GetFileName(asset));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // --- Assign button ---
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Assign to Addressables", GUILayout.Height(35)))
                AssignToAddressables();
            GUI.backgroundColor = Color.white;
        }
        else if (!string.IsNullOrEmpty(_selectedFolderPath))
        {
            EditorGUILayout.HelpBox("Không tìm thấy asset nào trong folder này.", MessageType.Warning);
        }
    }

    private void ScanFolder()
    {
        if (string.IsNullOrEmpty(_selectedFolderPath)) return;

        // Tìm tất cả asset trong folder (lọc theo đuôi bạn dùng)
        var guids = AssetDatabase.FindAssets("t:Object", new[] { _selectedFolderPath });
        _foundAssets = System.Array.ConvertAll(guids, g => AssetDatabase.GUIDToAssetPath(g));
        Repaint();
    }

    private void AssignToAddressables()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Settings not found! Hãy khởi tạo Addressables trước.");
            return;
        }

        // Tạo group nếu chưa có
        var group = settings.FindGroup(_groupName)
                    ?? settings.CreateGroup(_groupName, false, false, true, null);

        int added = 0;
        for (int i = 0; i < _foundAssets.Length; i++)
        {
            string assetPath = _foundAssets[i];
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // Tạo entry trong Addressables
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = assetName;

            // Set label Chapter
            int levelNum = i + 1;
            int chapterNum = Mathf.CeilToInt((float)levelNum / _levelPerChapter);
            string chapLabel = $"Chapter_{chapterNum:D2}";
            string lvlLabel = $"Level_{levelNum:D3}";

            settings.AddLabel(chapLabel);
            settings.AddLabel(lvlLabel);
            entry.SetLabel(chapLabel, true);
            entry.SetLabel(lvlLabel, true);

            added++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[LevelAssigner] Done! Assigned {added} assets vào group '{_groupName}'");
        EditorUtility.DisplayDialog("Done!", $"Đã assign {added} assets vào Addressables!", "OK");
    }
}
#endif