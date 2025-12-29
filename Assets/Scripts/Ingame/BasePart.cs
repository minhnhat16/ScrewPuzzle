using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class BasePart : MonoBehaviour
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

        public bool IsFalling => isFalling;

        public UnityEvent<bool, BasePart> OnStateChanged = new();

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

        //-----------------------------
        // UNITY LIFECYCLE
        //-----------------------------
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            activeSprite = GetComponentInChildren<SpriteRenderer>();
            col = GetComponent<PolygonCollider2D>();

            // Unique ID
            if (string.IsNullOrEmpty(uniqueID))
                uniqueID = GenerateUniqueID();

        }

        private void Start()
        {
            // No coroutine needed — using FixedUpdate instead
        }

        private void FixedUpdate()
        {
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

            bool falling = body.linearVelocity.y <=0f;

            if (falling != isFalling)
                SetFalling(falling);
        }

        private void SetFalling(bool newState)
        {
            isFalling = newState;
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

        public virtual void GenerateColliderFromSprite()
        {
            if (activeSprite == null || col == null || activeSprite.sprite == null)
                return;

            var sprite = activeSprite.sprite;
            int shapeCount = sprite.GetPhysicsShapeCount();

            col.pathCount = shapeCount;

            List<Vector2> path = new();
            for (int i = 0; i < shapeCount; i++)
            {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                col.SetPath(i, path);
            }
        }

        //-----------------------------
        // RESET
        //-----------------------------
        public void Reset()
        {
            isFalling = false;

            if (activeSprite != null)
                activeSprite.sprite = null;

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.gravityScale = 0;
            }
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
            //Debug.Log("Stay: vẫn đang chạm");

            StartCoroutine(ApplyForceAfter(2f));
        }

        private IEnumerator ApplyForceAfter(float v)
        {
            //if (!isFalling) yield break;
            yield return new WaitForSeconds(v);
            body.AddForceAtPosition(Vector2.up,transform.position);

        }
    }
}
