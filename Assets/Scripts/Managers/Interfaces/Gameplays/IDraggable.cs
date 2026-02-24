using UnityEngine;

public interface IDraggable
{
    bool IsInteractable { get; }
    Transform Transform { get; }
    bool OnTouchBegin(Vector2 screenPosition);
    void OnTouchMove(Vector2 screenPosition);
    void OnTouchEnd(Vector2 screenPosition);
    void OnCancel();
}
