using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("HideDialogAnimation");
        animator.Play("SpinViewHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ShowDialogAnimation");
        animator.Play("SpinViewShow");
    }
    public void OnSpinDone(Action callback)
    {
        this.callback = callback;
        animator.Play("SpinDone");
    }
    public void ShowAnim()
    {
        callback?.Invoke();
    }

    public void HideAnim()
    {
        callback?.Invoke();
    }
    public void Clear()
    {
        callback?.Invoke();
    }
    public void SpinDone()
    {
        callback?.Invoke();
    }
}
