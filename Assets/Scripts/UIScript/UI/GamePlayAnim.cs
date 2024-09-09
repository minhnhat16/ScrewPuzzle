using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GamePlayAnim : BaseViewAnimation
{
    public Animator animator;
    private Action callback;


    // Start is called before the first frame update
    public override void HideViewAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("GamePlayHide");
    }

    public override void ShowViewAnimation(Action callback)
    {
        this.callback = callback;
        animator.Play("GamePlayShow");
    }

    public void ShowAnim()
    {
        callback?.Invoke();
    }

    public void HideAnim()
    {
        callback?.Invoke();
    }
}
