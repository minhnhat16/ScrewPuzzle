using DG.Tweening;
using System;
using UnityEngine;

namespace Ingame.Screw
{
    public class ScrewAnimation : MonoBehaviour
    {
        [SerializeField] private Transform renderTransform;

        public void Shake(Action onComplete = null)
        {
            Tween t = renderTransform.DOShakePosition(1f, new Vector3(0.15f, 0, 0))
                .SetEase(Ease.OutBounce);

            t.OnComplete(() =>
            {
                renderTransform.localPosition = Vector3.zero;
                onComplete?.Invoke();
            });
        }

        public void MoveScrewUp(Action callback)
        {
            Vector3 targetPos = renderTransform.position + new Vector3(0, 0.22f, 0);

            Sequence seq = DOTween.Sequence();
            seq.Append(renderTransform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad))
                .Join(renderTransform.DORotate(new Vector3(0, 0, 290), 0.2f, RotateMode.FastBeyond360))
                .Join(renderTransform.DOScale(1.08f, 0.1f).SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                renderTransform.DOScale(1f, 0.1f);
                callback?.Invoke();
            });
        }

        public void JumpScrewToHold(HoldScrew holdScrew, Action onComplete)
        {
            Vector3 toPos = holdScrew.transform.position + new Vector3(0, 0.18f);

            Sequence seq = DOTween.Sequence();
            seq.Append(renderTransform
                .DOJump(toPos, 1.6f, 1, 0.45f)
                .SetEase(Ease.OutQuad));
            seq.Join(renderTransform.DOScale(1.12f, 0.3f));

            seq.OnComplete(() =>
            {
                MoveScrewDown(holdScrew, onComplete);
            });
        }

        private void MoveScrewDown(HoldScrew holdScrew, Action onComplete)
        {
            var targetPos = renderTransform.position - new Vector3(0, 0.18f);
            Sequence seq = DOTween.Sequence();
            seq.Append(renderTransform.DOMoveY(targetPos.y, 0.15f))
               .Join(renderTransform.DORotate(new Vector3(0, 0, -360f), 0.15f, RotateMode.FastBeyond360))
               .Join(renderTransform.DOScale(1f, 0.15f));

            seq.OnComplete(() => { 
                ResetRenderLocalPosition();
                onComplete?.Invoke(); });
        }

        /// <summary>
        /// Stop any running tweens on the render and reset its local transform to identity.
        /// Use this instead of setting component.transform.position = Vector3.zero.
        /// </summary>
        public void ResetRenderLocalPosition()
        {
            if (renderTransform == null) return;
            // Kill any running tweens on this transform to avoid conflicting animations
            renderTransform.DOKill();
            renderTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            renderTransform.localScale = Vector3.one;
        }
    }
}
