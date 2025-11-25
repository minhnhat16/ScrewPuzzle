using System;
using UnityEngine;

public class QuestAnim : BaseDialogAnimation
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
        animator.Play("QuestHide");
    }

    public override void ShowDialogAnimation(Action callback)
    {
        this.callback = callback;
        //Debug.Log("ItemDialogAnimation");
        animator.Play("QuestShow");
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
