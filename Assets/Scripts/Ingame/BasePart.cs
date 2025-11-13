
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.AI;
namespace Ingame
{
    public class BasePart : MonoBehaviour
    {
        public string uniqueID;
        private static HashSet<string> usedIDs = new HashSet<string>(); // To track used IDs
        private List<HingeJoint> joints = new List<HingeJoint>();

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

        [SerializeField] private bool isFalling;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private SpriteRenderer activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private PolygonCollider2D col;



        private Coroutine checkFallingRoutine;
        public bool IsFalling
        {
            get => isFalling;
            private set => isFalling = value;
        }
        public Sprite OutLine => inactiveSprite;


        public UnityEvent<bool,BasePart> OnStateChanged = new();
        public BasePart(Rigidbody2D body, SpriteRenderer renderer, PolygonCollider2D collider)
        {
            this.body = body;
            this.activeSprite = renderer;
            this.col = collider;
        }
        public int PartLayer()
        {
            return gameObject.layer;
        }

        public void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            activeSprite = GetComponentInChildren<SpriteRenderer>();
            col = GetComponent<PolygonCollider2D>();
            // Assign a GUID if not already set
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = GenerateUniqueID();
            }

            // Immediately evaluate falling state on Awake
            UpdateFallingState();
        }

        // Start is called before the first frame update
        public void Start()
        {
            // Ensure the checking coroutine is running (StartFallingCheck uses null-coalescing so it's safe)
            StartFallingCheck();
        }

        public IEnumerator Init(SpriteRenderer render, Action callBack = null)
        {
            yield return new WaitForSeconds(0.125f);
            col = GetComponent<PolygonCollider2D>();
            col.pathCount = 0;
            this.activeSprite = render;
            col.SetPath(0, this.activeSprite.sprite.vertices);
        }


        private void SetUpCollider()
        {

        }

        // Public helper to perform a single immediate falling-state check and fire event if changed.
        public void UpdateFallingState()
        {
            bool wasFalling = isFalling;
            if (body != null)
            {
                isFalling = body.linearVelocity.y < -5f;
            }
            else
            {
                isFalling = false;
            }

            if (isFalling != wasFalling)
            {

                Debug.Log("Falling " + isFalling);
                OnStateChanged?.Invoke(isFalling,this);
            }
        }

        public void StartFallingCheck()
        {
            Debug.Log($"[{name}] StartFallingCheck called. routine={checkFallingRoutine}");
            if (checkFallingRoutine == null)
            {
                checkFallingRoutine = StartCoroutine(CheckFalling());
                Debug.Log($"[{name}] Coroutine started!");
            }
        }

        // Coroutine để kiểm tra trạng thái rơi
        private IEnumerator CheckFalling()
        {
            while (true)
            {
                bool wasFalling = isFalling;
                isFalling = (body != null) && body.linearVelocity.y < -5f;

                // Nếu trạng thái thay đổi, kích hoạt sự kiện
                if (isFalling != wasFalling)
                {

                    Debug.Log("Falling " + isFalling);
                    OnStateChanged?.Invoke(isFalling,this);
                }

                // Điều chỉnh thời gian chờ giữa các lần kiểm tra, có thể thay đổi thời gian cho phù hợp
                yield return new WaitForSeconds(0.1f); // Kiểm tra sau mỗi 0.1 giây
            }
        }
        // Hàm dừng Coroutine kiểm tra trạng thái rơi
        public void StopFallingCheck()
        {
            if (checkFallingRoutine != null)
            {
                StopCoroutine(checkFallingRoutine);
                checkFallingRoutine = null;
            }
        }
        public void HandleNoHingesLeft()
        {
            var partLayer = PartLayer();
            SetIgnoreColliderLayer(false, partLayer, partLayer);
            // Ví dụ: đổi trạng thái, cập nhật UI, hoặc xóa đối tượng
            body.gravityScale = 1;
            body.bodyType = RigidbodyType2D.Dynamic;

        }

        public void SetIgnoreColliderLayer(bool isIgnoring, int idLayer, int idTargetLayer)
        {

            if (idLayer < 0 || idTargetLayer < 0)
            {
                //Debug.LogWarning($"Layer {idLayer} hoặc {idTargetLayer} không tồn tại.");
                return;
            }

            Physics2D.IgnoreLayerCollision(idLayer, idLayer, isIgnoring);

            //Debug.Log($"Đã {(isIgnoring ? "bỏ qua" : "kích hoạt")} va chạm giữa lớp {idLayer} và {idTargetLayer}.");
        }
        public void SetSortingLayer(string layerName)
        {
            //Debug.Log("sorting layer name " + layerName + "sortinglayer name" + render.sortingLayerName);

            if (activeSprite != null)
            {
                activeSprite.sortingLayerName = layerName;
            }

            //if (inactiveSprite != null)
            //{
            //    inactiveSprite.sortingLayerName = layerName; // Both sprites use the same sorting layer
            //}

            //Debug.Log("After sorting layer name " + layerName + "sortinglayer name" + render.sortingLayerName);

            activeSprite.sortingOrder = 0;
            //inactiveSprite.sortingOrder = activeSprite.sortingOrder+1; 
        }
        private string GenerateUniqueID()
        {
            string newID;
            do
            {
                newID = UnityEngine.Random.Range(1000, 9999).ToString(); // Generate a random 4-digit number
            } while (usedIDs.Contains(newID)); // Ensure the ID is unique
            usedIDs.Add(newID); // Mark the ID as used
            return newID;
        }
        public virtual void ResetAndReapplyPolygonCollider()
        {
            // Reset the collider by clearing all paths
            col.pathCount = 0;

            // Generate a new shape for the polygon collider from the sprite
            GenerateColliderFromSprite();
        }

        public virtual void GenerateColliderFromSprite()
        {
            var sprite = activeSprite.sprite;

            if (activeSprite == null || col == null || sprite == null)
                return;

            // Xoá các path cũ
            col.pathCount = sprite.GetPhysicsShapeCount();

            // Copy physics shape từ sprite sang collider
            List<Vector2> path = new List<Vector2>();
            for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
            {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                col.SetPath(i, path);
            }

        }
        public void Reset()
        {
            isFalling = false;
            activeSprite.sprite = null;

        }
    }
}