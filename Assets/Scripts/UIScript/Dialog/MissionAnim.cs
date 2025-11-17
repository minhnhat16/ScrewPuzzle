using System;
using UnityEngine;

public class MissionAnim : BaseDialogAnimation
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
        animator.Play("MissionHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ItemDialogAnimation");
        animator.Play("MissionShow");
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