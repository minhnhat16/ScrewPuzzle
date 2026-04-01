using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager ins;
    [Range(0f, 1f)] public float sampleWait = 0.5f;
    [SerializeField] private float timeWait = 5f;
    public float progress;

    private Action callback;
    private string sceneName;

    public float TimeWait { get => timeWait; set => timeWait = value; }

    private void Awake()
    {
        ins = this;
    }

    /// <summary>
    /// Load scene với animation loading.
    /// Nếu sceneName == sceneName hiện tại, sẽ skip scene load và chỉ chạy animation + callback.
    /// </summary>
    public void LoadSceneByName(string sceneName, Action callback)
    {
        Debug.Log("Start Load Scene: " + sceneName + " current view " + ViewManager.Instance.currentView);
        StopAllCoroutines();
        this.callback = callback;
        this.sceneName = sceneName;
        
        ViewManager.Instance.SwitchView(ViewIndex.LoadingView, null, () =>
        {
            StartCoroutine(RunFullLoadProcess());
        });
    }

    IEnumerator RunFullLoadProcess()
    {
        Debug.Log("Run task start");
        yield return StartCoroutine(TaskManager.ins.RunTasks());
        Debug.Log("Run task done");
        
        // Check xem có cần load scene hay không
        if (IsSceneLoaded(sceneName))
        {
            Debug.Log($"[LoadSceneManager] Scene '{sceneName}' đã loaded rồi, skip scene load");
            yield return StartCoroutine(SimulateLoadingProgress());
        }
        else
        {
            Debug.Log($"[LoadSceneManager] Loading scene '{sceneName}'");
            yield return StartCoroutine(LoadSceneProgress(sceneName, callback));
            yield break;
        }
        
        // Invoke callback sau khi loading animation xong
        callback?.Invoke();
    }

    /// <summary>
    /// Simulate loading progress khi scene đã loaded rồi.
    /// Tạo ra animation loading mượt mà mà không cần load scene.
    /// </summary>
    private IEnumerator SimulateLoadingProgress()
    {
        float progress = 0f;
        float duration = 1.5f; // Loading animation kéo dài 1.5 giây
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progress = Mathf.Clamp01(elapsed / duration);
            
            // Publish progress so LoadingView can read it
            TaskManager.ins.SetCurrentTaskProgress(progress);
            this.progress = progress;

            yield return null;
        }

        // Ensure full progress
        TaskManager.ins.SetCurrentTaskProgress(1f);
        this.progress = 1f;
    }

    private IEnumerator LoadSceneProgress(string sceneName, Action onComplete = null)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float smooth = 0f;

        while (!op.isDone)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f);
            smooth = Mathf.MoveTowards(smooth, target, Time.deltaTime * 0.8f);

            // Publish progress so LoadingView can read it from TaskManager (or update LoadingView directly)
            TaskManager.ins.SetCurrentTaskProgress(smooth);
            progress = smooth;

            if (smooth >= 0.99f)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        // ensure full progress and invoke callback
        TaskManager.ins.SetCurrentTaskProgress(1f);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Check xem scene có loaded rồi không.
    /// </summary>
    private bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }
}
