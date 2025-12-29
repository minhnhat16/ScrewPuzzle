using UnityEngine;
using DG.Tweening;

public class UISliding : SingletonMono<UISliding>
{
    public void SlidePanel(
        RectTransform from,
        Vector2 fromTarget,
        RectTransform to,
        Vector2 toTarget,
        float duration = 0.25f,
        Ease ease = Ease.OutCubic,
        System.Action onComplete = null)
    {
        // Kill old tweens (avoid stacking)
        from.DOKill();
        to.DOKill();

        // Move new panel to start position before animation
        to.anchoredPosition = to.anchoredPosition;

        // Start animations
        from.DOAnchorPos(fromTarget, duration)
            .SetEase(ease);

        to.DOAnchorPos(toTarget, duration)
            .SetEase(ease)
            .OnComplete(() => onComplete?.Invoke());
    }
}
