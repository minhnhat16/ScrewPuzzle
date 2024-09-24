using UnityEngine;
using DG.Tweening;
using Ingame;
using Ingame.Screw; // For animations

public class BoxSlot : MonoBehaviour
{
    public ScrewBox screwBox; // Reference to the box
    public Vector3 initialPosition; // Position to move to
    public bool isLocked;
    public bool isContainingBox;
    public void Initialize(Vector3 position, bool locked, ScrewBox box = null)
    {
        initialPosition = position;
        isLocked = locked;
        transform.position = initialPosition;
        /*screwBox.OnInit(position, locked);*/
    }

    public void ActivateBox()
    {
        gameObject.SetActive(true);
    }

    public void AddBox(ScrewBox box)
    {
        isContainingBox = box != null;  
        if (!isContainingBox) return;
        Debug.LogError("Added box to slot" + box.name);
        screwBox = box;
    }

    public bool CheckIsContainingThisBox(ScrewBox screwBox)
    {
        return this.screwBox == screwBox;
    }
    public void DeactivateBox(Vector3 moveToPosition, System.Action onComplete = null)
    {
        // Animate box deactivation or other behavior
        screwBox.CloseBox(isAnimDone =>
        {
            if (!isAnimDone) return;
            // Animation or movement when deactivated
            var t = screwBox.transform.DOMove(moveToPosition, 1f).SetEase(Ease.OutCirc);
            t.OnComplete(() => onComplete?.Invoke());
        });
    }

    public void MoveToPosition(Vector3 newPosition, float duration = 1f, System.Action onComplete = null)
    {
        // Move the box to a new position smoothly
        screwBox.transform.DOMove(newPosition, duration).OnComplete(() => onComplete?.Invoke());
    }
}