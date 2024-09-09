using System;
using UnityEngine;

public class ItemDialogAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ItemDialogAnimation");
        animator.Play("ItemDialogHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ItemDialogAnimation");
        animator.Play("ItemDialogShow");
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
