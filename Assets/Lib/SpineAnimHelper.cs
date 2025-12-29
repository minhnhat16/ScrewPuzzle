using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public static class AnimationHelper
{
    /// <summary>
    /// Play a Spine SkeletonGraphic animation with an optional completion callback.
    /// Works for both loop and non-loop animations.
    /// </summary>
    public static void PlaySpineAnimation(
        SkeletonGraphic skeleton,
        string animationName,
        bool loop,
        UnityAction onComplete = null)
    {
        if (skeleton == null || skeleton.AnimationState == null)
        {
            Debug.LogWarning("AnimationHelper: SkeletonGraphic is null.");
            return;
        }

        var state = skeleton.AnimationState;
        var track = state.SetAnimation(0, animationName, loop);

        // If loop → không có completion callback
        if (loop || onComplete == null)
            return;

        // If NON-loop → listen for Complete event
        Spine.AnimationState.TrackEntryDelegate completeHandler = null;

        completeHandler = (entry) =>
        {
            if (entry.Animation.Name == animationName)
            {
                state.Complete -= completeHandler;
                onComplete?.Invoke();
            }
        };

        state.Complete += completeHandler;
    }
}
