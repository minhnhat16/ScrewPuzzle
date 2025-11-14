using System;
using UnityEngine;

public class QuitAnim : BaseDialogAnimation
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
        //Debug.Log("ItemDialogAnimation");
        animator.Play("QuitHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ItemDialogAnimation");
        animator.Play("QuitShow");
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
