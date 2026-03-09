using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TutorialHand : MonoBehaviour
{
    private Tween tween;
    private RectTransform rect;

    private void Awake()
    {
        rect = transform as RectTransform;
    }

    public void ShowAtScreenPos(Transform target, TutorialHandDirection dir)
    {

        var canvas = gameObject.GetComponentInParent<Canvas>();
        ScreenToWorld.Instance.WorldToScreenCanvas(target, canvas, out Vector2 pos);
        rect.anchoredPosition = pos + new Vector2(150, -150);
        gameObject.SetActive(true);
        PlayTween();
    }

    private void PlayTween()
    {
        tween?.Kill();
        tween = rect
            .DOAnchorPosY(rect.anchoredPosition.y + 30f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void Hide()
    {
        tween?.Kill();
        gameObject.SetActive(false);
    }
}

