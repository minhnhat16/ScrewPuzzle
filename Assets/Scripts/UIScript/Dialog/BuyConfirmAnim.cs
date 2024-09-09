using System;
using UnityEngine;

public class BuyConfirmAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("BuyConfirmHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("BuyConfirmShow");
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
