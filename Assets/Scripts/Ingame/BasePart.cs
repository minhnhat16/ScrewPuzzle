using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.U2D.Path;
using UnityEngine;
using Random = Unity.Mathematics.Random;

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
            get => renderer;
            set => renderer = value;
        }

        public virtual PolygonCollider2D Collider
        {
            get => collider;
            set => collider = value;
        }

        [SerializeField] private bool isFalling;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private SpriteRenderer renderer;
        [SerializeField] private SpriteRenderer outLine;
        [SerializeField] private PolygonCollider2D collider;
        
        private Coroutine checkFallingRoutine;
        public bool IsFalling
        {
            get => isFalling;
            private set => isFalling = value;
        }
        public SpriteRenderer OutLine => outLine;

        public Action OnStateChanged;
        public BasePart(Rigidbody2D body, SpriteRenderer renderer, PolygonCollider2D collider)
        {
            this.body = body;
            this.renderer = renderer;
            this.collider = collider;
        }
        public int PartLayer()
        {
            return gameObject.layer;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            outLine = transform.GetChild(0).GetComponent<SpriteRenderer>();
            collider = GetComponent<PolygonCollider2D>();
            // Assign a GUID if not already set
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = GenerateUniqueID();
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            StartFallingCheck(); 
        }

        public IEnumerator Init(SpriteRenderer render, Action callBack = null)
        {
            yield return new WaitForSeconds(0.125f);
            collider = GetComponent<PolygonCollider2D>();
            collider.pathCount = 0;
            this.renderer = render;
            collider.SetPath(0,renderer.sprite.vertices);
        }

    
        private void SetUpCollider()
        {
            
        }
        public void StartFallingCheck()
        {
            checkFallingRoutine ??= StartCoroutine(CheckFalling());
        }

        // Coroutine để kiểm tra trạng thái rơi
        private IEnumerator CheckFalling()
        {
            while (true)
            {
                bool wasFalling = isFalling;
                isFalling = body.velocity.y < -5;

                // Nếu trạng thái thay đổi, kích hoạt sự kiện
                if (isFalling != wasFalling)
                {
                    OnStateChanged?.Invoke();
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
            // Ví dụ: đổi trạng thái, cập nhật UI, hoặc xóa đối tượng
            body.gravityScale = 1;
            body.bodyType = RigidbodyType2D.Dynamic;
        }
        public void SetIgnoreColliderLayer(bool isIgnoring, int idLayer, int idTargetLayer)
        {

            if (idLayer < 0 || idTargetLayer < 0)
            {
                Debug.LogWarning($"Layer {idLayer} hoặc {idTargetLayer} không tồn tại.");
                return;
            }

            Physics2D.IgnoreLayerCollision(idLayer, idLayer, isIgnoring);

            Debug.Log($"Đã {(isIgnoring ? "bỏ qua" : "kích hoạt")} va chạm giữa lớp {idLayer} và {idTargetLayer}.");
        }
        public void SetSortingLayer(string layerName)
        {
            Debug.Log("sorting layer name " + layerName + "sortinglayer name" + renderer.sortingLayerName);

            if (renderer != null)
            {
                renderer.sortingLayerName = layerName;
            }

            if (outLine != null)
            {
                outLine.sortingLayerName = layerName; // Both sprites use the same sorting layer
            }
            
            Debug.Log("After sorting layer name " + layerName + "sortinglayer name" + renderer.sortingLayerName);

            renderer.sortingOrder = 0;
            outLine.sortingOrder = renderer.sortingOrder+1; 
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
            collider.pathCount = 0;

            // Generate a new shape for the polygon collider from the sprite
            GenerateColliderFromSprite();
        }
            
        public  virtual void GenerateColliderFromSprite()
        {
            // Use the sprite's texture to define the polygon's points
            // This method is for auto-generating a polygon collider based on the sprite's shape
            collider.enabled = false; // Disable the collider temporarily to prevent issues
            Destroy(collider);        // Destroy the old collider

            // Add and create a new PolygonCollider2D
            collider = gameObject.AddComponent<PolygonCollider2D>();
            collider.enabled = true;

        }
        public void Reset()
        {
            isFalling = false;
            renderer.sprite = null;
            
        }
    }
}
