using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

namespace Ingame
{
    public class BasePart : MonoBehaviour
    {
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

        public virtual Collider2D Collider
        {
            get => collider;
            set => collider = value;
        }

        [SerializeField] private bool isFalling;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private SpriteRenderer renderer;
        [SerializeField] private SpriteRenderer outLine;
        [SerializeField] private Collider2D collider;
        private Coroutine checkFallingRoutine;
        public bool IsFalling
        {
            get => isFalling;
            private set => isFalling = value;
        }

        public SpriteRenderer OutLine => outLine;

        public Action OnStateChanged;
        public BasePart(Rigidbody2D body, SpriteRenderer renderer, Collider2D collider)
        {
            this.body = body;
            this.renderer = renderer;
            this.collider = collider;
        }
        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            outLine = transform.GetChild(0).GetComponent<SpriteRenderer>();
            collider = GetComponent<Collider2D>();
        }

        // Start is called before the first frame update
        private void Start()
        {
            StartFallingCheck(); 
        }

    
        public void StartFallingCheck()
        {
            if (checkFallingRoutine == null)
            {
                checkFallingRoutine = StartCoroutine(CheckFalling());
            }
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
    }
}
