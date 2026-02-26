using DG.Tweening;
using UnityEngine;
using System;

public class TweenMover : MonoBehaviour, IMovable
{
    private Tween moveTween;
    public bool IsMoving { get; private set; }

    public void MoveTo(Vector3 target, float duration, Ease ease = Ease.OutCubic, Action onComplete = null)
    {
        moveTween?.Kill();

        IsMoving = true;

        moveTween = transform
            .DOMove(target, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                IsMoving = false;
                onComplete?.Invoke();
            });
    }

    public void KillMove()
    {
        moveTween?.Kill();
        IsMoving = false;
    }
}