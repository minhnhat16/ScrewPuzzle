using Ingame.Screw;
using UnityEngine;

/// <summary>
/// Adapter that exposes only the interaction surface required by game logic.
/// Wraps the concrete ScrewController (composition) so the ScrewController itself
/// does not have to implement every gesture interface if it doesn't need to.
/// </summary>
public class ScrewInteractableAdapter : MonoBehaviour, ITappable, IDraggable
{
    [SerializeField] private ScrewController screw; // optional in inspector

    private ITappable tappableImpl;
    private IDraggable draggableImpl;

    private void Awake()
    {
        // auto-assign if not set in inspector (common case for prefab children)
        if (screw == null)
            screw = GetComponent<ScrewController>() ?? GetComponentInParent<ScrewController>();

        // cache available interfaces on the concrete screw
        tappableImpl = screw as ITappable;
        draggableImpl = screw as IDraggable;
    }

    public bool IsInteractable => (tappableImpl != null ? tappableImpl.IsInteractable : screw != null && screw.enabled);

    public Transform Transform => screw != null ? screw.transform : transform;

    public bool OnTap(Vector2 screenPosition)
    {
        if (tappableImpl != null)
            return tappableImpl.OnTap(screenPosition);

        if (screw == null) return false;
        // fallback to legacy method
        return screw.OnScrewClicked();
    }

    public bool OnTouchBegin(Vector2 screenPosition)
    {
        if (draggableImpl != null) return draggableImpl.OnTouchBegin(screenPosition);
        return false;
    }

    public void OnTouchMove(Vector2 screenPosition)
    {
        draggableImpl?.OnTouchMove(screenPosition);
    }

    public void OnTouchEnd(Vector2 screenPosition)
    {
        draggableImpl?.OnTouchEnd(screenPosition);
    }

    public void OnCancel()
    {
        draggableImpl?.OnCancel();
    }
}