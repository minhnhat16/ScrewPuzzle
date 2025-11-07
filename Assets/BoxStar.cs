using DG.Tweening;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxStar : MonoBehaviour
{
    [SerializeField] private Transform tf;
    [SerializeField] private Vector3 pos;
    [SerializeField] private SpriteRenderer render;
    [SerializeField] private ParticleSystem particle;

    public Transform Tf { get => tf; set => tf = value; }

    private void Awake()
    {
        Tf = transform;
        pos = Tf.position;
        render = GetComponent<SpriteRenderer>();
        particle = GetComponentInChildren<ParticleSystem>();
    }

    public void SetStarPos(Vector3 pos)
    {
        tf.SetLocalPositionAndRotation(pos, Quaternion.identity);
    }
    public void PopingStar(Vector3 popupScale, Vector3 targetPos, Action onComplete)
    {
        float popupDuration = GameManager.instance.PopupDuration;
        float moveDuration  = GameManager.instance.StarMoveDuration;
        // Start with a sequence
        var scale = transform.localScale;
        Sequence sequence = DOTween.Sequence();
        transform.DOScale(Vector3.zero,0);
        // Step 1: Popup effect (scale up and back to normal)
        sequence.Append(transform.DOScale(popupScale, popupDuration / 2).SetEase(Ease.OutBack));
        sequence.Append(transform.DOScale(scale, popupDuration/2 ).SetEase(Ease.InBack));

        // Step 2: Move to the target position
        sequence.Append(transform.DOMove(targetPos, moveDuration/2).SetEase(Ease.InQuart));

        // Optional: Play the sequence
        sequence.Play();
        particle.Play();


        sequence.OnComplete(() =>
        {
            onComplete.Invoke();
            StarPool.Instance.pool.ReturnToPool(this);
        });
    }
    public void PopingStarNew(Vector3 popupScale, Vector3 targetPos, Action onComplete)
    {
        float popupDuration = GameManager.instance.PopupDuration;
        float moveDuration = GameManager.instance.StarMoveDuration;

        // Ensure no conflicting animations
        DOTween.Kill(transform);
        transform.localScale = Vector3.zero; // Reset scale

        // Start with a sequence
        var scale = transform.localScale;
        Sequence sequence = DOTween.Sequence();

        // Step 1: Popup effect (scale up and back to normal)
        sequence.Append(transform.DOScale(popupScale, popupDuration / 2).SetEase(Ease.OutBack));
        sequence.Append(transform.DOScale(scale, popupDuration / 2).SetEase(Ease.InBack));

        // Step 2: Move to the target position
        sequence.Append(transform.DOMove(targetPos, moveDuration / 2).SetEase(Ease.InQuart));

        // OnComplete logic
        sequence.OnComplete(() =>
        {
            Debug.Log("PopingStar: Animation complete. Returning to pool.");
            onComplete.Invoke();
            StarPool.Instance.pool.ReturnToPool(this);
        });

        // Play sequence
        sequence.Play();

        // Play particle effects
        if (particle.isPlaying)
            particle.Stop();
        particle.Play();
    }

}
