#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class FindMissingScriptsPlus
{
    [MenuItem("Tools/Missing Scripts/Find in Open Scenes")]
    private static void FindInOpenScenes()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            var components = go.GetComponents<Component>();

            Debug.Log($"finding Script on Scene Object: {GetPath(go)}", go);

            foreach (var c in components)
            {
                if (c == null)
                {
                    Debug.LogError($"❌ Missing Script on Scene Object: {GetPath(go)}", go);
                    break;
                }
            }
        }
    }

    [MenuItem("Tools/Missing Scripts/Find in Prefab Assets (Project)")]
    public static void FindMissingScriptsInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            ScanPrefab(prefab, path);
        }
    }

    static void ScanPrefab(GameObject go, string path)
    {
        foreach (var c in go.GetComponents<Component>())
        {
            if (c == null)
            {
                Debug.LogError($"❌ Missing Script in Prefab: {path}", go);
                return;
            }
        }

        foreach (Transform child in go.transform)
        {
            ScanPrefab(child.gameObject, path);
        }
    }


    static bool RemoveMissingScriptsFromPrefab(GameObject prefabRoot, string path)
    {
        bool removedAny = false;

        // Instantiate prefab để sửa
        GameObject instance = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;

        try
        {
            removedAny = RemoveMissingRecursive(instance);

            if (removedAny)
            {
                PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
                Debug.Log($"✅ Removed missing scripts in prefab: {path}");
            }
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }

        return removedAny;
    }

    static bool RemoveMissingRecursive(GameObject go)
    {
        bool removed = false;

        Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");

        // API chính thức để remove missing
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        if (count > 0)
            removed = true;

        foreach (Transform child in go.transform)
        {
            if (RemoveMissingRecursive(child.gameObject))
                removed = true;
        }

        return removed;
    }
    private static int FindMissingInScene(Scene scene)
    {
        int count = 0;
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            count += FindMissingOnGameObject(root, $"[SCENE] {scene.name}");
        }
        return count;
    }

    private static int FindMissingOnGameObject(GameObject go, string context)
    {
        int count = 0;
        // Dùng GetComponents<Component>() để bắt missing component = null
        var comps = go.GetComponents<Component>();
        Debug.Log($"Finding {go.name} has {comps.Length} childs");

        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == null)
            {
                string path = GetPath(go);
                Debug.LogError($"❌ Missing Script on: {path}  {context}", go);
                count++;
            }
        }

        // Quét children
        for (int i = 0; i < go.transform.childCount; i++)
        {
            count += FindMissingOnGameObject(go.transform.GetChild(i).gameObject, context);
        }

        return count;
    }

    private static string GetPath(GameObject go)
    {
        // Trường hợp name rỗng -> show placeholder để khỏi "(Game Object '')"
        string name = string.IsNullOrEmpty(go.name) ? "<EMPTY_NAME>" : go.name;

        string path = name;
        var t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            string parentName = string.IsNullOrEmpty(t.name) ? "<EMPTY_NAME>" : t.name;
            path = parentName + "/" + path;
        }
        return path;
    }
}
#endif
