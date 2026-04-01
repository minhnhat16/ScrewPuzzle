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
            new InitSoundTask(),
            //new InitAdsBanner()
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

        // ── New player vs Returning player flow ──
        bool isNewPlayer = DataAPIController.instance.IsNewPlayer();

        if (isNewPlayer)
        {
            // New player: Load game scene → start tutorial → gameplay
            Debug.Log("[BOOT] New player detected → Loading InGame scene with tutorial");
            LoadSceneManager.ins.LoadSceneByName("BootScene", () =>
            {
                Debug.Log("[BOOT] InGame scene loaded → Starting tutorial");
                // SwitchViewForNewPlayer sẽ trigger LevelStartService.StartLevel(1)
                // → load level → start tutorial → gameplay
                ViewManager.Instance.SwitchViewForNewPlayer(isNewPlayer: true);
            });
        }
        else
        {
            // Returning player: Load game scene → show MainScreenView
            Debug.Log("[BOOT] Returning player detected → Loading game scene with MainScreenView");
            LoadSceneManager.ins.LoadSceneByName("BootScene", () =>
            {
                Debug.Log("[BOOT] Game scene loaded → Switching to MainScreenView");

                MainScreenViewParam param = new()
                {
                    level = DataAPIController.instance.GetPlayerLevel(),
                    totalGold = WalletManager.ins.Get(Currency.Gold),
                    ticket = WalletManager.ins.Get(Currency.Ticket),
                };

                ViewManager.Instance.SwitchView(ViewIndex.MainScreenView, param, () =>
                {
                    Debug.Log("[BOOT] MainScreenView shown");
                    DayTimeController.instance.CheckNewDay();

                    var isAppOpenEnabled = ZenSDK.instance.IsAppOpenReady();
                    Debug.Log("IsAppOpenReady: " + isAppOpenEnabled);
                  
                });
            });
        }
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