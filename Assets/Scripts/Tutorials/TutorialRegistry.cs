using System.Collections.Generic;
using UnityEngine;

public static class TutorialTargetRegistry
{
    private static readonly Dictionary<string, Transform> targets = new();

    public static void Register(string key, Transform transform)
    {
        // Overwrite nếu đã tồn tại — level reload sẽ update đúng reference
        targets[key] = transform;
        Debug.Log($"[TutorialTargetRegistry] Registered '{key}' → {transform.name}");
    }

    public static void Unregister(string key)
    {
        targets.Remove(key);
    }

    public static Transform Get(string key)
    {
        targets.TryGetValue(key, out var t);
        if (t == null)
            Debug.LogWarning($"[TutorialTargetRegistry] Key '{key}' not found. " +
                             $"Registered keys: [{string.Join(", ", targets.Keys)}]");
        return t;
    }

    /// <summary>Xóa toàn bộ — gọi khi load level mới.</summary>
    public static void Clear()
    {
        targets.Clear();
        Debug.Log("[TutorialTargetRegistry] Cleared all targets.");
    }
}
