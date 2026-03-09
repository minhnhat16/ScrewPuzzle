using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Repository chịu trách nhiệm load tất cả Level ScriptableObjects từ Addressables.
/// LevelManager không biết data đến từ đâu — chỉ hỏi repository.
///
/// Cách dùng:
///   var repo = new LevelDataRepository();
///   StartCoroutine(repo.LoadAll(() => Debug.Log("Done")));
///   repo.Get(1)       → Level ScriptableObject by ID
///   repo.LevelList    → List sorted by levelId
/// </summary>
public class LevelDataRepository
{
    // Key = levelId.ToString() — khớp với ResolveLevelDataStep
    public Dictionary<string, Level.Level> Levels { get; private set; } = new();

    // Sorted list dùng cho UI (LevelView)
    public List<Level.Level> LevelList { get; private set; } = new();

    public bool IsLoaded { get; private set; }

    // ─── Load ──────────────────────────────────────────────────────

    /// <summary>
    /// Load tất cả Level assets từ Addressables (or ResourceManager cache).
    /// Gọi StartCoroutine(repo.LoadAll(callback)) từ MonoBehaviour.
    /// </summary>
    public IEnumerator LoadAll(Action onComplete = null)
    {
        IsLoaded = false;
        Levels.Clear();
        LevelList.Clear();

        // Await ResourceManager để lấy tất cả cached assets
        var assetsTask = ResourceManager.ins.GetAllCachedAssets();
        yield return assetsTask;

        // Filter to only Level.Level objects
        var allLevels = new List<Level.Level>();
        if (assetsTask != null)
        {
            foreach (var kvp in assetsTask)
            {
                if (kvp.Value is Level.Level level && level != null)
                {
                    allLevels.Add(level);
                }
            }
        }

        if (allLevels.Count == 0)
        {
            Debug.LogWarning("[LevelDataRepository] No levels found in cached assets. " +
                "Ensure levels are added to Addressables and ResourceManager has preloaded them.");
        }
        else
        {
            foreach (var level in allLevels)
            {
                string key = level.levelId.ToString();

                if (Levels.ContainsKey(key))
                {
                    Debug.LogWarning($"[LevelDataRepository] Duplicate levelId: {key}. Skipping.");
                    continue;
                }

                Levels[key] = level;
                //Debug.Log($"[LevelDataRepository] Loaded Level ID {key}: {level.name}");
            }

            // Sort theo levelId cho UI
            LevelList = Levels.Values
                .OrderBy(l => l.levelId)
                .ToList();

            Debug.Log($"[LevelDataRepository] Loaded {LevelList.Count} levels total.");
        }

        yield return null; // Nhường frame

        IsLoaded = true;
        onComplete?.Invoke();
    }

    // ─── Query ─────────────────────────────────────────────────────

    /// <summary>Lấy Level theo ID. Trả về null nếu không tìm thấy.</summary>
    public Level.Level Get(int levelId) =>
        Levels.TryGetValue(levelId.ToString(), out var level) ? level : null;

    /// <summary>Tổng số level đã load.</summary>
    public int Count => LevelList.Count;

    /// <summary>ID của level tiếp theo sau levelId hiện tại.</summary>
    public int GetNextLevelId(int currentId)
    {
        var next = LevelList.FirstOrDefault(l => l.levelId > currentId);
        return next != null ? next.levelId : currentId;
    }
}