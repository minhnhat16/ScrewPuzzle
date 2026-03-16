using System;
using UnityEngine;

public interface IBoxAnimator
{
    /// <summary>
    /// Play close animation (upper box falls and bounces).
    /// Invokes onComplete when animation finishes.
    /// </summary>
    void PlayCloseAnimation(Action onComplete = null);

    /// <summary>
    /// Cartoon exit: squash bounce → fly to top-right corner → SetActive(false) + reset.
    /// Invokes onComplete before deactivating.
    /// </summary>
    void PlayExitAnimation(Action onComplete = null);

    /// <summary>
    /// Kill tất cả tween/animation đang pending — gọi khi box được reuse từ pool.
    /// Tránh stale callback từ level cũ fire vào box mới.
    /// </summary>
    void KillAllAnimations();
}