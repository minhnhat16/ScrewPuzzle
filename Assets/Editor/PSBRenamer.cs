using UnityEngine;
using UnityEditor;
using System.IO;

public class PSBBatchRenamer : EditorWindow
{
    private string folderPath = "Assets/Resources_moved/Sprites/HINH/level_screw"; // Đường dẫn thư mục chứa các prefab .psb

    [MenuItem("Tools/Batch Rename PSB Layers")]
    public static void ShowWindow()
    {
        GetWindow<PSBBatchRenamer>("Batch PSB Renamer");
    }

    void OnGUI()
    {
        GUILayout.Label("Đổi tên các layer trong nhiều prefab .psb", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Thư mục chứa prefab", folderPath);

        if (GUILayout.Button("Đổi tên tất cả prefab trong thư mục"))
        {
            RenameAllPrefabs();
        }
    }

    void RenameAllPrefabs()
    {
        string[] prefabPaths = Directory.GetFiles(folderPath, "*.psb", SearchOption.AllDirectories);

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            Transform aGroup = prefab.transform.Find("a");
            Transform bGroup = prefab.transform.Find("b");

            if (aGroup != null)
                RenameChildrenInGroup(aGroup, "a", prefab.name);

            if (bGroup != null)
                RenameChildrenInGroup(bGroup, "b", prefab.name);

            EditorUtility.SetDirty(prefab);
            Debug.Log($"✅ Đã xử lý prefab: {prefab.name}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"🎉 Hoàn tất đổi tên tất cả prefab trong: {folderPath}");
    }

    void RenameChildrenInGroup(Transform group, string suffix, string level)
    {
        int index = 1;

        Debug.Log("Chill count " + group.childCount + "group name " + group.name);
        foreach (Transform child in group)
        {
            string newName = $"Layer_{index}";
            child.name = newName;

            int j = 0;
            foreach (Transform part in child.transform)
            {
                string partname = $"{level}_{newName}_{j}_{suffix}";
                var sr = part.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    sr.sprite.name = partname;
                    sr.name = partname;
                    Debug.Log("Chill " + partname);
                    var lib = UnityEngine.Object.FindFirstObjectByType<SpriteLibControl>();
                    lib.AddSprite(sr.sprite); ;
                }
                j++;
            }

            index++;
        }
    }
}
public interface IPartSpriteService
{
    Sprite GetPartSprite(int levelId, string spriteName, bool outline);
}
