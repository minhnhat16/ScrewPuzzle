using Managers;
using System;
using System.Collections;
using System.DataBase;
using UnityEngine;

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
        if (levelId <= 0)
        {
            onError?.Invoke($"[LevelStartService] Invalid level ID: {levelId}");
            return;
        }

        Debug.Log($"[LevelStartService] Preparing level {levelId}...");

        _sceneManager.LoadSceneByName(_sceneName, () =>
        {
            TaskManager.ins.AddTask(() => InitAfterSceneLoaded(levelId, onLevelStarted, onError));
            TaskManager.ins.StartCoroutine(RunDeferredInit(onError));
        });
    }

    private IEnumerator RunDeferredInit(Action<string> onError)
    {
        // Chạy tất cả task đã add (chỉ có 1 task: InitAfterSceneLoaded)
        yield return TaskManager.ins.RunTasks();
    }

    private IEnumerator InitAfterSceneLoaded(int levelId, Action onLevelStarted, Action<string> onError)
    {
        // Đợi 1 frame để đảm bảo tất cả Awake() + Start() của scene đã chạy xong
        yield return null;

        var bootstrapper = ScrewGameBootstrapper.ins;
        var levelManager = LevelManager.ins;
        var ingameCtrl = IngameController.ins;

        if (bootstrapper == null || levelManager == null || ingameCtrl == null)
        {
            onError?.Invoke("[LevelStartService] Singleton(s) null sau khi scene load — kiểm tra Awake order.");
            yield break;
        }

        // 1. Wire dependencies trước khi LoadLevel
        bootstrapper.InitializeForLevel();
        Debug.Log("[LevelStartService] Bootstrapper initialized.");

        TaskManager.ins.SetCurrentTaskProgress(0.3f);

        // 2. Load level — chờ pipeline hoàn thành
        bool levelDone = false;
        levelManager.LoadLevel(levelId, () => levelDone = true);

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
            TaskManager.ins.SetCurrentTaskProgress(0.3f + Mathf.Clamp01(elapsed / timeout) * 0.6f);
            yield return null;
        }

        TaskManager.ins.SetCurrentTaskProgress(1f);
        Debug.Log($"[LevelStartService] Level {levelId} loaded ✅");

        // 3. Check new player → activate tutorial
        bool isNewPlayer = DataAPIController.instance.IsNewPlayer();

        // 4. Switch view rồi start gameplay
        ViewManager.Instance.SwitchView(ViewIndex.GameView, null, () =>
        {
            ingameCtrl.StartLevel();

            // ── Tutorial: chỉ activate cho new player ──
            // StartTutorial() sau StartLevel() để game đã ở Playing state
            // → TutorialManager highlight target, block input đúng cách
            if (isNewPlayer && TutorialManager.ins != null)
            {
                TutorialManager.ins.StartTutorial();
                Debug.Log("[LevelStartService] Tutorial started for new player.");
            }

            onLevelStarted?.Invoke();
            Debug.Log("[LevelStartService] GameView shown, gameplay started ✅");
        });
    }
}