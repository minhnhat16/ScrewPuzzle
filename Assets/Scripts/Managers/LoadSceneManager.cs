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

    List<Func<IEnumerator>> preTask =null;

    public float TimeWait { get => timeWait; set => timeWait = value; }

    private void Awake()
    {
        ins = this;
    }

    // Hàm chính gọi load
    public void LoadSceneByName(string sceneName, Action callback)
    {
        StopAllCoroutines();
        this.callback = callback;

        this.sceneName = sceneName;
        ViewManager.Instance.SwitchView(ViewIndex.LoadingView, null, () =>
        {
            Debug.Log("Switch loading view ");
            StartCoroutine(RunFullLoadProcess());
        });
    }
     IEnumerator RunFullLoadProcess()
    {
        yield return TaskManager.ins.RunTasks(callback);
        Debug.Log("Run task done");
        // 2. Load scene progress + fake progress
        yield return StartCoroutine(LoadSceneProgress(sceneName));
    }


    private IEnumerator LoadSceneProgress(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float smooth = 0;
        float target = TaskManager.ins.CurrentTaskProgress;
        while (!op.isDone)
        {
            target = Mathf.Clamp01(op.progress / 0.9f);

            smooth = Mathf.MoveTowards(smooth, target, Time.deltaTime * 0.8f);

            // Gửi progress đến LoadingView

           progress = smooth;


            Debug.Log("Progress " + progress); ;
            if (smooth >= 0.99f)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }

}
