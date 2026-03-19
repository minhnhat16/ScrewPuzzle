using System.Collections.Generic;
using UnityEngine;

public class QuestItemFactory
{
    private readonly QuestItem prefab;
    private Transform parent;
    private readonly Queue<QuestItem> pool = new();

    public QuestItemFactory(QuestItem prefab, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
    }

    public void SetParent(Transform newParent)
    {
        parent = newParent;
    }

    public QuestItem Get()
    {
        QuestItem item;
        if (pool.Count > 0)
        {
            item = pool.Dequeue();
            item.gameObject.SetActive(true);
        }
        else
        {
            item = GameObject.Instantiate(prefab, parent);
        }
        return item;
    }

    public void Release(QuestItem item)
    {
        item.gameObject.SetActive(false);
        pool.Enqueue(item);
    }
}