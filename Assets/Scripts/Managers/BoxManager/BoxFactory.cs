using ConfigFile;
using Ingame;
using Ingame.Pools;
using System;
using System.Collections.Generic;

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
            //box.SetAct(false);

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
        switch (config.NumberOfScrewHoles)
        {
            case 3:
                return BoxPool.Instance.pool.SpawnNonGravity();
            default:
                throw new Exception("Unsupported box size");
        }
    }
}