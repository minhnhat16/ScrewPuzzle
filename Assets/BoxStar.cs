using DG.Tweening;
using Managers;
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
    public void PopingStar(Vector3 popupScale, Vector3 targetPos)
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
        sequence.Append(transform.DOMove(targetPos, moveDuration/2).SetEase(Ease.OutQuad));

        // Optional: Play the sequence
        sequence.Play();
        particle.Play();


        sequence.OnComplete(() =>
        {
            StarPool.Instance.pool.ReturnToPool(this);
        });
    }

}
