using DG.Tweening;
using System;
using UnityEngine;

namespace Ingame.Screw
{
    public class ScrewAnimation : MonoBehaviour, IResetable
    {
        [SerializeField] private Transform renderTransform;

        // Track the currently running tween/sequence so we can cancel/reset them safely.
        private Tween activeTween;
        private Sequence activeSequence;

        /// <summary>
        /// Cancel any tracked tweens/sequences and clear references.
        /// </summary>
        private void CancelActiveTweens()
        {
            try
            {
                if (activeSequence != null)
                {
                    activeSequence.Kill(false);
                    activeSequence = null;
                }

                if (activeTween != null)
                {
                    activeTween.Kill(false);
                    activeTween = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ScrewAnimation] CancelActiveTweens exception: {ex.Message}");
            }
        }

        public void Shake(Action onComplete = null)
        {
            if (renderTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            CancelActiveTweens();

            activeTween = renderTransform.DOShakePosition(1f, new Vector3(0.15f, 0, 0))
                .SetEase(Ease.OutBounce);

            activeTween.OnComplete(() =>
            {
                activeTween = null;
                // Reset local position and invoke callback safely
                if (renderTransform != null)
                    renderTransform.localPosition = Vector3.zero;

                try
                {
                    if (onComplete != null && this != null && gameObject != null && isActiveAndEnabled)
                        onComplete.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ScrewAnimation] Shake onComplete threw: {ex.Message}");
                }
            });
        }

        public void MoveScrewUp(Action callback)
        {
            if (renderTransform == null)
            {
                callback?.Invoke();
                return;
            }

            CancelActiveTweens();

            Vector3 targetPos = renderTransform.position + new Vector3(0, 0.22f, 0);

            activeSequence = DOTween.Sequence();
            activeSequence.Append(renderTransform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad))
                .Join(renderTransform.DORotate(new Vector3(0, 0, 290), 0.2f, RotateMode.FastBeyond360))
                .Join(renderTransform.DOScale(1.08f, 0.1f).SetEase(Ease.OutBack));

            activeSequence.OnComplete(() =>
            {
                activeSequence = null;
                // start the small scale-back tween and track it so it can be cancelled if needed
                activeTween = renderTransform.DOScale(1f, 0.1f);
                activeTween.OnComplete(() => { activeTween = null; });

                if (callback != null && this != null && gameObject != null && isActiveAndEnabled)
                    callback.Invoke();

            });
        }

        public void JumpScrewToHold(HoldScrew holdScrew, Action onComplete)
        {
            if (renderTransform == null || holdScrew == null)
            {
                onComplete?.Invoke();
                return;
            }

            CancelActiveTweens();

            Vector3 toPos = holdScrew.transform.position + new Vector3(0, 0.18f);

            activeSequence = DOTween.Sequence();
            activeSequence.Append(renderTransform
                .DOJump(toPos, 1.6f, 1, 0.45f)
                .SetEase(Ease.OutQuad));
            activeSequence.Join(renderTransform.DOScale(1.12f, 0.3f));

            activeSequence.OnComplete(() =>
            {
                activeSequence = null;
                // Hand off to MoveScrewDown which will create/track its own sequence/tween
                MoveScrewDown(holdScrew, onComplete);
            });
        }

        private void MoveScrewDown(HoldScrew holdScrew, Action onComplete)
        {
            if (renderTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Don't CancelActiveTweens() here because MoveScrewDown is only called after
            // the preceding sequence completes — but ensure previous references were cleared by callers.

            var targetPos = renderTransform.position - new Vector3(0, 0.18f);
            activeSequence = DOTween.Sequence();
            activeSequence.Append(renderTransform.DOMoveY(targetPos.y, 0.15f))
               .Join(renderTransform.DORotate(new Vector3(0, 0, -360f), 0.15f, RotateMode.FastBeyond360))
               .Join(renderTransform.DOScale(1f, 0.15f));

            activeSequence.OnComplete(() =>
            {
                activeSequence = null;
                ResetRenderLocalPosition(); // also cancels/clears any remaining tweens
                try
                {
                    if (onComplete != null && this != null && gameObject != null && isActiveAndEnabled)
                        onComplete.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ScrewAnimation] MoveScrewDown onComplete threw: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Stop any running tweens on the render and reset its local transform to identity.
        /// Use this instead of setting component.transform.position = Vector3.zero.
        /// </summary>
        public void ResetRenderLocalPosition()
        {
            if (renderTransform == null) return;

            // Kill tracked tweens first
            CancelActiveTweens();

            // Also kill DOTween's internal tweens targeting this transform to be defensive.
            renderTransform.DOKill();

            renderTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            renderTransform.localScale = Vector3.one;
        }

        public void OnReset()
        {
            ResetRenderLocalPosition();
        }
    }
}