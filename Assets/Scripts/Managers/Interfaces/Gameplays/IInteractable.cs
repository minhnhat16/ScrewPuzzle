using UnityEngine;

/// <summary>
/// Lightweight touch-driven interaction contract.
/// Implement this on objects that should respond to simple touch gestures
/// (begin/move/end, tap, long-press). Methods return bool when the implementation
/// may consume the event (true = consumed).
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// True when object can currently be interacted with.
    /// </summary>
    bool IsInteractable { get; }

    /// <summary>
    /// Expose transform for anchoring UI / tutorial targets.
    /// </summary>
    Transform Transform { get; }

    /// <summary>
    /// Called when a touch/finger/pointer first presses on the object.
    /// Return true to indicate the event was consumed.
    /// </summary>
    bool OnTouchBegin(Vector2 screenPosition);

    /// <summary>
    /// Called while the touch/finger/pointer moves (drag).
    /// </summary>
    void OnTouchMove(Vector2 screenPosition);

    /// <summary>
    /// Called when the touch/finger/pointer is released.
    /// </summary>
    void OnTouchEnd(Vector2 screenPosition);

    /// <summary>
    /// Called when a quick tap is detected. Return true to consume.
    /// </summary>
    bool OnTap(Vector2 screenPosition);

    /// <summary>
    /// Called when a long-press gesture is detected.
    /// </summary>
    void OnLongPress(Vector2 screenPosition);

    /// <summary>
    /// Called to cancel a pending interaction (e.g. pointer left, interrupted).
    /// </summary>
    void OnCancel();
}