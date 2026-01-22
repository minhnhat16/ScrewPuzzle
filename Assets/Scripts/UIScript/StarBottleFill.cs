using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StarBottleFill : MonoBehaviour, IResetable
{
    [SerializeField] protected Image imgFill;
    public float animationDuration = 1.0f;

    public UnityEvent<float> fillChange = new();

    protected Tween fillTween;

    protected virtual void OnEnable()
    {
        fillChange.AddListener(AnimateToPercent);
    }

    protected virtual void OnDisable()
    {
        fillChange.RemoveListener(AnimateToPercent);
    }

    protected virtual void AnimateToPercent(float targetPercent)
    {
        if (imgFill == null) return;

        fillTween?.Kill();

        fillTween = imgFill
            .DOFillAmount(targetPercent, animationDuration / 2)
            .SetEase(Ease.InOutSine);
    }

    public virtual float CurrentFill => imgFill != null ? imgFill.fillAmount : 0f;

    public virtual void OnReset()
    {
        // kill and clear any running tween to avoid stray callbacks
        fillTween?.Kill();
        fillTween = null;

        // guard against missing image
        if (imgFill != null)
        {
            imgFill.fillAmount = 0f;
            imgFill.transform.localScale = Vector3.one;
        }
    }
}
