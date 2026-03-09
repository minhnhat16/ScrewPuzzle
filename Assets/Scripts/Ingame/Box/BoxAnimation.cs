using DG.Tweening;
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
    [SerializeField] private SoundManager.SFX soundLidFall = SoundManager.SFX.BoxClose;  // nắp đang rơi
    [SerializeField] private SoundManager.SFX soundLidLand = SoundManager.SFX.BoxLand;   // nắp chạm xuống
    [SerializeField] private SoundManager.SFX soundBoxExit = SoundManager.SFX.BoxExit;   // box bay đi

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

        // Nắp bắt đầu rơi → play sound rơi
        //_currentSequence.AppendCallback(() =>
        //    SoundHelper.PlaySFX(soundLidFall));

        // Nắp rơi xuống
        _currentSequence.Append(upperBoxRenderer.transform
            .DOLocalMove(Vector3.zero, fallDuration)
            .SetEase(Ease.InQuad));

        // Nắp chạm → play sound đóng sầm + punch scale
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

        // Squeeze anticipation
        _currentSequence.Append(transform
            .DOScale(new Vector3(1f, 1f, 1f), 0.08f)
            .SetEase(Ease.OutQuad));

        // Sound khi bay đi
        _currentSequence.AppendCallback(() =>
            SoundHelper.PlaySFX(soundBoxExit));

        // Move + scale nhỏ đồng thời
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
            gameObject.SetActive(false);
        });
    }

    // ─── Helpers ───────────────────────────────────────────────────

    private Vector3 GetTopRightWorldPosition()
    {
        return CameraMain.instance.GetTopRight();
    }
}