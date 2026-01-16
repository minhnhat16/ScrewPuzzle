#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts in Scene")]
    static void FindInScene()
    {
        var allGOs = Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
);
        int count = 0;

        foreach (var go in allGOs)
        {
            var components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null)
                {
                    Debug.LogError($"❌ Missing Script found on GameObject: {GetPath(go)}", go);
                    count++;
                }
            }
        }

        Debug.Log($"🔎 Done. Found {count} missing scripts in Scene.");
    }

    static string GetPath(GameObject go)
    {
        string path = go.name;
        while (go.transform.parent != null)
        {
            go = go.transform.parent.gameObject;
            path = go.name + "/" + path;
        }
        return path;
    }
}
#endif
