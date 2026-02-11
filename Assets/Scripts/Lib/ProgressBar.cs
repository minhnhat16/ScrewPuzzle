using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ProgressBar : MonoBehaviour
{


    [Header("Assign the fill image (type: Filled)")]
    [SerializeField] internal Image fillImage;

    [Header("Optional: Show progress text")]
    [SerializeField] internal Text progressText;

    [Header("Fill Direction")]
    [SerializeField] internal FillDirection fillDirection = FillDirection.HorizontalLeftToRight;

    [Range(0, 1)]
    [SerializeField] internal float progress = 0f;


    internal RectTransform fillRect;

    public float Progress
    {
        get => progress;
        set => SetProgress(value);
    }

    public virtual void Awake()
    {
        ApplyFillDirection();
        fillRect = fillImage.GetComponent<RectTransform>();
    }

    /// <summary>
    /// Set the progress value (0 to 1).
    /// </summary>
    public virtual void SetProgress(float value, bool bytime = true)
    {
        progress = value;
        if (bytime)
        {
            UpdateProgressByTime(value, 0.5f, null);
            return;
        }
        UpdateProgress();
    }

    public virtual void UpdateProgress()
    {
        if (fillImage != null)
            fillImage.fillAmount = progress;

        if (progressText != null)
            progressText.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
    }
    public virtual void UpdateProgressByTime(float targetProgress, float duration = 1f, Action callback = null)
    {
        if (fillImage != null)
        {
            // Tween the fillAmount from its current value to the target value
            fillImage.DOFillAmount(targetProgress, duration)
                     .SetEase(Ease.Linear)
                     .OnUpdate(() =>
                     {
                         // Update progress variable (if needed)
                         progress = fillImage.fillAmount;

                         // Update text during the tween
                         if (progressText != null)
                             progressText.text = $"Loading {Mathf.RoundToInt(progress * 100)}%";
                     })
                     .OnComplete(() => callback?.Invoke());
        }
    }
    private void ApplyFillDirection()
    {
        if (fillImage == null) return;

        fillImage.type = Image.Type.Filled;

        switch (fillDirection)
        {
            case FillDirection.HorizontalLeftToRight:
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                break;
            case FillDirection.HorizontalRightToLeft:
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Right;
                break;
            case FillDirection.VerticalBottomToTop:
                fillImage.fillMethod = Image.FillMethod.Vertical;
                fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
                break;
            case FillDirection.VerticalTopToBottom:
                fillImage.fillMethod = Image.FillMethod.Vertical;
                fillImage.fillOrigin = (int)Image.OriginVertical.Top;
                break;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Áp dụng lại cấu hình fill direction nếu thay đổi trong Inspector
        ApplyFillDirection();

        // Giữ giá trị nằm trong 0–1
        progress = Mathf.Clamp01(progress);

        // Cập nhật trực quan thanh và particle trong Editor
        UpdateProgress();
    }
#endif
}
public enum FillDirection
{
    HorizontalLeftToRight,
    HorizontalRightToLeft,
    VerticalBottomToTop,
    VerticalTopToBottom
}