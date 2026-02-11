using System.Collections.Generic;
using UnityEngine;

public static class TutorialTargetRegistry
{
    private static readonly Dictionary<string, Transform> targets = new();

    public static void Register(string key, Transform transform)
    {
        if (!targets.ContainsKey(key))
            targets.Add(key, transform);

        Debug.Log($"[TutorialTargetRegistry] Registered target '{key}'");   
    }

    public static void Unregister(string key)
    {
        if (targets.ContainsKey(key))
            targets.Remove(key);
    }

    public static Transform Get(string key)
    {
        targets.TryGetValue(key, out var t);
        return t;
    }
}
