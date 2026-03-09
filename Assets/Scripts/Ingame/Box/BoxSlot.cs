using DG.Tweening;
using System;
using UnityEngine;

namespace Ingame
{
    public class BoxSlot : MonoBehaviour, ITappable
    {
        public Box screwBox;
        public Vector3 initialPosition;
        public bool isLocked;
        public bool isContainingBox;

        [Header("Lock Visual")]
        [SerializeField] private GameObject lockVisual;
        [SerializeField] private Collider2D tapCollider; // assign 2D collider trên slot để InputRouter detect

        /// <summary>
        /// Fire khi player tap vào slot đang bị lock.
        /// BoxQueue lắng nghe để show AddBoxDialog.
        /// </summary>
        public event Action<BoxSlot> OnLockedSlotTapped;

        public void Initialize(Vector3 position, bool locked, Box box = null)
        {
            initialPosition = position;
            transform.position = initialPosition;
            SetLocked(locked);
        }

        // ─── ITappable ─────────────────────────────────────────────

        public bool IsInteractable => isLocked;
        public Transform Transform => transform;

        public bool OnTap(Vector2 screenPosition)
        {

            Debug.Log($"[BoxSlot] Tapped at screen position: {screenPosition}. IsLocked: {isLocked}");
            if (!isLocked) return false;

            OnLockedSlotTapped?.Invoke(this);
            return true; // consumed — không forward xuống game logic
        }

        // ─── Lock State ────────────────────────────────────────────

        public void SetLocked(bool locked)
        {
            isLocked = locked;
            if (lockVisual != null)
                lockVisual.SetActive(locked);

            // Enable/disable collider theo trạng thái lock
            if (tapCollider != null)
                tapCollider.enabled = locked;
        }

        public void UnlockSlot()
        {
            SetLocked(false);
        }

        // ─── Box Containment ───────────────────────────────────────

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

        public void RemoveBox()
        {
            screwBox = null;
            isContainingBox = false;
        }

        public bool CheckIsContainingThisBox(Box screwBox)
        {
            return this.screwBox == screwBox;
        }
    }
}