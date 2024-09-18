using UnityEngine;
using DG.Tweening;
using Ingame; // For animations

public class BoxSlot : MonoBehaviour
{
    public ScrewBox screwBox; // Reference to the box
    public Vector3 initialPosition; // Position to move to
    public bool isLocked;

    public void Initialize(Vector3 position, bool locked)
    {
        initialPosition = position;
        isLocked = locked;
        transform.position = initialPosition;
        /*screwBox.OnInit(position, locked);*/
    }

    public void ActivateBox()
    {
        gameObject.SetActive(true);
        // Additional activation logic
    }

    public void DeactivateBox(Vector3 moveToPosition, System.Action onComplete = null)
    {
        // Animate box deactivation or other behavior
        screwBox.CloseBox(isAnimDone =>
        {
            if (isAnimDone)
            {
                // Animation or movement when deactivated
                var t = screwBox.transform.DOMove(moveToPosition, 1f).SetEase(Ease.OutCirc);
                t.OnComplete(() => onComplete?.Invoke());
            }
        });
    }

    public void MoveToPosition(Vector3 newPosition, float duration = 1f, System.Action onComplete = null)
    {
        // Move the box to a new position smoothly
        screwBox.transform.DOMove(newPosition, duration).OnComplete(() => onComplete?.Invoke());
    }
}