using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreenAnim : BaseViewAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideViewAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("MainViewHide");
    }

    public override void ShowViewAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("MainViewShow");
    }
    public void PlayToast()
    {
        animator.Play("ToastShow");

    }
    public void HideToast()
    {
        animator.Play("ToastHide");

    }
    public void ShowAnim()
    {
        callback?.Invoke();
    }

    public void HideAnim()
    {
        callback?.Invoke();
    }
    public void SpinDone()
    {
        callback?.Invoke();
    }
}
