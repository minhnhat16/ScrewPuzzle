using System.Collections.Generic;
using UnityEngine;

public class ByUIPoolUI<T> where T : MonoBehaviour
{
    private readonly T prefab;
    private readonly Transform parent;

    private readonly Queue<T> inactive = new();
    private readonly List<T> active = new();

    public List<T> Active => active;

    public ByUIPoolUI(T prefab, int preload, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < preload; i++)
        {
            inactive.Enqueue(CreateNew());
        }
    }

    private T CreateNew()
    {
        var obj = Object.Instantiate(prefab);

        // UI-safe parenting
        var rect = obj.transform as RectTransform;
        rect.SetParent(parent, false);       // <--- quan trọng
        rect.localScale = Vector3.one;

        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get()
    {
        T obj = inactive.Count > 0 ? inactive.Dequeue() : CreateNew();

        var rect = obj.transform as RectTransform;
        rect.SetParent(parent, false);       // <--- giữ anchor/layout

        obj.gameObject.SetActive(true);
        Active.Add(obj);

        return obj;
    }

    public void Release(T obj)
    {
        obj.gameObject.SetActive(false);
        Active.Remove(obj);
        inactive.Enqueue(obj);
    }

    public void ReleaseAll()
    {
        foreach (var obj in Active)
        {
            obj.gameObject.SetActive(false);
            inactive.Enqueue(obj);
        }
        Active.Clear();
    }
}
