using DG.Tweening;
using UnityEngine;
using System;

public class TweenMover : MonoBehaviour, IMovable
{
    private Tween moveTween;

    [SerializeField] private bool isMoving;

    public bool IsMoving { get => isMoving; set => isMoving = value; }

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
    public void OnMoving(Action action)
    {
        moveTween.onUpdate += () => action?.Invoke();
    }
    public void KillMove()
    {
        moveTween?.Kill();
        IsMoving = false;
    }
}