using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOffCardAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("OutCardHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("OutCardShow");
    }

    public void ShowAnim()
    {
        callback?.Invoke();
    }

    public void HideAnim()
    {
        callback?.Invoke();
    }
    public void SetLoseCamOn()
    {
        //Debug.Log("Clear");
    }
    public void SetLoseCamOff()
    {
    }
}
