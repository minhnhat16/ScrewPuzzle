using Coffee.UIExtensions;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public enum FillFollowType
{
    Horizontal,
    Vertical,
    Radial
}

public class StarBottleFillFlexible : StarBottleFill
{
    [Header("Follow")]
    [SerializeField] private RectTransform handle;
    [SerializeField] private FillFollowType followType;

    [Header("Common")]
    [SerializeField] private float animationMultiplier = 0.5f;

    [Header("Horizontal")]
    [SerializeField] private float offsetX;

    [Header("Vertical")]
    [SerializeField] private float offsetY;

    [Header("Radial")]
    [SerializeField] private float radius = 50f;
    [SerializeField] private float startAngle = -90f; // top
    [SerializeField] private bool clockwise = true;

    [Header("Percent UI")]
    [SerializeField] private Text percentText;

    private Tween handleTween;
    private RectTransform barRect;


    [SerializeField]
    private UIParticle popStart;
    private UnityEvent OnFillDone = new UnityEvent();
    protected override void OnEnable()
    {
        base.OnEnable();
        OnFillDone.AddListener(FillDone);

        // listen to base fill change event to update percent text continuously
        fillChange.AddListener(UpdatePercentText);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnFillDone.RemoveListener(FillDone);

        // unregister listener
        fillChange.RemoveListener(UpdatePercentText);
    }
    private void FillDone()
    {

        bool isCloseToOne = Mathf.Approximately(imgFill.fillAmount, 1f);

        if (isCloseToOne)
        {
            PlayPopStart();
            handle.gameObject.SetActive(false);
            if (percentText != null) percentText.text = "100%";
        }
        else
        {
            // ensure percent UI is synced when animation finishes but not full
            if (percentText != null) percentText.text = Mathf.RoundToInt(imgFill.fillAmount * 100f) + "%";
        }
    }

    private void PlayPopStart()
    {
        popStart.Play();
    }

    protected override void AnimateToPercent(float targetPercent)
    {
        base.AnimateToPercent(targetPercent);

        // update percent text to the target immediately (base tween will update further via fillChange)
        if (percentText != null)
            percentText.text = Mathf.RoundToInt(Mathf.Clamp01(targetPercent) * 100f) + "%";

        if (handle == null || imgFill == null) return;

        targetPercent = Mathf.Clamp01(targetPercent);
        handleTween.Kill();

        barRect = imgFill.transform.parent as RectTransform;

        switch (followType)
        {
            case FillFollowType.Horizontal:
                AnimateHorizontal(targetPercent);
                break;

            case FillFollowType.Vertical:
                AnimateVertical(targetPercent);
                break;

            case FillFollowType.Radial:
                AnimateRadial(targetPercent);
                break;
        }
    }

    #region Follow Types

    private void AnimateHorizontal(float percent)
    {
        float width = barRect.rect.width;
        float x = width * percent + offsetX;

        handleTween = handle
            .DOAnchorPosX(x, animationDuration * animationMultiplier)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => OnFillDone.Invoke());

    }

    private void AnimateVertical(float percent)
    {
        float height = barRect.rect.height;
        float y = height * percent + offsetY;

        handleTween = handle
            .DOAnchorPosY(y, animationDuration * animationMultiplier)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => OnFillDone.Invoke());
        ;
    }

    private void AnimateRadial(float percent)
    {
        float angle = startAngle +
            (clockwise ? -1 : 1) * percent * 360f;

        float rad = angle * Mathf.Deg2Rad;
        Vector2 pos = new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        ) * radius;

        handleTween = handle
            .DOAnchorPos(pos, animationDuration * animationMultiplier)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => OnFillDone.Invoke());
    }

    #endregion

    public override void OnReset()
    {
        base.OnReset();
        handleTween?.Kill();
        handle.anchoredPosition = Vector2.zero;
        handle.gameObject.SetActive(true);

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(imgFill.fillAmount * 100f) + "%";
    }

    private void OnValidate()
    {
        //Debug.Log("Validated " + imgFill.fillAmount);
        AnimateToPercent(imgFill.fillAmount);

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(imgFill.fillAmount * 100f) + "%";
    }

    // Called by base fillChange event to update percent UI while tweening
    private void UpdatePercentText(float percent)
    {
        if (percentText == null) return;
        percentText.text = Mathf.RoundToInt(Mathf.Clamp01(percent) * 100f) + "%";
    }
}
