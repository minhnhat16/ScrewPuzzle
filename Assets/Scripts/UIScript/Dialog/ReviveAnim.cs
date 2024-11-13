using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    private void Awake()
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("RateHideAnim");
        animator.Play("ReviveHIdeAnim");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("RateShowAnim");
        animator.Play("ReviveShowAnim");
    }

    public void ShowAnim()
    {
        callback?.Invoke();
    }

    public void HideAnim()
    {
        //Debug.Log("HideAnim");
        callback?.Invoke();
    }
    public void Clear()
    {
        callback?.Invoke();
    }
    public void RateCallBack()
    {
        Debug.Log("Revive callback ");
        callback?.Invoke();
    }
}
