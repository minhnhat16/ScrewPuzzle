using UIScript;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopPrefabDB", menuName = "Game/Shop Prefab DB")]
public class ShopPrefabDatabase : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public PackType type;
        public PackItem prefab;
    }

    public Entry[] entries;

    public PackItem GetPrefab(PackType type)
    {
        foreach (var e in entries)
            if (e.type == type)
                return e.prefab;

        Debug.LogError("Prefab not found for type: " + type);
        return null;
    }
}
