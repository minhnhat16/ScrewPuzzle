using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyRewardAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("HideDialogAnimation");
        animator.Play("DailyRewardHide");
    }
    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ShowDialogAnimation");
        animator.Play("DailyRewardShow");
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
