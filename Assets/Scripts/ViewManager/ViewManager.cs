using Enums;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

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
        yield return new WaitForSeconds(0.1f);
        foreach (ViewIndex viewIndex in ViewConfig.viewArray)
        {
            if (dicView.ContainsKey(viewIndex)) continue;
            string viewName = viewIndex.ToString();
            GameObject view = Instantiate(Resources.Load("Prefabs/UIPrefab/Views/" + viewName, typeof(GameObject))) as GameObject;
            view.transform.SetParent(anchorView, false);
            view.GetComponent<BaseView>().Init();
            dicView.Add(viewIndex, view.GetComponent<BaseView>());
            yield return new WaitForSeconds(0.5f);

        }
    }
    public void SwitchView(ViewIndex newView, ViewParam viewParam = null, Action callback = null)
    {

        Debug.Log("SwitchView to " + newView + " from " + (currentView != null ? currentView.viewIndex.ToString() : "null"));   
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

        // SAME VIEW
        if (currentView == nextView)
        {
            currentView.gameObject.SetActive(true);
            Debug.Log("SwitchView: same view " + newView + "callback " + callback == null);
            currentView.ShowViewAnimation(() =>
            {
                callback?.Invoke();
            });
            return;
        }

        // SWITCH VIEW
        currentView = nextView;
        currentView.gameObject.SetActive(true);

        currentView.Setup(viewParam);
        currentView.ShowViewAnimation(() =>
        {
            callback?.Invoke();
        });
    }



    public Vector3 UIToWorld(RectTransform uiObj, Camera worldCam)
    {
        var uiCam = this.canvas.worldCamera;
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
        {
            gameview.UpdateSpecialBoxCount(color, v);
        }
    }


    public void SwitchViewForNewPlayer(bool isNewPlayer)
    {
        if (isNewPlayer)
        {
            LevelManager.ins.LoadLevel(0, () =>
            {
                IngameController.ins.PauseGame();
            });
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
            });
        }

    }
}