
using System;
using UnityEngine;

public class BaseView : MonoBehaviour
{
    public ViewIndex viewIndex;
    [SerializeField]
    private BaseViewAnimation baseViewAnim;

    public BaseViewAnimation BaseViewAnimation { get { return baseViewAnim; } }
    private void Awake()
    {
        baseViewAnim = gameObject.GetComponentInChildren<BaseViewAnimation>();
    }

    public void Init()
    {
        OnInit();
        gameObject.SetActive(false);

    }

    public virtual void Setup(ViewParam viewParam) { }

    public virtual void OnInit() { }
    public virtual void OnInit(Action callback) { }


    public void ShowViewAnimation(Action callback)
    {
        baseViewAnim.ShowViewAnimation(() =>
        {
            OnStartShowView();
            callback?.Invoke();
            OnEndShowView();
            HideView();
        });
    }
    public void HideViewAnimation(Action callback)
    {
        baseViewAnim.HideViewAnimation(() => 
        {
            //Debug.Log("HideViewAnimation");
            OnStartHideView();
            callback?.Invoke();
            OnEndHideView();
            ShowView();
        });
    }

    public virtual void OnStartShowView() { }

    public virtual void OnEndShowView() { }

    public virtual void OnStartHideView() { }

    public virtual void OnEndHideView() { }

    public virtual void ShowView()
    {

    }

    public virtual void HideView()
    {

    }
}
