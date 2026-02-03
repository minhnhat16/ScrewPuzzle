using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ConfigFileManager : MonoBehaviour
{
    public static ConfigFileManager Instance;

    private Dictionary<Type, ScriptableObject> configMap
        = new Dictionary<Type, ScriptableObject>();

    public bool IsDone { get; private set; }

    private void Awake() => Instance = this;

    public void Init(Action callback)
    {
        StartCoroutine(LoadAllConfigs_Addressable("Config",callback));
    }

    public IEnumerator LoadAllConfigs_Addressable(string label, Action callback)
    {
        IsDone = false;
        configMap.Clear();

        // 1) Init
        var init = Addressables.InitializeAsync();
        yield return init;

        // 2) Check label có location không
        var locHandle = Addressables.LoadResourceLocationsAsync(label, typeof(ScriptableObject));
        yield return locHandle;

        if (locHandle.Status != AsyncOperationStatus.Succeeded || locHandle.Result == null || locHandle.Result.Count == 0)
        {
            Debug.LogError($"[Config] Label '{label}' has no locations. " +
                           $"Did you assign label + build addressables + reinstall app?");
            Addressables.Release(locHandle);
            IsDone = true;
            callback?.Invoke();
            yield break;
        }

        Addressables.Release(locHandle);

        // 3) Load assets theo label
        var handle = Addressables.LoadAssetsAsync<ScriptableObject>(
            label,
            cfg =>
            {
                if (cfg == null) return;
                configMap[cfg.GetType()] = cfg;
                Debug.Log("Loaded Config: " + cfg.GetType().Name);
                if (cfg is MissionConfig missionCfg)
                {
                    Debug.Log($" - MissionConfig has {missionCfg.GetAllRecord().Count} levels");
                }

            });

        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[Config] Load failed: {handle.OperationException}");
        }

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
