using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsRemoveAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("SpecialHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("SpecialShow");
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
