using UnityEngine;

public interface ITappable
{
    bool IsInteractable { get; }
    Transform Transform { get; }
    bool OnTap(Vector2 screenPosition);
}