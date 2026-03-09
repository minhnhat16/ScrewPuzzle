using Enums;
using Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxSequenceService : IBoxSequenceService
{
    private List<Box> _queue = new();

    public void Load(IEnumerable<Box> boxes)
    {
        _queue = boxes?.ToList() ?? new List<Box>();

        Debug.Log("[BoxSequenceService] Loaded boxes: " +
                  $"[{string.Join(", ", _queue.Select(b => b == null ? "null" : b.Color.ToString()))}]");
    }

    public Box GetNext()
    {
        if (_queue.Count == 0) return null;
        var box = _queue[0];
        _queue.RemoveAt(0);
        return box;
    }

    public bool HasNext() => _queue.Count > 0;

    public Box TryDequeueMatching(Func<Box, bool> predicate)
    {
        if (predicate == null) return null;

        if (_queue.Count == 0)
        {
            Debug.Log("[BoxSequenceService] TryDequeueMatching — queue rỗng.");
            return null;
        }

        Debug.Log($"[BoxSequenceService] TryDequeueMatching — queue có {_queue.Count} box: " +
                  $"[{string.Join(", ", _queue.Select(b => b == null ? "null" : b.Color.ToString()))}]");

        var match = _queue.FirstOrDefault(predicate);

        if (match == null)
        {
            Debug.Log("[BoxSequenceService] TryDequeueMatching — không tìm thấy box thỏa predicate.");
            return null;
        }

        _queue.Remove(match);
        Debug.Log($"[BoxSequenceService] TryDequeueMatching — matched color={match.Color}, còn lại {_queue.Count} box.");
        return match;
    }

    public Dictionary<ColorEnum, int> GetColorCounts()
    {
        var result = new Dictionary<ColorEnum, int>();
        foreach (var box in _queue)
        {
            if (box == null)
            {
                Debug.LogWarning("[BoxSequenceService] GetColorCounts — box null trong queue!");
                continue;
            }
            Debug.Log($"[BoxSequenceService] GetColorCounts — box '{box.name}' color={box.Color} active={box.gameObject.activeSelf}");
            if (!result.ContainsKey(box.Color))
                result[box.Color] = 0;
            result[box.Color]++;
        }
        Debug.Log($"[BoxSequenceService] GetColorCounts result: [{string.Join(", ", result.Select(kv => $"{kv.Key}={kv.Value}"))}]");
        return result;
    }
}