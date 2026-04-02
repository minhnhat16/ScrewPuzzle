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
        index++;
        if (index >= list.Count) index = 0;
        
        T trans = list[index];
        // Nếu object đang được sử dụng, tiến hành mở rộng (Expand) pool thay vì cướp object
        if (trans.gameObject.activeSelf)
        {
            return ExpandPool();
        }

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
            // Tránh đệ quy vô hạn: thay vì gọi lại SpawnNonGravityNext(), sinh mới luôn.
            return ExpandPool();
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
        this.index++;
        if (this.index >= list.Count) this.index = 0;
        T trans = list[this.index];
        
        if (trans.gameObject.activeSelf)
        {
            return ExpandPool();
        }

        trans.gameObject.SetActive(true);
        ActiveList.Add(trans);
        return trans;
    }
    
    public T SpawnGravity()
    {
        index++;
        if (index >= list.Count) index = 0;
        T trans = list[index];
        
        if (trans.gameObject.activeSelf)
        {
            trans = ExpandPool();
        }
        else 
        {
            trans.gameObject.SetActive(true);
            ActiveList.Add(trans);
        }
        
        trans.GetComponent<Rigidbody2D>().gravityScale = 1;
        return trans;
    }

    /// <summary>
    /// Hàm sinh bổ sung thêm clone nếu số lượng trong pool không đủ đáp ứng.
    /// </summary>
    private T ExpandPool()
    {
        T newTrans = Object.Instantiate(prefab, parent);
        newTrans.gameObject.SetActive(true);
        list.Add(newTrans);
        ActiveList.Add(newTrans);
        total++;
        index = list.Count - 1; // Cập nhật vị trí index mới nhất
        return newTrans;
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
        //if (trans is IResetable resetable)
        //    resetable.OnReset();
        DeSpawnNonGravity(trans);
    }
}

