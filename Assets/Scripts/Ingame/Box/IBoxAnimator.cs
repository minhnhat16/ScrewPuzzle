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
}