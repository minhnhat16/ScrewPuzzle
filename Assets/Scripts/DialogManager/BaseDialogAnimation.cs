using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDialogAnimation : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        /*canvasGroup = gameObject.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }*/
    }

    public virtual void ShowDialogAnimation(Action callback)
    {
        /*canvasGroup.DOFade(1, 0.5f).OnComplete(() =>
        {
            callback();
        }).SetUpdate(true);*/

        callback();
    }

    public virtual void HideDialogAnimation(Action callback)
    {
        /*canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
        {
            callback();
        }).SetUpdate(true);*/

        callback();
    }
}
