using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance;
    public Transform anchorView;
    public Dictionary<ViewIndex, BaseView> dicView = new Dictionary<ViewIndex, BaseView>();
    public BaseView currentView = null;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponent<Canvas>();
        LoadingView loadingView = GetComponentInChildren<LoadingView>(true);
        Debug.Log("loading view " + loadingView);
        dicView.Add(ViewIndex.LoadingView, loadingView);
    }

    public IEnumerator Init()
    {
        // Quick init with minimal delay - defer view loading
        yield return new WaitForSeconds(0.1f);

        // Only log that Init started, actual view loading is now lazy-loaded on demand
        Debug.Log("[ViewManager] Init completed. Views will be lazy-loaded on first use.");
    }

    /// <summary>
    /// Lazy-load a view on first access. Called internally before switching to a view.
    /// </summary>
    private IEnumerator EnsureViewLoaded(ViewIndex viewIndex)
    {
        if (dicView.ContainsKey(viewIndex))
            yield break; // Already loaded

        Debug.Log($"[ViewManager] Lazy-loading view: {viewIndex}");
        float startTime = Time.realtimeSinceStartup;

        string viewName = viewIndex.ToString();
        GameObject view = Instantiate(Resources.Load("Prefabs/UIPrefab/Views/" + viewName,
            typeof(GameObject))) as GameObject;

        if (view == null)
        {
            Debug.LogError($"[ViewManager] Failed to load view prefab: {viewName}");
            yield break;
        }

        view.transform.SetParent(anchorView, false);
        view.GetComponent<BaseView>().Init();
        dicView.Add(viewIndex, view.GetComponent<BaseView>());

        float duration = Time.realtimeSinceStartup - startTime;
        Debug.Log($"[ViewManager] Lazy-loaded view {viewIndex} in {duration:F2}s");
    }

    /// <summary>
    /// Tìm view theo type T trong dicView.
    /// Trả về null nếu không tìm thấy.
    /// </summary>
    public T GetView<T>() where T : BaseView
    {
        foreach (var view in dicView.Values)
        {
            if (view is T typed)
                return typed;
        }

        Debug.LogWarning($"[ViewManager] GetView<{typeof(T).Name}>: không tìm thấy view.");
        return null;
    }

    public void SwitchView(ViewIndex newView, ViewParam viewParam = null, Action callback = null)
    {
        Debug.Log("SwitchView to " + newView + " from " +
                  (currentView != null ? currentView.viewIndex.ToString() : "null"));

        // Ensure the target view is loaded (lazy-load if needed)
        StartCoroutine(SwitchViewAsync(newView, viewParam, callback));
    }

    private IEnumerator SwitchViewAsync(ViewIndex newView, ViewParam viewParam = null, Action callback = null)
    {
        // Ensure view is loaded
        yield return StartCoroutine(EnsureViewLoaded(newView));

        if (currentView != null)
        {
            currentView.HideViewAnimation(() =>
            {
                currentView.gameObject.SetActive(false);
                ShowNextView(newView, viewParam, callback);
            });
        }
        else
        {
            ShowNextView(newView, viewParam, callback);
        }
    }

    private void ShowNextView(ViewIndex newView, ViewParam viewParam = null, Action callback = null)
    {
        var nextView = dicView[newView];

        if (currentView == nextView)
        {
            currentView.gameObject.SetActive(true);
            currentView.ShowViewAnimation(() => callback?.Invoke());
            return;
        }

        currentView = nextView;
        currentView.gameObject.SetActive(true);
        currentView.Setup(viewParam);
        currentView.ShowViewAnimation(() => callback?.Invoke());
    }

    public Vector3 UIToWorld(RectTransform uiObj, Camera worldCam)
    {
        var uiCam = canvas.worldCamera;
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCam, uiObj.position);
        screenPos.z = Mathf.Abs(worldCam.transform.position.z);
        return worldCam.ScreenToWorldPoint(screenPos);
    }

    public T GetUIObject<T>(BaseView targetView) where T : Component
    {
        return targetView.GetComponentInChildren<T>(true);
    }

    internal void UpdateSpecialBoxCount(ColorEnum color, int v)
    {
        if (currentView is GameView gameview)
            gameview.UpdateSpecialBoxCount(color, v);
    }

    public void SwitchViewForNewPlayer(bool isNewPlayer)
    {
        if (isNewPlayer)
        {
            var service = new LevelStartService(LoadSceneManager.ins);
            service.StartLevel(
                levelId: 1,
                onLevelStarted: () => Debug.Log("[ViewManager] New player level 0 started."),
                onError: (err) => Debug.LogError($"[ViewManager] {err}")
            );
        }
        else
        {

            MainScreenViewParam param = new()
            {
                totalGold = DataAPIController.instance.GetGold(),
                ticket = (int)DataAPIController.instance.GetTicket(),
                level = DataAPIController.instance.GetPlayerLevel(),
            };

            ViewManager.Instance.SwitchView(ViewIndex.MainScreenView, param, () =>
            {
                Debug.Log("task run done switch view");
                DayTimeController.instance.CheckNewDay();

                var isAppOpenEnabled = ZenSDK.instance.IsAppOpenReady();
                Debug.Log("IsAppOpenReady: " + isAppOpenEnabled);
                ZenSDK.instance.ShowAppOpen((success) =>
                {
                    Debug.Log("ShowAppOpen callback: " + success);
                });
            });
        }
    }
}