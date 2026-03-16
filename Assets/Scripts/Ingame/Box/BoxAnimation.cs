using DG.Tweening;
using Ingame.Pools;
using System;
using UnityEngine;

public class BoxAnimation : MonoBehaviour, IBoxAnimator
{
    [SerializeField] private SpriteRenderer boxSpriteRenderer;
    [SerializeField] private SpriteRenderer upperBoxRenderer;

    [Header("Close Animation Settings")]
    [SerializeField] private float fallStartY = 10f;
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private int bounceCount = 2;
    [SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private Vector2 punchScale = new(1.2f, 1.2f);
    [SerializeField] private Vector3 upperStartPosition = new(0, 10, 0);

    [Header("Lid Close Animation Settings")]
    [SerializeField] private float lidCloseDuration = 0.5f;
    [SerializeField] private float lidRotationAngle = 90f;

    [Header("Exit Animation Settings")]
    [SerializeField] private float exitDuration = 0.5f;
    [SerializeField] private float exitScaleDown = 0.3f;
    [SerializeField] private Vector2 exitScreenOffset = new(-80f, -80f);

    [Header("Sound")]
    [SerializeField] private SoundManager.SFX soundLidFall = SoundManager.SFX.BoxClose;
    [SerializeField] private SoundManager.SFX soundLidLand = SoundManager.SFX.BoxLand;
    [SerializeField] private SoundManager.SFX soundBoxExit = SoundManager.SFX.BoxExit;

    private Transform upperBoxTransform;
    private Sequence _currentSequence;

    private void Awake()
    {
        if (upperBoxRenderer != null)
            upperBoxTransform = upperBoxRenderer.transform;
    }

    private void OnDestroy()
    {
        _currentSequence?.Kill();
    }

    // ─── KillAllAnimations ─────────────────────────────────────────

    /// <summary>
    /// Kill tất cả tween pending — gọi khi box được reuse từ pool.
    /// Tránh stale callback từ PlayExitAnimation level cũ fire vào box mới.
    /// </summary>
    public void KillAllAnimations()
    {
        _currentSequence?.Kill(complete: false); // complete=false → KHÔNG fire onComplete
        _currentSequence = null;

        // Reset visual về trạng thái ban đầu
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        if (upperBoxTransform != null)
        {
            upperBoxTransform.localScale = Vector3.one;
            upperBoxTransform.localRotation = Quaternion.identity;
            upperBoxTransform.localPosition = upperStartPosition;
            upperBoxRenderer.gameObject.SetActive(false);
        }
    }

    // ─── PlayCloseAnimation ────────────────────────────────────────

    public void PlayCloseAnimation(Action onComplete = null)
    {
        if (upperBoxTransform == null)
        {
            Debug.LogWarning("[BoxAnimation] upperBoxRenderer not assigned.");
            onComplete?.Invoke();
            return;
        }

        _currentSequence?.Kill();
        upperBoxRenderer.gameObject.SetActive(true);
        upperBoxTransform.localPosition = upperStartPosition;
        upperBoxTransform.gameObject.SetActive(true);

        _currentSequence = DOTween.Sequence();

        _currentSequence.Append(upperBoxRenderer.transform
            .DOLocalMove(Vector3.zero, fallDuration)
            .SetEase(Ease.InQuad));

        _currentSequence.AppendCallback(() =>
            SoundHelper.PlaySFX(soundLidLand));

        _currentSequence.Append(transform
            .DOPunchScale(punchScale, fallDuration)
            .SetEase(Ease.InCirc));

        _currentSequence.OnComplete(() => onComplete?.Invoke());
    }

    // ─── PlayExitAnimation ─────────────────────────────────────────

    public void PlayExitAnimation(Action onComplete = null)
    {
        _currentSequence?.Kill();

        Vector3 topRightWorld = GetTopRightWorldPosition();

        _currentSequence = DOTween.Sequence();

        _currentSequence.Append(transform
            .DOScale(new Vector3(1f, 1f, 1f), 0.08f)
            .SetEase(Ease.OutQuad));

        _currentSequence.AppendCallback(() =>
            SoundHelper.PlaySFX(soundBoxExit));

        _currentSequence.Append(transform
            .DOMove(topRightWorld, exitDuration)
            .SetEase(Ease.InBack));

        _currentSequence.Join(transform
            .DOScale(exitScaleDown, exitDuration)
            .SetEase(Ease.InQuad));

        _currentSequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            if (upperBoxTransform != null)
            {
                upperBoxTransform.localScale = Vector3.one;
                upperBoxTransform.localRotation = Quaternion.identity;
            }

        });
    }

    // ─── Helpers ───────────────────────────────────────────────────

    private Vector3 GetTopRightWorldPosition()
    {
        return CameraMain.instance.GetTopRight();
    }
}