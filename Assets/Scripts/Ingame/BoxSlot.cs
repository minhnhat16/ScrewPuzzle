using DG.Tweening;
using UnityEngine;

// For animations

namespace Ingame
{
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
            //Debug.LogError("Added box to slot" + box.name);
            screwBox = box;
        }

        public bool CheckIsContainingThisBox(ScrewBox screwBox)
        {
            return this.screwBox == screwBox;
        }

        
    }
}