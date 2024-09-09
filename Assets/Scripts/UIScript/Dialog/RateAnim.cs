using System;
using UnityEngine;

public class RateAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;

    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("RateHideAnim");
        animator.Play("RateHideAnim");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("RateShowAnim");
        animator.Play("RateShowAnim");
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
        Debug.Log("RateCallBack");
        callback?.Invoke();
    }
    public void PlaySuccesfullRating( Action callback)
    {
        this.callback = callback;
        //Debug.Log("PlaySuccesfullRating");
        animator.Play("SuccessRate");
    }
}
