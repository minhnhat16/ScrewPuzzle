using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class BasePart : MonoBehaviour, IBreakable, ITappable
    {
        public string uniqueID;
        private static readonly HashSet<string> usedIDs = new HashSet<string>();

        [Header("Components")]
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private SpriteRenderer activeSprite;
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] private PolygonCollider2D col;

        [Header("State")]
        [SerializeField] private bool isFalling;
        // Set bởi LayerVisibilityController — true = fully visible, false = prereview/hidden
        private bool _isBreakable = true;

        /// <summary>
        /// Trạng thái visibility hiện tại của part — set bởi LayerVisibilityController.
        /// Dùng để skip fade nếu state không đổi.
        /// </summary>
        public enum VisibilityState { Hidden, Prereview, FullyVisible }

        private VisibilityState _visibilityState = VisibilityState.Hidden;

        public VisibilityState CurrentVisibilityState
        {
            get => _visibilityState;
            set => _visibilityState = value;
        }

        public bool IsFalling => isFalling;

        /// <summary>
        /// Chỉ true khi layer đang ở trạng thái FullyVisible.
        /// Set bởi LayerVisibilityController.SetLayerFullyVisible / SetLayerPrereview / SetLayerHidden.
        /// </summary>
        public bool IsBreakableByItem
        {
            get => _isBreakable && gameObject.activeInHierarchy;
            set => _isBreakable = value;
        }

        public bool IsVisible => IsBreakableByItem; // alias cho rõ nghĩa ở OnTap/Break

        public UnityEvent<bool, BasePart> OnStateChanged = new();


        private IInteractionService _interactionService;
        //-----------------------------
        // PROPERTIES
        //-----------------------------
        public virtual Rigidbody2D Body
        {
            get => body;
            set => body = value;
        }

        public virtual SpriteRenderer Renderer
        {
            get => activeSprite;
            set => activeSprite = value;
        }

        public virtual PolygonCollider2D Collider
        {
            get => col;
            set => col = value;
        }
        public SpriteRenderer Outline { get => outline; set => outline = value; }

        public bool IsInteractable => true;

        public Transform Transform => transform;

        public bool canBreak => true;


        public void OnDisable()
        {
            //Debug.Log("ONDISABLE part " + uniqueID);    
        }

        public bool OnTap(Vector2 screenPosition)
        {
            if (IngameController.ins == null) return false;
            if (!IngameController.ins.IsItemExecutingBreaker) return false;

            // Part đang hidden hoặc prereview → không thể break
            if (!IsVisible)
            {
                Debug.Log($"[BasePart] OnTap blocked — part '{uniqueID}' is not visible (hidden or prereview).");
                return false;
            }

            var itemController = IngameController.ins.ItemController;
            itemController.RemovePartState.Perform(this, transform.position);

            return true;
        }

        //-----------------------------
        // UNITY LIFECYCLE
        //-----------------------------
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            activeSprite = GetComponentInChildren<SpriteRenderer>();
            col = GetComponent<PolygonCollider2D>();

            // Freeze ngay khi Awake — hinge chưa connect, không được apply gravity
            FreezeBody();

            if (string.IsNullOrEmpty(uniqueID))
                uniqueID = GenerateUniqueID();
        }

        /// <summary>
        /// Freeze hoàn toàn Rigidbody2D — dùng khi part chưa có hinge.
        /// Gọi từ Awake và Reset() để đảm bảo không bao giờ rơi trước khi sẵn sàng.
        /// </summary>
        public void FreezeBody()
        {
            if (body == null) return;
            body.gravityScale = 0;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0;
            body.bodyType = RigidbodyType2D.Kinematic;
        }

        /// <summary>
        /// Unfreeze sau khi hinge đã connect — gọi từ ScrewSpawnService.
        /// </summary>
        public void UnfreezeBody()
        {
            if (body == null) return;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 1; // vẫn giữ 0 — hinge giữ part, gravity do HandleNoHingesLeft set
        }

        //-----------------------------
        // FALLING LOGIC (STABLE VERSION)
        //-----------------------------
        public void UpdateFallingState()
        {
            if (body == null)
            {
                SetFalling(false);
                return;
            }

            bool falling = body.linearVelocity.y < -0.1f;

            if (falling != isFalling)
                SetFalling(falling);
        }

        private void SetFalling(bool newState)
        {
            isFalling = newState;

            Debug.Log("set falling part " + isFalling + " part id " + uniqueID);
            OnStateChanged?.Invoke(newState, this);

            if (newState)
                body.gravityScale = 1;
        }

        //-----------------------------
        // COLLISION / SORTING
        //-----------------------------
        public void SetIgnoreColliderLayer(bool isIgnoring, int idLayer, int idTargetLayer)
        {
            if (idLayer < 0 || idTargetLayer < 0)
                return;

            Physics2D.IgnoreLayerCollision(idLayer, idTargetLayer, isIgnoring);
        }

        public void SetSortingLayer(string layerName)
        {
            if (activeSprite != null)
            {
                activeSprite.sortingLayerName = layerName;
                activeSprite.sortingOrder = 0;
            }

            if (outline != null)
            {
                outline.sortingLayerName = layerName;
                outline.sortingOrder = 0;
            }
        }

        //-----------------------------
        // HINGE REMOVED
        //-----------------------------
        public void HandleNoHingesLeft()
        {
            if (body == null) return;

            body.gravityScale = 1;
            body.bodyType = RigidbodyType2D.Dynamic;

            int layer = gameObject.layer;
            SetIgnoreColliderLayer(false, layer, layer);

            Debug.Log("State part falling: " + isFalling + " part id " + uniqueID);


            OnStateChanged.Invoke(true, this);
        }

        //-----------------------------
        // COLLIDER GENERATION
        //-----------------------------
        public IEnumerator Init(SpriteRenderer render, Action callback = null)
        {
            yield return new WaitForSeconds(0.1f);

            activeSprite = render;
            col = GetComponent<PolygonCollider2D>();

            ResetAndReapplyPolygonCollider();

            gameObject.tag = "Part";
            callback?.Invoke();
        }

        public virtual void ResetAndReapplyPolygonCollider()
        {
            if (col == null || activeSprite == null || activeSprite.sprite == null)
                return;

            GenerateColliderFromSprite();
        }

        public virtual void GenerateColliderFromSprite(float scale = 1.02f)
        {
            if (activeSprite == null || col == null)
                return;

            var sprite = activeSprite.sprite;
            if (sprite == null)
                return;

            int shapeCount = sprite.GetPhysicsShapeCount();
            col.pathCount = shapeCount;

            List<Vector2> path = new();

            for (int i = 0; i < shapeCount; i++)
            {
                path.Clear();
                sprite.GetPhysicsShape(i, path);

                // ===== SCALE PATH =====
                for (int p = 0; p < path.Count; p++)
                {
                    path[p] *= scale;
                }

                col.SetPath(i, path);
            }
        }


        //-----------------------------
        // RESET
        //-----------------------------
        public void Reset()
        {
            isFalling = false;
            gameObject.name = "Part (Clone)";

            if (activeSprite != null)
                activeSprite.sprite = null;

            // Freeze lại khi return về pool
            FreezeBody();
            OnStateChanged.RemoveAllListeners();
        }

        //-----------------------------
        // UTILITIES
        //-----------------------------
        private string GenerateUniqueID()
        {
            string newID;

            do
            {
                newID = UnityEngine.Random.Range(1000, 9999).ToString();
            }
            while (usedIDs.Contains(newID));

            usedIDs.Add(newID);
            return newID;
        }

        public int PartLayer()
        {
            return gameObject.layer;
        }

        void OnCollisionStay2D(Collision2D col)
        {
            StartCoroutine(ApplyForceAfter(2f));
        }

        private IEnumerator ApplyForceAfter(float v)
        {
            //if (!isFalling) yield break;
            yield return new WaitForSeconds(v);
            body.AddForceAtPosition(Vector2.up, transform.position);

        }
        public void Break()
        {
            // Guard: không break part đang hidden hoặc prereview
            if (!IsVisible)
            {
                Debug.LogWarning($"[BasePart] Break blocked — part '{uniqueID}' is not visible.");
                return;
            }

            Debug.Log("Break part: " + uniqueID);
            gameObject.SetActive(false);
        }

        internal void SetSpriteAlpha(float v)
        {
            if (activeSprite != null)
            {
                activeSprite.color = new Color(activeSprite.color.r, activeSprite.color.g, activeSprite.color.b, v);
            }
        }
    }
}
