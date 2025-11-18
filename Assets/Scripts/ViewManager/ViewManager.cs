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
        LoadingView loadingView = GetComponentInChildren<LoadingView>();
        dicView.Add(ViewIndex.LoadingView, loadingView);
    }

    IEnumerator Start()
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
        //Debug.Log("Switch View" + newView);
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
            Debug.Log("Show Next View");
            ShowNextView(newView, viewParam, callback);
        }
    }

    private void ShowNextView(ViewIndex newView, ViewParam viewParam = null, Action callback = null)
    {
        //Debug.Log("Show Next View");
        currentView = dicView[newView];
        currentView.gameObject.SetActive(true);
        currentView.Setup(viewParam);
        currentView.ShowViewAnimation(() =>
        {
            callback?.Invoke();
        });
    }
}