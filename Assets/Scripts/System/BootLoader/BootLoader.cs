using Managers;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootLoader : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private UIRootControlScale uiRootControl;

    private void Awake()
    {
        ScreenSetup();
    }

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        // Prepare ordered boot tasks
        var bootTasks = new List<IBootTask>
        {
            new LoadRemoteAssetsTask(),
            new InitConfigTask(),
            new InitDataTask(),
            new InitMissionTask(),
            new SetupUITask(),
            new InitSoundTask()
        };

        // Run tasks sequentially
        StartCoroutine(RunBootTasks(bootTasks));
    }

    private IEnumerator RunBootTasks(List<IBootTask> tasks)
    {
        if (tasks == null || tasks.Count == 0)
        {
            Debug.LogWarning("[BOOT] No tasks to execute");
            yield break;
        }

        int completedCount = 0;
        float bootStartTime = Time.realtimeSinceStartup;

        foreach (var task in tasks)
        {
            if (task == null)
            {
                Debug.LogWarning("[BOOT] Null task encountered, skipping");
                continue;
            }

            float taskStartTime = Time.realtimeSinceStartup;
            Debug.Log($"[BOOT] Starting task ({completedCount + 1}/{tasks.Count}): {task.Name}");

            // Use SafeExecute to catch exceptions thrown inside the IEnumerator
            bool success = true;
            yield return StartCoroutine(SafeExecute(task, (ex) =>
            {
                Debug.LogError($"[BOOT] Task {task.Name} failed: {ex.Message}\n{ex.StackTrace}");
                success = false;
            }));

            if (success)
            {
                float taskDuration = Time.realtimeSinceStartup - taskStartTime;
                Debug.Log($"[BOOT] ✓ Completed task: {task.Name} ({taskDuration:F2}s)");
                completedCount++;
            }
            else
            {
                // current policy: continue to next task. Could implement retry/abort here.
            }
        }

        float totalBootDuration = Time.realtimeSinceStartup - bootStartTime;
        Debug.Log($"[BOOT] Boot complete: {completedCount}/{tasks.Count} succeeded in {totalBootDuration:F2}s");

        yield return new WaitForEndOfFrame();

        bool isNew = DataAPIController.instance.IsNewPlayer();
        string sceneName = isNew ? "InGame" : "Buffer";
        LoadSceneManager.ins.LoadSceneByName(sceneName, () =>
        {
            Debug.Log("BootLoader: Load Scene Done");
            if (isNew)
            {
                StartCoroutine(LoadLevelFromService(isNew));
            }
            ViewManager.Instance.SwitchViewForNewPlayer(isNew);

        });
    }

    /// <summary>
    /// Safely execute the task's IEnumerator and invoke onError if any exception occurs during iteration.
    /// This captures exceptions thrown inside coroutines (across yields).
    /// </summary>
    private IEnumerator SafeExecute(IBootTask task, System.Action<Exception> onError)
    {
        var enumerator = task.Execute();
        if (enumerator == null) yield break;

        try
        {
            while (true)
            {
                object current = null;
                try
                {
                    if (!enumerator.MoveNext())
                        break;
                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                    yield break;
                }
                yield return current;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    /// <summary>
    /// Find LevelLoaderService in the loaded scene and run its load routine.
    /// Adjust logic that chooses the target level id if you store last-played level in player data.
    /// </summary>
    private IEnumerator LoadLevelFromService(bool isNewPlayer)
    {
        // Choose level id: new players typically start at 0. Replace the fallback for returning players
        // with your real "last played level" lookup (DataAPIController, player profile, etc.)
        int levelId = isNewPlayer ? 0 : 1;

        // Wait a frame to ensure scene objects have been initialized
        yield return null;

        var loader = FindAnyObjectByType<LevelLoaderService>();
        if (loader == null)
        {
            Debug.LogWarning("[BOOT] LevelLoaderService not found in scene. Skipping level load.");
            yield break;
        }

        Debug.Log($"[BOOT] Loading level {levelId} via LevelLoaderService");
        // This yields until the service completes the load routine
        yield return StartCoroutine(loader.LoadLevelRoutine(levelId, () => Debug.Log("[BOOT] LevelLoaderService finished load")));
        Debug.Log("[BOOT] Level load from service completed");
    }

    private void ScreenSetup()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
    }

    private void OnApplicationPause(bool pause)
    {
        Time.timeScale = pause ? 0 : 1;
    }
    private void OnApplicationFocus(bool focus)
    {
        Time.timeScale = 1;
    }
}

