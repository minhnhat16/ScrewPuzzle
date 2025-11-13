using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager instance;
    [Range(0f, 1f)] public float sampleWait = 0.5f;
    [SerializeField] private float timeWait = 5f;
    public float progress;

    private Action callback;

    private void Awake()
    {
        instance = this;
    }

    // Hàm chính gọi load
    public void LoadSceneByName(string sceneName, Action callback, List<Func<IEnumerator>> preTasks = null)
    {
        StopAllCoroutines();
        this.callback = callback;

        ViewManager.Instance.SwitchView(ViewIndex.LoadingView, null, () =>
        {
            StartCoroutine(LoadSceneProgress(sceneName, preTasks));
        });
    }

    private IEnumerator LoadSceneProgress(string sceneName, List<Func<IEnumerator>> preTasks)
    {
        progress = 0;
        float taskWeight = sampleWait;
        float sceneWeight = 1f - sampleWait;

        // 🔹 1. Chạy các pre-task
        if (preTasks != null && preTasks.Count > 0)
        {
            float step = taskWeight / preTasks.Count;
            foreach (var task in preTasks)
            {
                yield return StartCoroutine(task());
                progress += step; // tăng dần progress khi task xong
            }
        }

        // 🔹 2. Bắt đầu load scene
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false; // tạm thời dừng khi 0.9

        while (async.progress < 0.9f)
        {
            progress = sampleWait + async.progress * sceneWeight;
            yield return null;
        }

        // 🔹 3. Chờ một chút cho đẹp (optional)
        yield return new WaitForSeconds(0.5f);

        // 🔹 4. Cho phép vào scene
        async.allowSceneActivation = true;

        // 🔹 5. Gọi callback
        callback?.Invoke();
    }
}
