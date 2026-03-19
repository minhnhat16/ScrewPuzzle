using Core.Match;
using DG.Tweening;
using Enums;
using Level;
using System;
using System.Collections;
using System.Drawing;
using UnityEngine;

namespace Ingame.Screw
{
    [RequireComponent(typeof(ScrewPhysics), typeof(ScrewRender), typeof(ScrewAnimation))]
    public class ScrewController : MonoBehaviour, IResetable, ITappable, IMatchItem
    {
        [Header("References")]
        [SerializeField] internal ScrewPhysics screwPhysics;
        [SerializeField] internal ScrewRender screwRender;
        [SerializeField] internal ScrewAnimation screwAnimation;
        [SerializeField] internal HingeController hingeController;

        [Header("State Flags")]
        [SerializeField] private bool isClicked;
        [SerializeField] private bool isInHold;
        [SerializeField] private bool isMoving;
        [SerializeField] private bool isShaking;

        // NEW: separate reservation flag for movement so clicks and move-locks don't collide
        private bool isReservedForMove;
        public bool IsInHold => isInHold;
        public bool IsMoving => isMoving;
        public bool IsActionComplete { get; private set; }
        public bool IsClicked { get => isClicked; set => isClicked = value; }

        public bool IsInteractable => enabled && gameObject.activeInHierarchy && !isInHold && !isMoving;

        // Expose transform for targeting / UI anchoring
        public Transform Transform => transform;

        private Transform _transform;
        internal string tutorialKey;

        internal int GetSortingOrder()
        {
            if (screwRender == null)
            {
                Debug.LogWarning($"[ScrewController] {name}: ScrewRender reference is missing!");
                return 0;
            }
            return screwRender.GetSortingOrder();
        }
        internal string GetSortingLayerName()
        {
            return screwRender != null ? screwRender.GetSortingLayer() : string.Empty;
        }


        public void OnDisable()
        {
            Debug.Log("Screw on disable: " + name);

        }
        private void Awake()
        {
            _transform = transform;

            if (!screwPhysics) screwPhysics = GetComponent<ScrewPhysics>();
            if (!screwRender) screwRender = GetComponent<ScrewRender>();
            if (!screwAnimation) screwAnimation = GetComponent<ScrewAnimation>();
        }

        private void Start()
        {
            StartCoroutine(Init());
        }

        internal IEnumerator Init()
        {
            IsActionComplete = false;   
            string bodyLayer = hingeController.GetConnectedBodyRenderLayer(0);
            yield return new WaitUntil(() => bodyLayer != null);
            screwRender.SetSortingOrderAndLayer(0, bodyLayer);
            hingeController.InitHingeJoints();
        }

        public bool OnScrewClicked()
        {
            if (isClicked) return true;

            isClicked = true;

            if (screwPhysics.IsBlocked())
            {
                isShaking = true;
                screwAnimation.Shake(() =>
                {
                    isShaking = false;
                    ResetClickedFlag();
                });
                return true;
            }

            return false;
        }

        // Attempts to reserve this screw for a move into the array/box.
        // Returns true if reserved (caller may proceed). Returns false if already reserved or blocked.
        public bool TryLockForMove()
        {
            // already reserved for another move
            if (isReservedForMove) return false;

            // if physics blocks interaction, show feedback and do not reserve
            if (screwPhysics != null && screwPhysics.IsBlocked())
            {
                isShaking = true;
                screwAnimation.Shake(() =>
                {
                    isShaking = false;
                    ResetClickedFlag();
                });
                Debug.Log($"[ScrewController] {name} is blocked! Cannot reserve for move.");
                return false;
            }

            // reserve and mark clicked to block further taps until released/reset
            isReservedForMove = true;
            isClicked = true;
            return true;
        }

        // Release reservation when move is finished or cancelled
        public void ReleaseLockForMove()
        {
            isReservedForMove = false;
        }

        public void MoveToHold(HoldScrew holdScrew, bool isTele = false, Action callback = null)
        {
            isClicked = isMoving = true;
            isInHold = true;

            if (isTele)
            {
                screwPhysics.DisableCollider();
                screwPhysics.FreeHinge();
                _transform.SetParent(holdScrew.transform);
                _transform.localPosition = Vector3.zero;
                _transform.localScale = Vector3.one; // Ensure scale is reset
                isReservedForMove = false;
                callback?.Invoke();
                return;
            }

            screwAnimation.MoveScrewUp(() =>
            {
                screwPhysics.DisableCollider();
                screwPhysics.FreeHinge();
                screwAnimation.JumpScrewToHold(holdScrew, () =>
                {
                    _transform.SetParent(holdScrew.transform, worldPositionStays: false);
                    _transform.localPosition = Vector3.zero;
                    _transform.localScale = Vector3.one; // Ensure scale is reset
                    isMoving = false;
                    isReservedForMove = false;
                    callback?.Invoke();
                    Debug.Log($"[ScrewController] MoveToHold complete for {name}");
                });
            });
        }

        public void FreeHinge()
        {
            if (screwPhysics != null)
                screwPhysics.FreeHinge();
        }

        public void ResetClickedFlag() => StartCoroutine(ResetClickFlagAfterDelay(0.5f));

        private IEnumerator ResetClickFlagAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isClicked = false;
            // also clear reservation to allow next attempts
            isReservedForMove = false;
            screwPhysics.EnableCollider();
        }

        internal void SetSortingOrderAndLayer(int order, string layer)
        {
            Debug.Log("[Screw controller ] Set sorting order and layer: " + name + " order: " + order + " layer: " + layer);
            screwRender.SetSortingOrderAndLayer(order, layer);
        }

        public void OnReset()
        {
            isClicked = isInHold = isMoving = isShaking = false;
            IsActionComplete = true;
            screwRender.ResetRender();
            screwPhysics.ResetPhysics();
            screwAnimation.OnReset();
           // hingeController.Reset();
            if (_transform == null) _transform = transform;
            _transform.localScale = Vector3.one; // Always reset scale

        }

        public ColorEnum GetColor()
        {
            return screwRender != null ? screwRender.Color : ColorEnum.Clear;
        }

        public void ChangeScrewColor(ColorEnum color)
        {
            screwRender.SetSpriteBy(color);
        }

        public void ChangeScrewColor(int color)
        {
            ColorEnum colorEnum = (ColorEnum)color;
            screwRender.SetSpriteBy(colorEnum);
        }

        public virtual HingeJoint2D CreateHinge(Rigidbody2D targetPart, HingeConnection connection)
        {
            // Spawn hinge object from pool
            HingeObject hinge = HingePool.Instance.pool.SpawnNonGravity();
            GameObject newHingeChild = hinge.gameObject;

            // Parent and position hinge child relative to screw
            newHingeChild.transform.SetParent(transform);
            newHingeChild.transform.SetLocalPositionAndRotation(connection.hingePosition, Quaternion.identity);

            // Ensure the hinge has a HingeJoint2D component and use the HingeObject wrapper
            if (hinge.HingeJoint2D == null)
            {
                hinge.HingeJoint2D = newHingeChild.GetComponent<HingeJoint2D>();
            }

            // Ensure a Rigidbody2D exists on the hinge child (kinematic so it doesn't respond to physics)
            if (!newHingeChild.TryGetComponent<Rigidbody2D>(out var hingeBody))
            {
                hingeBody = newHingeChild.AddComponent<Rigidbody2D>();
            }
            else
            {
                Debug.Log("[Warning] Hinge child already has Rigidbody2D component. Reusing existing component.");
            }
            hingeBody.bodyType = RigidbodyType2D.Kinematic;

            // Wire the hinge joint to the target part (guard targetPart == null)
            if (hinge.HingeJoint2D != null)
            {
                if(targetPart == null)
                {
                    Debug.LogWarning($"[ScrewController] {name}: Attempting to create hinge with null targetPart! Hinge will not be connected.");
                }
                else
                {
                    hinge.HingeJoint2D.connectedBody = targetPart;
                    hinge.HingeJoint2D.autoConfigureConnectedAnchor = true;
                }
            }

            if (hingeController != null)
            {
                hingeController.HingeJoint2D = hinge.HingeJoint2D;
                hingeController.BodyConnect = targetPart;
            }


            return hinge.HingeJoint2D;
        }

        public bool OnTap(Vector2 screenPosition)
        {
            if (!IsInteractable)
                return false;

            bool consumedLocally = OnScrewClicked();

            if (consumedLocally)
                return true;

            return false;
        }

        internal void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
            screwRender.SetActive(isActive);
        }

        public void EnableColliderAndRig(bool isEnabled)
        {
            if (screwPhysics != null)
            {
                if (isEnabled)
                {
                    screwPhysics.EnableCollider();
                }
                else
                {
                    screwPhysics.DisableCollider();
                }
            }
        }

        internal void ResetHoldState()
        {
            isInHold = false;
            isMoving = false;
            isClicked = false;
            isReservedForMove = false;
            screwPhysics.EnableCollider();
        }

        #region IMatchItem

        // Explicit implementation — không làm ô nhiễm public API của ScrewController
        // Caller dùng IMatchItem interface để access, không gọi thẳng từ ScrewController

        public string Tag
            => GetColor().ToString().ToLower();       // "red", "blue", "rainbow"...

        Vector3 IMatchItem.Position
            => transform.position;

        Transform IMatchItem.Transform
            => transform;

        bool Core.Match.IMatchItem.IsInteractable
            => IsInteractable;                        // đã có, tái sử dụng

        #endregion
    }

}