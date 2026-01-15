using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager : SingletonMono<ResourceManager>
{
    // Cache for loaded assets
    private Dictionary<string, Object> assetCache = new Dictionary<string, Object>();
    private readonly Dictionary<string, AsyncOperationHandle> _handleCache = new();
    public IEnumerator Init(List<string> labels, Action callback = null)
    {
        HashSet<string> allKeys = new HashSet<string>();

        // 1. Lặp qua từng label để lấy key
        foreach (var label in labels)
        {
            Task<List<string>> keysTask = TaskExtensions.GetKeysFromLabel(label);
            yield return new WaitUntil(() => keysTask.IsCompleted);

            if (keysTask.Exception != null)
            {
                Debug.LogError("Failed to get keys for label: " + label + " => " + keysTask.Exception);
                yield break;
            }

            foreach (var key in keysTask.Result)
            {
                Debug.Log($"[ResourceManager] Found key '{key}' for label '{label}'");
                allKeys.Add(key); // thêm vào set để tránh trùng
            }
        }

        // 2. Load tất cả asset từ danh sách key duy nhất
        Task loadTask = LoadAssetsAsync(allKeys.ToList());
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogError("Error loading addressable assets: " + loadTask.Exception);
            yield break;
        }
        Debug.Log($"Loaded {allKeys.Count} addressable assets from {labels.Count} labels successfully.");
        callback?.Invoke();
    }

    /// <summary>
    /// Load assets by their keys and cache them
    /// </summary>
    public async Task LoadAssetsAsync(List<string> keys)
    {
        foreach (string key in keys)
        {
            if (!assetCache.ContainsKey(key))
            {
                if (key.Contains("/HINH1/"))
                {
                    Debug.Log($"[ResourceManager] Loading asset with key '{key}'");
                    AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(key);
                    await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        assetCache[key] = handle.Result;
                    }
                }
                else
                {
                    AsyncOperationHandle<Object> handle = Addressables.LoadAssetAsync<Object>(key);
                    await handle.Task;

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        assetCache[key] = handle.Result;
                    }
                }

            }
        }
    }
    public IEnumerator YieldUntilLoaded<T>(string key, System.Action<T> onDone) where T : UnityEngine.Object
    {
        var task = ResourceManager.ins.GetAssetAsync<T>(key); // Task<T>
        while (!task.IsCompleted) yield return null;

        if (task.Result == null)
            Debug.LogError($"[Asset] '{key}' load FAILED");
        else
            onDone(task.Result);
    }
    /// <summary>
    /// Get a loaded asset by key (cast to specific type)
    /// </summary>
    public T GetAsset<T>(string key) where T : Object
    {
        if (assetCache.TryGetValue(key, out Object obj) && obj is T typedAsset)
        {
            //Debug.Log($"[AssetManager] Retrieved asset '{key}' of type: {obj.GetType()}");
            return typedAsset;
        }

        //Debug.LogError($"[AssetManager] Asset with key '{key}' not found or wrong type. Did you forget to load it?");
        return null;
    }

    /// Load asset async (và cache). Trả về null nếu fail/ bị cancel.
    public async Task<T> GetAssetAsync<T>(string key, CancellationToken ct = default) where T : Object
    {
        // Cache hit
        if (assetCache.TryGetValue(key, out var cached))
            return cached as T;

        // Load
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        _handleCache[key] = handle;

        try
        {
            // Chờ mà vẫn tôn trọng cancel
            while (!handle.IsDone)
            {
                if (ct.IsCancellationRequested)
                {
                    Addressables.Release(handle);
                    _handleCache.Remove(key);
                    return null;
                }
                await Task.Yield();
            }

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                assetCache[key] = handle.Result;
                return handle.Result;
            }

            Debug.LogError($"[AssetManager] Load failed: '{key}' ({handle.Status})");
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }

        // Fail-safe
        if (_handleCache.TryGetValue(key, out var h)) { Addressables.Release(h); }
        _handleCache.Remove(key);
        return null;
    }

    public Sprite GetSpriteFromResources(string path)
    {
        // The path should be relative to the Resources folder, without the .png/.jpg extension
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
        }
        return sprite;
    }
    /// <summary>
    /// Optional: unload all cached assets
    /// </summary>
    public void ClearCache()
    {
        foreach (var item in assetCache)
        {
            Addressables.Release(item.Value);
        }
        assetCache.Clear();
    }

    public async Task<Level.Level> LoadLevelOnceAsync(string levelKey)
    {
        if (assetCache.TryGetValue(levelKey, out var cached))
            return cached as Level.Level;

        AsyncOperationHandle<Level.Level> handle =
            Addressables.LoadAssetAsync<Level.Level>(levelKey);

        _handleCache[levelKey] = handle;
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            assetCache[levelKey] = handle.Result;
            return handle.Result;
        }

        Addressables.Release(handle);
        _handleCache.Remove(levelKey);
        return null;
    }

    public Dictionary<string, Sprite> GetAllSprites()
    {
        Dictionary<string, Sprite> result = new();

        var snapshot = assetCache.ToList();

        foreach (var pair in snapshot)
        {
            string key = pair.Key;
            var value = pair.Value;

            if (value is Sprite sprite)
            {
                result[key] = sprite;
            }
            else if (value is Texture2D tex)
            {
                Sprite sp = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                sp.name = tex.name;

                result[key] = sp;

                // ⚠️ Ghi lại assetCache thì OK vì snapshot rồi
                assetCache[key] = sp;
            }
        }
        return result;
    }


    public IReadOnlyDictionary<string, Object> GetAllCachedAssets()
    {
        return assetCache;
    }

    Dictionary<string, Sprite> spriteDict = new();
    public async Task LoadPSB(string psbKey)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(psbKey);
        await handle.Task;

        var obj = handle.Result;

        // Lấy tất cả SpriteRenderer trong prefab
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();

        foreach (var r in renderers)
        {
            string key = $"{r.sprite.name}";
            if (r.sprite != null && !spriteDict.ContainsKey(key))
            {
                spriteDict.Add(key, r.sprite);
                Debug.Log($"Added sprite: {key} + {r.sprite.name}");
            }
        }

        Debug.Log($"[LoadPSB] Loaded {spriteDict.Count} sprites from PSB, renderer count {renderers.Count()}");
    }

    //public async Task LoadPSB(string psbKey)
    //{
    //    var handle = Addressables.LoadAssetAsync<GameObject>(psbKey);
    //    await handle.Task;

    //    var obj = handle.Result;

    //    // Lấy tất cả SpriteRenderer trong prefab
    //    var renderers = obj.GetComponentsInChildren<SpriteRenderer>();

    //    foreach (var r in renderers)
    //    {
    //        string key = $"{obj.name}_{r.sprite.name}";
    //        if (r.sprite != null && !spriteDict.ContainsKey(r.sprite.name))
    //        {
    //            spriteDict.Add(key, r.sprite);
    //            Debug.Log($"Added sprite: {key}");
    //        }
    //    }

    //    Debug.Log($"[LoadPSB] Loaded {spriteDict.Count} sprites from PSB");
    //}


    public Sprite GetSprite(string layerName)
    {
        if (spriteDict.TryGetValue(layerName, out var s))
            return s;

        Debug.LogError($"Sprite not found: {layerName}");
        return null;
    }
}
