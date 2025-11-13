using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono <T>: MonoBehaviour where T: MonoBehaviour
{
    static T instance_;
    public static T ins
    {
        get
        {
            if(instance_==null)
            {
                instance_ = GameObject.FindFirstObjectByType<T>();
                if(instance_ == null)
                {
                    GameObject gameobject_ = new GameObject();
                    gameobject_.AddComponent<T>();
                    gameobject_.name = typeof(T).ToString();
                    instance_ = gameobject_.GetComponent<T>();
                }
            }
            return instance_;
        }
    }

   public virtual void Awake()
    {
        instance_ = gameObject.GetComponent<T>();
        OnAwake();
    }

    public void Empty()
    {

    }

    public virtual void OnAwake()
    {

    }
    void Reset()
    {
        gameObject.name = typeof(T).Name.ToString();
    }

}
