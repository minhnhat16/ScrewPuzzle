using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
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

            //Debug.Log($"[ResourceManager] Getting keys for label: '{label}'");  
            Task<List<string>> keysTask = TaskExtensions.GetKeysFromLabel(label);
            yield return new WaitUntil(() => keysTask.IsCompleted);

            if (keysTask.Exception != null)
            {
                //Debug.LogError("Failed to get keys for label: " + label + " => " + keysTask.Exception);
                yield break;
            }

            foreach (var key in keysTask.Result)
            {
                //Debug.Log($"[ResourceManager] Found key '{key}' for label '{label}'");
                allKeys.Add(key); // thêm vào set để tránh trùng
            }
        }

        // 2. Load tất cả asset từ danh sách key duy nhất
        Task loadTask = LoadAssetsAsync(allKeys.ToList());
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            //Debug.LogError("Error loading addressable assets: " + loadTask.Exception);
            yield break;
        }
        //Debug.Log($"Loaded {allKeys.Count} addressable assets from {labels.Count} labels successfully.");
        callback?.Invoke();
    }

    /// <summary>
    /// Load assets by their keys and cache them - with parallel loading for better performance
    /// </summary>
    public async Task LoadAssetsAsync(List<string> keys)
    {
        if (keys == null || keys.Count == 0)
            return;

        // Split keys into batches and load in parallel for better performance
        // This is more efficient than loading one-by-one
        const int batchSize = 5; // Load up to 5 assets in parallel
        var tasks = new List<Task>();

        for (int i = 0; i < keys.Count; i += batchSize)
        {
            var batch = keys.GetRange(i, Math.Min(batchSize, keys.Count - i));
            tasks.Add(LoadAssetBatchAsync(batch));
        }

        try
        {
            await Task.WhenAll(tasks);
            Debug.Log($"[ResourceManager] Loaded {keys.Count} assets in parallel batches");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ResourceManager] Error loading asset batch: {ex.Message}");
        }
    }

    private async Task LoadAssetBatchAsync(List<string> batch)
    {
        var batchTasks = new List<Task>();

        foreach (string key in batch)
        {
            if (assetCache.ContainsKey(key))
                continue;

            batchTasks.Add(LoadSingleAssetAsync(key));
        }

        if (batchTasks.Count > 0)
            await Task.WhenAll(batchTasks);
    }

    private async Task LoadSingleAssetAsync(string key)
    {
        try
        {
            if (key.Contains("/HINH1/"))
            {
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
        catch (Exception ex)
        {
            Debug.LogError($"[ResourceManager] Failed to load asset '{key}': {ex.Message}");
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
    public Sprite GetSpriteByName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        // 1. PSB sprites (spriteDict key = sprite.name trực tiếp)
        if (spriteDict.TryGetValue(spriteName, out var fromPsb))
            return fromPsb;

        // 2. Addressable assetCache (key = full address, phải duyệt qua value.name)
        foreach (var pair in assetCache)
        {
            if (pair.Value is Sprite s &&
                string.Equals(s.name, spriteName, StringComparison.OrdinalIgnoreCase))
                return s;
        }

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

    private readonly Dictionary<string, Sprite> spriteDict = new();

    // Track which PSB keys have been loaded to avoid double-loading
    private readonly HashSet<string> _loadedPsbKeys = new();

    /// <summary>
    /// Returns true if the PSB for this key has already been loaded.
    /// </summary>
    public bool IsPSBLoaded(string psbKey) => _loadedPsbKeys.Contains(psbKey);

    /// <summary>
    /// Load tất cả PSB assets theo label từ Addressables,
    /// extract sprites từ SpriteRenderer (nếu là GameObject/Prefab).
    /// 
    /// Trên Android build, Addressables có thể trả về type khác so với Editor.
    /// Dùng LoadAssetsAsync&lt;Object&gt; thay vì &lt;GameObject&gt; để tương thích cả 2.
    /// </summary>
    public async Task LoadPSB(string psbKey)
    {
        if (string.IsNullOrEmpty(psbKey))
        {
            Debug.LogError("[ResourceManager] LoadPSB: psbKey is null or empty.");
            return;
        }

        if (_loadedPsbKeys.Contains(psbKey))
        {
            Debug.Log($"[ResourceManager] PSB '{psbKey}' already loaded, skipping.");
            return;
        }

        // ── Step 1: Kiểm tra label có tồn tại trong catalog không ──
        AsyncOperationHandle<IList<IResourceLocation>> locHandle =
            Addressables.LoadResourceLocationsAsync(psbKey, typeof(Object));

        await locHandle.Task;

        if (locHandle.Status != AsyncOperationStatus.Succeeded
            || locHandle.Result == null
            || locHandle.Result.Count == 0)
        {
            Debug.LogWarning($"[ResourceManager] LoadPSB: label '{psbKey}' has no locations in catalog. " +
                             "Check Addressables labels and rebuild (Build > New Build > Default Build Script).");
            Addressables.Release(locHandle);
            _loadedPsbKeys.Add(psbKey); // đánh dấu để không retry mãi
            return;
        }

        int locationCount = locHandle.Result.Count;
        Addressables.Release(locHandle);

        // ── Step 2: Load bằng Object type — tương thích cả Editor lẫn Android build ──
        AsyncOperationHandle<IList<Object>> handle =
            Addressables.LoadAssetsAsync<Object>(psbKey, null);

        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
        {
            Debug.LogError($"[ResourceManager] LoadPSB failed for label '{psbKey}': {handle.Status}. " +
                           $"Catalog had {locationCount} locations but load returned nothing. " +
                           $"Exception: {handle.OperationException?.Message}");
            Addressables.Release(handle);
            return;
        }

        // ── Step 3: Extract sprites từ kết quả ──
        int added = 0;
        foreach (var asset in handle.Result)
        {
            if (asset == null) continue;

            // Case 1: Asset là GameObject (PSB prefab) — extract từ SpriteRenderer
            if (asset is GameObject go)
            {
                var renderers = go.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
                foreach (var r in renderers)
                {
                    if (r.sprite == null) continue;
                    string spriteKey = r.sprite.name;
                    if (!spriteDict.ContainsKey(spriteKey))
                    {
                        spriteDict[spriteKey] = r.sprite;
                        added++;
                    }
                }
            }
            // Case 2: Asset là Sprite trực tiếp (sub-asset từ PSB SpriteAtlas)
            else if (asset is Sprite sprite)
            {
                string spriteKey = sprite.name;
                if (!spriteDict.ContainsKey(spriteKey))
                {
                    spriteDict[spriteKey] = sprite;
                    added++;
                }
            }
            // Case 3: Asset là Texture2D — convert sang Sprite
            else if (asset is Texture2D tex)
            {
                var sp = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                sp.name = tex.name;
                if (!spriteDict.ContainsKey(sp.name))
                {
                    spriteDict[sp.name] = sp;
                    added++;
                }
            }
        }

        // Cache handle để có thể release sau khi unload
        _handleCache[psbKey] = handle;
        _loadedPsbKeys.Add(psbKey);

        Debug.Log($"[ResourceManager] LoadPSB label='{psbKey}': " +
                  $"{handle.Result.Count} assets loaded, +{added} sprites, total={spriteDict.Count}");
    }

    /// <summary>
    /// Unload PSB và xóa sprites của key đó khỏi spriteDict.
    /// Gọi từ PsbSlidingWindowLoader khi level ra ngoài window.
    /// </summary>
    public void UnloadPSB(string psbKey)
    {
        if (string.IsNullOrEmpty(psbKey) || !_loadedPsbKeys.Contains(psbKey))
            return;

        // Release Addressable handle nếu còn giữ
        if (_handleCache.TryGetValue(psbKey, out var handle))
        {
            Addressables.Release(handle);
            _handleCache.Remove(psbKey);
        }

        _loadedPsbKeys.Remove(psbKey);
        Debug.Log($"[ResourceManager] UnloadPSB '{psbKey}'. " +
                  $"Note: sprites already in spriteDict remain until next re-index.");
    }

    /// <summary>
    /// Load nhiều PSB cùng lúc (parallel).
    /// Gọi từ BootTask hoặc trước khi load level.
    /// </summary>
    public async Task LoadPSBBatch(string psbKeys)
    {
        var tasks = new List<Task>();
        if (!_loadedPsbKeys.Contains(psbKeys))
            tasks.Add(LoadPSB(psbKeys));
        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Coroutine wrapper for LoadPSBBatch — use from MonoBehaviour.
    /// </summary>
    public IEnumerator LoadPSBBatchCoroutine(string psbKeys, Action onComplete = null)
    {
        var task = LoadPSBBatch(psbKeys);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
            Debug.LogError($"[ResourceManager] LoadPSBBatch error: {task.Exception}");
        else
            Debug.Log($"[ResourceManager] LoadPSBBatch complete — {psbKeys} PSBs processed.");

        onComplete?.Invoke();
    }

    public Sprite GetSprite(string layerName)
    {
        if (string.IsNullOrEmpty(layerName)) return null;

        if (spriteDict.TryGetValue(layerName, out var s))
            return s;

        Debug.LogWarning($"[ResourceManager] Sprite not found: '{layerName}'. Make sure LoadPSB was called before GetSprite.");
        return null;
    }

    /// <summary>
    /// Trả về toàn bộ PSB sprites (extract từ SpriteRenderer trong prefab).
    /// Dùng bởi SpriteLibControl.AppendPsbSprites() để index sau khi LoadPSB.
    /// </summary>
    public IReadOnlyDictionary<string, Sprite> GetAllPsbSprites()
    {
        return spriteDict;
    }
}