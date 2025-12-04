using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StarBottleFill : MonoBehaviour, IResetable
{
    [SerializeField] private Image imgFill;
    public float animationDuration = 1.0f;

    public UnityEvent<float> fillChange = new();

    private Tween fillTween;
    private Tween popTween;

    public void OnEnable()
    {
        fillChange.AddListener(AnimateToPercent);
    }
    public void OnDisable()
    {
        fillChange.RemoveListener(AnimateToPercent);
    }
    public void AnimateToPercent(float targetPercent)
    {
        if (imgFill == null) return;

        // Kill tween cũ tránh conflict
        fillTween?.Kill();
        popTween?.Kill();

        // Tween fill
        fillTween = imgFill
            .DOFillAmount(targetPercent, animationDuration / 2)
            .SetEase(Ease.InOutSine);

        //// Tween popping scale — scale nhẹ cho UI sống động
        //popTween = imgFill.transform
        //    .DOPunchScale(Vector3.one * 1.15f, animationDuration / 2)       // phồng lên
        //    .SetEase(Ease.OutBack).OnComplete(() =>
        //    {
        //        imgFill.transform.localScale = Vector3.one;
        //    });
    }
    public void OnReset()
    {
        fillTween?.Kill();
        popTween?.Kill();
        fillTween = imgFill.DOFillAmount(0, 0);

        imgFill.fillAmount = 0;
        imgFill.transform.localScale = Vector3.one;
    }
}
