using Ingame;
using System.Collections.Generic;

public class BoxSequenceService : IBoxSequenceService
{
    private Queue<Box> _queue = new();

    public void Load(IEnumerable<Box> boxes)
    {
        _queue.Clear();
        foreach (var box in boxes)
            _queue.Enqueue(box);
    }

    public Box GetNext()
    {
        return _queue.Count > 0 ? _queue.Dequeue() : null;
    }

    public bool HasNext()
    {
        return _queue.Count > 0;
    }
}