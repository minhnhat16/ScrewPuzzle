using System;
using System.Collections;
using System.Linq;
using ConfigFile;
using DG.Tweening;
using Enum;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

namespace Ingame
{
    public class ScrewBox : FSMSystem   
    {
        public BoxConfig config; // ScriptableObject chứa cấu hình cho CrewBox
        [SerializeField] private SpriteRenderer render;
        [SerializeField] private SpriteRenderer renderUpper;
        [SerializeField] private Transform transform;
        [SerializeField] private Vector3 position;
        [SerializeField] private bool isBoxFull;
        [SerializeField] private int nextEmptyIndex = -1;
        [SerializeField] public HoldScrew[] holdScrews; // Mảng các lỗ Screw
        [SerializeField] public UnityEvent<bool> onScrewBoxFull;
        [SerializeField] private ColorEnum color;

        public Animator animator;
        public float moveDuration = 1f;  // Thời gian di chuyển nắp
        public float squashAmount = 0.9f;  // Độ co giãn
        public float squashDuration = 0.2f;
        public bool IsBoxFull
        {
            get => isBoxFull;
            set => isBoxFull = value;
        }
        public ColorEnum Color {get => color;
            set => color=value;
        }
        public SpriteRenderer Render {get => render;
            set => render = value;
        }
        private void OnEnable()
        {
        }
       public virtual void Start()
        {
           // / render = GetComponentIN<SpriteRenderer>();
            transform = transform.GetComponent<Transform>(); 
            position = transform.position;
            SetBoxColor(UnityEngine.Color.white);
            animator = GetComponent<Animator>();
            // Debug.Log("Initialized CrewBox with " + config.numberOfScrewHoles + " screw holes.");
        }

        // Hàm kiểm tra xem các lỗ trong CrewBox có đầy đủ Screw không
        public bool AreAllHolesFilled()
        {
            return holdScrews.All(screw => !screw.IsEmpty());
        }
        // khi box đầy thuc hien ham sau 
        protected virtual void BoxFullInvoker(bool isFull)
        {
            Debug.Log("Box full invoker " + gameObject.name  + "\t" + isBoxFull);

            if (isFull)
            {
                // set box active fasle
                Debug.Log("Box full invoker " + gameObject.name );
                StartCoroutine(DeactiveBoxCouroutine());
            }
        }

        IEnumerator DeactiveBoxCouroutine()
        {
           
            yield return new WaitForSeconds(2f);
            BoxQueue.instance.DeactivateAndMoveQueue(this);
        }

        public void CloseBox()
        {
            DoUpperBoxMove((boxFull)=>
            {
                if (!boxFull) return;
            });
        }

        private void TunOffScrews()
        {
            
            Debug.Log("Turn off Screww");
            foreach (var t in holdScrews)
            {
                t.Screw.gameObject.SetActive(false);
            }
        }
        private void DoUpperBoxMove(Action<bool> callback)
        {
            animator.Play("BoxPunch");
            OnAnimStop(callback);
        }

        public virtual void  SetBoxInActive()
        {
            gameObject.SetActive(false);
        }
    private void OnAnimStop(Action<bool> callback = null)
    {
        callback?.Invoke(true);
    }
   
        public void PlayStarAnimation(Action<bool> callback)
        {
            foreach (var hold in holdScrews)
            {
                SpawningStar(hold.Index);
            }
            callback?.Invoke(true);
        }

        public void SpawningStar(int index)
        {
            //star[index].SetActive(true);
            //start.setposition = hold[index] position
            //particle shiny play
        }
        // Hàm di chuyển Screw vào một lỗ trống trong CrewBox
        public void AddScrew(Screw screw)
        {
            // Nếu màu screw không khớp, kết thúc ngay
            if (screw.Color != color)
            {
                Debug.LogWarning("Screw color mismatch!");
                return;
            }

            // Nếu đã biết vị trí trống
            if (nextEmptyIndex >= 0 && nextEmptyIndex < holdScrews.Length)
            {
                if (holdScrews[nextEmptyIndex].IsEmpty())
                {
                    holdScrews[nextEmptyIndex].AddScrew(screw);
                    UpdateNextEmptyIndex(); // Tìm vị trí trống mới
                    return;
                }
            }

            // Tìm lỗ trống lần đầu hoặc khi trạng thái thay đổi
            for (int i = 0; i < holdScrews.Length; i++)
            {
                if (holdScrews[i].IsEmpty())
                {
                    holdScrews[i].AddScrew(screw);
                    nextEmptyIndex = i;
                    UpdateNextEmptyIndex(); // Tìm lỗ trống tiếp theo
                    return;
                }
            }

            // Nếu không có lỗ trống nào
            Debug.LogWarning("All screw holes are filled!" + gameObject.name + " at hold ");
        }

        private void UpdateNextEmptyIndex()
        {
            nextEmptyIndex = -1; // Đặt mặc định không có lỗ trống
            for (int i = 0; i < holdScrews.Length; i++)
            {
                if (holdScrews[i].IsEmpty())
                {
                    nextEmptyIndex = i;
                    break;
                }
            }
            if (nextEmptyIndex == -1)
            {
                Debug.Log("All screw holes are now filled!");
                onScrewBoxFull.Invoke(true); // Gọi sự kiện khi tất cả lỗ đã đầy
            }
        }
        
        // Hàm để thay đổi màu của CrewBox
        private void SetBoxColor(Color newColor)
        {
            // Đặt màu cho box (có thể thêm logic cập nhật màu)
            render.material.color = newColor;
        }
    }
}
