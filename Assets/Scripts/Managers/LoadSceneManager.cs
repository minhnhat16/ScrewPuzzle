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

    //List<Func<IEnumerator>> preTask =null;

    public float TimeWait { get => timeWait; set => timeWait = value; }

    private void Awake()
    {
        ins = this;
    }

    // Hàm chính gọi load
    public void LoadSceneByName(string sceneName, Action callback)
    {

        Debug.Log("Start Load Scene: " + sceneName);
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
        // Run TaskManager's runner (it yields each registered task).
        yield return StartCoroutine(TaskManager.ins.RunTasks());

        Debug.Log("Run task done");

        // After tasks finished, run scene load progress (we pass the original callback to invoke after load)
        yield return StartCoroutine(LoadSceneProgress(sceneName, callback));
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

            // also keep a local copy if needed
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

}
