using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
public class BY_Local_Pool<T> where T : MonoBehaviour
{
    public T prefab;
    public Transform parent;
    public int total;
    [NonSerialized]
    public List<T> list = new List<T>();
    private List<T> activeList = new List<T>();
    private int index = -1;

    public List<T> ActiveList { get => activeList; set => activeList = value; }

    public BY_Local_Pool(T prefab, int total, Transform parent = null)
    {
        this.parent = parent;
        this.prefab = prefab;
        this.total = total;
        index = -1;
        for (int i = 0; i < total; i++)
        {
            T trans = Object.Instantiate(prefab);
            trans.transform.SetParent(parent);
            trans.gameObject.SetActive(false);
            list.Add(trans);
        }
    }
    public T SpawnNonGravity()
    {
        //Debug.Log("SPAWN");
        index++;
        if (index >= list.Count) index = 0;
        T trans = list[index];
        trans.gameObject.SetActive(true);
        ActiveList.Add(trans);
        return trans;
    }
    public T SpawnNonGravityNext()
    {
        index++;
        if (index >= list.Count) index = 0;
        T trans = list[index];
        if (trans.gameObject.activeSelf == true)
        {
            index++;
            trans = SpawnNonGravityNext();
            ActiveList.Add(trans);
            return trans;
        }
        else
        {
            trans.gameObject.SetActive(true);
            ActiveList.Add(trans);
            return trans;
        }
    }
    public T SpawnNonGravityWithIndex(int index)
    {
        // Debug.Log("spawn brick");
        this.index++;
        if (index >= list.Count) index = 0;
        T trans = list[index];
        trans.gameObject.SetActive(true);
        ActiveList.Add(trans);
        return trans;

    }
    public T SpawnGravity()
    {
        index++;
        if (index >= list.Count) index = 0;
        T trans = list[index];
        trans.gameObject.SetActive(true);
        ActiveList.Add(trans);
        trans.GetComponent<Rigidbody2D>().gravityScale = 1;
        return trans;
    }
    public void DeSpawnNonGravity(T trans)
    {
        ActiveList.Remove(trans);
        trans.gameObject.SetActive(false);

    }
    public void DeSpawnGravity(T trans)
    {
        ActiveList.Remove(trans);
        trans.GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
        trans.GetComponent<Rigidbody2D>().gravityScale = 0;
        trans.gameObject.SetActive(false);

    }
    public void SpawnAll()
    {
        foreach (var g in list)
        {
            g?.gameObject.SetActive(true);
        }
        ActiveList.Clear();
        index++;
    }
    public void DeSpawnAll()
    {
        foreach (var g in list)
        {
            g?.gameObject.SetActive(false);
        }
        ActiveList.Clear();
        index = -1;
    }

    public void ReturnToPool(T trans)
    {
        if (trans == null) return;
        if (trans.transform.parent != parent)
        {
            trans.transform.SetParent(parent);
        }
        DeSpawnNonGravity(trans);
    }
}

