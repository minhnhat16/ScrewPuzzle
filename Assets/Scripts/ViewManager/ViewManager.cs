using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
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
            if(dicView.ContainsKey(viewIndex))  continue;
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
        currentView = dicView[newView];
        currentView.gameObject.SetActive(true);
        //Debug.Log("Show Next View call back " + currentView);

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

        if(currentView is GameView gameview)
        {
            gameview.UpdateSpecialBoxCount(color, v);
        }
    }
}