using DG.Tweening;
using System;
using UnityEngine;

public interface IMovable
{
    bool IsMoving { get; }
    void MoveTo(Vector3 target, float duration, Ease ease = Ease.OutCubic, Action onComplete = null);
    void KillMove();
}