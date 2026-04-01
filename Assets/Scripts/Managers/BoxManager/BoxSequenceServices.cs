using Enums;
using Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoxSequenceService : IBoxSequenceService
{
    private List<Box> _queue = new();

    public List<Box> Queue { get => _queue; set => _queue = value; }

    public void Load(IEnumerable<Box> boxes)
    {
        Queue = boxes?.ToList() ?? new List<Box>();

        //Debug.Log("[BoxSequenceService] Loaded boxes: " +
                 // $"[{string.Join(", ", Queue.Select(b => b == null ? "null" : b.Color.ToString()))}]");
    }

    public Box GetNext()
    {
        if (Queue.Count == 0) return null;
        var box = Queue[0];
        Queue.RemoveAt(0);

        string colorRemains = string.Join(", ", Queue.Select(b => b == null ? "null" : b.Color.ToString()));
        //Debug.Log($"[BoxSequenceService] GetNext — dequeued color={box.Color}, còn lại {Queue.Count} box: [{colorRemains}]");
        return box;
    }

    public bool HasNext() => Queue.Count > 0;

    public Box TryDequeueMatching(Func<Box, bool> predicate)
    {
        if (predicate == null) return null;

        if (Queue.Count == 0)
        {
            //Debug.Log("[BoxSequenceService] TryDequeueMatching — queue rỗng.");
            return null;
        }

        var match = Queue.FirstOrDefault(predicate);
        if (match == null) return null;

        Queue.Remove(match);
        //Debug.Log($"[BoxSequenceService] TryDequeueMatching — matched color={match.Color}, còn lại {Queue.Count} box.");
        return match;
    }

    public Dictionary<ColorEnum, int> GetColorCounts()
    {
        var result = new Dictionary<ColorEnum, int>();
        foreach (var box in Queue)
        {
            if (box == null) continue;
            if (!result.ContainsKey(box.Color))
                result[box.Color] = 0;
            result[box.Color]++;
        }
        string colorCountsStr = string.Join(", ", result.Select(kv => $"{kv.Key}: {kv.Value}"));
        //Debug.Log($"[BoxSequenceService] GetColorCounts: {colorCountsStr}");
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
        for (int i = Queue.Count - 1; i >= 0 && removed < count; i--)
        {
            if (Queue[i] != null && Queue[i].Color == color)
            {
                var box = Queue[i];
                Queue.RemoveAt(i);
                box.gameObject.SetActive(false); // return về pool state
                removed++;
            }
        }
        //Debug.Log($"[BoxSequenceService] RemoveByColor: color={color} removed={removed}");
        return removed;
    }

    public List<Box> GetAllBox()
    {
        return Queue;
    }   

    public void ReturnToFront(Box smart)
    {
        if (smart == null) return;
        Queue.Insert(0, smart);
    }
}