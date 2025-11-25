using System;
using UnityEngine;

public class LoadingAnim : BaseViewAnimation
{
    public Animator animator;
    private Action callback;

    private void Awake()
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }
    // Start is called before the first frame update
    public override void HideViewAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("LoadingHide");
    }

    public override void ShowViewAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("LoadingShow");
    }
    public void ShowAnim()
    {
        Debug.Log("show anim loading");

        callback?.Invoke();
    }

    public void HideAnim()
    {
        Debug.Log("hide anim loading");
        callback?.Invoke();
    }
}
