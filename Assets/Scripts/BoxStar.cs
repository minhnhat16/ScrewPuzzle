using DG.Tweening;
using Ingame;
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
        float moveDuration = GameManager.instance.StarMoveDuration;

        Vector3 startScale = transform.localScale;

        // Reset trạng thái
        transform.localScale = Vector3.zero;
        render.color = new Color(render.color.r, render.color.g, render.color.b, 1f);

        Sequence seq = DOTween.Sequence();

        // ⭐ 1. Popup scale
        seq.Append(transform.DOScale(popupScale, popupDuration * 0.5f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(startScale, popupDuration * 0.5f).SetEase(Ease.InBack));

        // ⭐ 2. Move tới target
        seq.Append(transform.DOMove(targetPos, moveDuration * 0.7f).SetEase(Ease.InQuad));

        //// ⭐ 3. FadeOut khi gần chạm
        //seq.Join(render.DOFade(0f, moveDuration * 0.4f).SetEase(Ease.OutSine));

        // ⭐ 4. Charging effect (tại target)
        seq.AppendCallback(() =>
        {
            // gọi tăng star
            IngameController.ins.StarChanging(1);

            // hiệu ứng charge (scale pulse + flash)
            Sequence charge = DOTween.Sequence();
            charge.Append(transform.DOScale(1.35f, 0.15f).SetEase(Ease.OutBack));
            charge.Append(transform.DOScale(1f, 0.12f).SetEase(Ease.InSine));

            // nếu có particle nổ sáng
            if (particle != null)
                particle.Play();
        });

        // ⭐ 5. Delay 1 chút để charge xong
        seq.AppendInterval(0.05f);

        // ⭐ 6. Return pool sau charging
        seq.AppendCallback(() =>
        {
            onComplete?.Invoke();
            StarPool.Instance.pool.ReturnToPool(this);

            // Reset trước khi return
            transform.localScale = startScale;
            render.color = new Color(render.color.r, render.color.g, render.color.b, 1f);
        });
    }


    public void OnReset()
    {
        var color = render.color;
        color.a = 1;
        render.color = color;
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
