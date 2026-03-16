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

        var match = _queue.FirstOrDefault(predicate);
        if (match == null) return null;

        _queue.Remove(match);
        Debug.Log($"[BoxSequenceService] TryDequeueMatching — matched color={match.Color}, còn lại {_queue.Count} box.");
        return match;
    }

    public Dictionary<ColorEnum, int> GetColorCounts()
    {
        var result = new Dictionary<ColorEnum, int>();
        foreach (var box in _queue)
        {
            if (box == null) continue;
            if (!result.ContainsKey(box.Color))
                result[box.Color] = 0;
            result[box.Color]++;
        }
        return result;
    }

    /// <summary>
    /// Xóa tối đa <paramref name="count"/> box màu <paramref name="color"/>
    /// khỏi sequence (chưa spawn) và return về pool.
    /// Trả về số box thực sự đã xóa.
    /// </summary>
    public int RemoveByColor(ColorEnum color, int count)
    {
        int removed = 0;
        for (int i = _queue.Count - 1; i >= 0 && removed < count; i--)
        {
            if (_queue[i] != null && _queue[i].Color == color)
            {
                var box = _queue[i];
                _queue.RemoveAt(i);
                box.gameObject.SetActive(false); // return về pool state
                removed++;
            }
        }
        Debug.Log($"[BoxSequenceService] RemoveByColor: color={color} removed={removed}");
        return removed;
    }
}