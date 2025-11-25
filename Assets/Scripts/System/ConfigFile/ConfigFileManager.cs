using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigFileManager : MonoBehaviour
{
    public static ConfigFileManager Instance;

    private Dictionary<Type, ScriptableObject> configMap
        = new Dictionary<Type, ScriptableObject>();

    public bool IsDone { get; private set; }

    private void Awake() => Instance = this;

    public void Init(Action callback)
    {
        StartCoroutine(LoadAllConfigs(callback));
    }

    private IEnumerator LoadAllConfigs(Action callback)
    {
        IsDone = false;

        // Load tất cả ScriptableObject trong Resources/Config/*
        ScriptableObject[] configs = Resources.LoadAll<ScriptableObject>("Config");

        foreach (var cfg in configs)
        {
            configMap[cfg.GetType()] = cfg;
            Debug.Log("Loaded Config: " + cfg.GetType().Name);
        }

        // Load Factory
        var factory = Resources.Load<SoundFactory>("Factory/SoundFactory");
        configMap[typeof(SoundFactory)] = factory;

        yield return null;

        IsDone = true;
        callback?.Invoke();
    }

    // Generic getter
    public T GetConfig<T>() where T : ScriptableObject
    {
        if (configMap.TryGetValue(typeof(T), out ScriptableObject cfg))
            return (T)cfg;

        Debug.LogError("Config not found: " + typeof(T).Name);
        return null;
    }

 
}
