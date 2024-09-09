using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("BreakHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("BreakShow");
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
}
