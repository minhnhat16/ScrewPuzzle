using ConfigFile;
using Ingame;
using Ingame.Pools;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class BoxFactory : IBoxFactory
{
    private Stack<Box> _stack = new();
    private List<Box> _boxes = new();

    public List<Box> CreateBoxes(IEnumerable<BoxConfigRecord> records)
    {
        _boxes.Clear();
        _stack.Clear();

        foreach (var config in records)
        {
            Box box = SpawnFromPool(config);

            // Reset trước khi Initialize — clear screws cũ từ level trước
            box.OnReset();

            box.Initialize(config.BoxColor, config.NumberOfScrewHoles);
            box.gameObject.SetActive(false);
            _boxes.Add(box);
            _stack.Push(box);
        }

        return _boxes;
    }

    public Box SpawnNext()
    {
        return _stack.Count > 0 ? _stack.Pop() : null;
    }

    public Box SpawnByPredicate(Func<Box, bool> predicate)
    {
        var list = new List<Box>(_stack);
        var box = list.Find((Predicate<Box>)predicate.Target);
        if (box == null) return null;

        list.Remove(box);
        _stack = new Stack<Box>(list);
        return box;
    }

    private Box SpawnFromPool(BoxConfigRecord config)
    {
        return config.NumberOfScrewHoles switch
        {
            3 => BoxPool.Instance.pool.SpawnNonGravity(),
            _ => throw new Exception("Unsupported box size"),
        };
    }
}