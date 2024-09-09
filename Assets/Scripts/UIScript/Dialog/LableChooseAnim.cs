using System;
using UnityEngine;

public class LableChooseAnim : BaseDialogAnimation
{
    public Animator animator;
    private Action callback;
    public override void HideDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("HideDialogAnimation");
        animator.Play("LableHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ShowDialogAnimation");
        animator.Play("LabelShow");
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
