using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseViewAnimation : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
        }
    }

    public virtual void ShowViewAnimation(Action callback)
    {
        /*canvasGroup.DOFade(1, 0.5f).OnComplete(() =>
        {
            callback();
        }).SetUpdate(true);*/
        callback?.Invoke();
    }

    public virtual void HideViewAnimation(Action callback) 
    {
        /*canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
        {
            callback();
        }).SetUpdate(true);*/
        callback?.Invoke();
    } 
}
