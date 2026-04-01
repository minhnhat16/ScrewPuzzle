using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
public static class TutorialEventBus
{
    private static readonly Dictionary<string, Action<object>> events = new();

    public static void Subscribe(string eventKey, Action<object> callback)
    {
        if (!events.ContainsKey(eventKey))
            events[eventKey] = delegate { };

        events[eventKey] += callback;
    }

    public static void Unsubscribe(string eventKey, Action<object> callback)
    {
        if (events.ContainsKey(eventKey))
            events[eventKey] -= callback;
    }

    public static void Emit(string eventKey, object payload = null)
    {
        //Debug.Log($"[TutorialEventBus] Emit event '{eventKey}' with payload: {payload}");
        if (events.ContainsKey(eventKey))
            events[eventKey]?.Invoke(payload);
    }
    /// <summary>Xóa toàn bộ subscriptions — gọi khi tutorial kết thúc hoặc level reset.</summary>
    public static void Clear()
    {
        events.Clear();
        //Debug.Log("[TutorialEventBus] Cleared all subscriptions.");
    }
}
