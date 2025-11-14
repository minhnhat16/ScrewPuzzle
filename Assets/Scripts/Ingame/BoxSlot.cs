using DG.Tweening;
using System;
using UnityEngine;

// For animations

namespace Ingame
{
    public class BoxSlot : MonoBehaviour
    {
       
        public Box screwBox; // Reference to the box
        public Vector3 initialPosition; // Position to move to
        public bool isLocked;
        public bool isContainingBox;
        public void Initialize(Vector3 position, bool locked, Box box = null)
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

        public void AddBox(Box box)
        {
            isContainingBox = box != null;  
            if (!isContainingBox) return;
            screwBox = box;
        }

        public bool CheckIsContainingThisBox(Box screwBox)
        {
            return this.screwBox == screwBox;
        }

        
    }
}