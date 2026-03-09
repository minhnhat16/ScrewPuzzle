using Managers;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Encapsulates the flow: Load InGame scene → Initialize bootstrapper → Load level → Start gameplay.
/// Decouples UI from game initialization logic (Single Responsibility).
/// Testable and reusable across MainScreenView, LevelSelectView, etc.
///
/// IMPORTANT: LevelManager, IngameController, ScrewGameBootstrapper all live in InGame scene.
/// They are resolved LAZILY inside the scene-loaded callback — NOT at construction time.
/// </summary>
public interface ILevelStartService
{
    void StartLevel(int levelId, Action onLevelStarted = null, Action<string> onError = null);
}

public class LevelStartService : ILevelStartService
{
    private readonly LoadSceneManager _sceneManager;
    private readonly string _sceneName;

    public LevelStartService(
        LoadSceneManager sceneManager,
        string sceneName = "InGame")
    {
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        _sceneName = sceneName;
    }

    public void StartLevel(int levelId, Action onLevelStarted = null, Action<string> onError = null)
    {
        if (levelId < 0)
        {
            onError?.Invoke($"[LevelStartService] Invalid level ID: {levelId}");
            return;
        }

        Debug.Log($"[LevelStartService] Preparing level {levelId}...");

        // ── Đăng ký preload task vào TaskManager TRƯỚC KHI load scene ──
        // TaskManager.RunTasks() chạy trong RunFullLoadProcess, trước LoadSceneProgress
        // → level data + spawn objects hoàn thành trong loading screen
        TaskManager.ins.AddTask(() => PreloadLevelTask(levelId, onLevelStarted, onError));

        // Sau khi task xong → load scene (scene đã có data sẵn)
        _sceneManager.LoadSceneByName(_sceneName, () =>
        {
            // Scene loaded — chỉ cần switch view và start gameplay
            // KHÔNG load level ở đây nữa vì đã preload xong trong task
            OnSceneLoaded(onError);
        });
    }

    /// <summary>
    /// Coroutine task chạy trong TaskManager (loading screen):
    /// 1. Chờ scene load xong (bootstrapper + manager available)
    /// 2. Wire dependencies
    /// 3. LoadLevel với progress update cho loading bar
    /// </summary>
    private IEnumerator PreloadLevelTask(int levelId, Action onLevelStarted, Action<string> onError)
    {
        // Chờ scene InGame load xong thì mới có các singleton
        // LoadSceneProgress chạy song song — đợi cho đến khi scene active
        yield return new WaitUntil(() =>
            ScrewGameBootstrapper.ins != null &&
            LevelManager.ins != null &&
            IngameController.ins != null);

        TaskManager.ins.SetCurrentTaskProgress(0.1f);

        var bootstrapper = ScrewGameBootstrapper.ins;
        var levelManager = LevelManager.ins;
        var ingameCtrl = IngameController.ins;

        // Wire dependencies
        bootstrapper.InitializeForLevel();
        Debug.Log("[LevelStartService] Bootstrapper initialized.");
        TaskManager.ins.SetCurrentTaskProgress(0.3f);

        // Load level data + spawn (coroutine với progress)
        bool levelDone = false;
        levelManager.LoadLevel(levelId, () =>
        {
            levelDone = true;
        });

        // Chờ level load xong, update progress
        float timeout = 30f;
        float elapsed = 0f;
        while (!levelDone)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                onError?.Invoke($"[LevelStartService] Timeout loading level {levelId}");
                yield break;
            }

            // Progress 0.3 → 0.9 trong lúc level load
            float t = Mathf.Clamp01(elapsed / timeout);
            TaskManager.ins.SetCurrentTaskProgress(0.3f + t * 0.6f);
            yield return null;
        }

        TaskManager.ins.SetCurrentTaskProgress(1f);
        Debug.Log($"[LevelStartService] Level {levelId} preloaded ✅");

        // Lưu callback để OnSceneLoaded gọi sau
        _onLevelStarted = () =>
        {
            ingameCtrl.StartLevel();
            onLevelStarted?.Invoke();
        };
    }

    // Cache callback giữa PreloadLevelTask và OnSceneLoaded
    private Action _onLevelStarted;

    private void OnSceneLoaded(Action<string> onError)
    {
        if (_onLevelStarted == null)
        {
            onError?.Invoke("[LevelStartService] _onLevelStarted null — preload chưa xong?");
            return;
        }

        // Tất cả đã preload xong → switch view và start ngay
        ViewManager.Instance.SwitchView(ViewIndex.GameView, null, () =>
        {
            _onLevelStarted?.Invoke();
            _onLevelStarted = null;
            Debug.Log("[LevelStartService] GameView shown, gameplay started ✅");
        });
    }
}