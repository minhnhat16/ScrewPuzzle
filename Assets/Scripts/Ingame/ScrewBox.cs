using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Transform _transform;
        [SerializeField] private Transform _anchor;
        [SerializeField] private Vector3 position;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private bool isBoxFull;

        [SerializeField] private int nextEmptyIndex = -1;

        public int NextEmptyIndex
        {
            get => nextEmptyIndex;
            set => nextEmptyIndex = value;
        }

        [SerializeField] public List<HoldScrew> holdScrews; // Mảng các lỗ Screw
        [SerializeField] public UnityEvent<bool> onScrewBoxFull;
        [SerializeField] private ColorEnum color;
        [SerializeField] public BoxSlot boxSlot;

        public Animator animator;
        public float moveDuration = 1f;  // Thời gian di chuyển nắp
        public float squashAmount = 0.9f;  // Độ co giãn
        public float squashDuration = 0.2f;
        public Vector3 initialPosition;
        public bool isMoving;

        public bool IsBoxFull
        {
            get => isBoxFull;
            set => isBoxFull = value;
        }
        public Transform Transform
        {
            get => _transform;
            set => _transform = value;
        }
        public ColorEnum Color {get => color;
            set => color=value;
        }
        public SpriteRenderer Render {get => render;
            set => render = value;
        }
        
        public Vector3 Position
        {
            get => _anchor.transform.position;
            set => transform.position = value;
        }
    
        private void OnEnable()
        {
        }

        public void OnInit(Vector3 position, BoxConfigRecord config, bool isBoxFull)
        {
            this.color= config.BoxColor;
            this.Position = position;
            this.isBoxFull = isBoxFull;
        }
       public virtual void Start()
        {
            _transform = transform.GetComponent<Transform>(); 
           
           // SetBoxColor(UnityEngine.Color.white);
        }

        public void Reset()
        {
            _transform.localScale = Vector3.one;
            color = ColorEnum.Empty;
            var upperGameObj = render.gameObject;
            upperGameObj.transform.localPosition = 10 * Vector3.up;
            upperGameObj.SetActive(false);
            foreach (var h in holdScrews)
            {
                h.Screw = null; // should put reset here
            };
        }

        // Hàm kiểm tra xem các lỗ trong CrewBox có đầy đủ Screw không
        public bool AreAllHolesFilled()
        {
            return holdScrews.All(screw => !screw.IsEmpty());
        }
        public void SetIsLocked(bool isLocked)
        {
            renderUpper.transform.position = gameObject.transform.position + new Vector3(0, 5f, 0);
            // _collider = new BoxCollider();
        }

        public void OnClickCollider()
        {
            //OpenAds dialog;
            Debug.Log("Open ads dialog");
        }
        // khi box đầy thuc hien ham sau 
        protected virtual void BoxFullInvoker(bool isFull)
        {
            Debug.Log("Box full invoker " + gameObject.name  + "\t" + isBoxFull);
            if (!isFull) return;
            isBoxFull = true;
            // set box active fasle
            Debug.Log("Box full invoker " + gameObject.name );
            StartCoroutine(InactiveBoxCoroutine());
        }

        IEnumerator InactiveBoxCoroutine()
        {
            yield return new WaitForSeconds(1f);
            BoxQueue.Instance.DeactivateAndMoveQueue(this);
        }

        public void CloseBox(Action<bool> callback)
        {
            DoUpperBoxMove((boxFull)=>
            {
                callback?.Invoke(boxFull);
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
            Sequence mySequence = DOTween.Sequence();
            renderUpper.gameObject.SetActive(true);
            // Thêm các hành động di chuyển vào Sequence
            mySequence.Append(renderUpper.transform.DOLocalMoveY(0, 0.5f) // Di chuyển theo trục Y
                    .SetEase(Ease.InCirc).OnComplete(TunOffScrews))
                .Append(transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.5f, 1) // Punch scale
                    .SetEase(Ease.InBack).OnComplete(()=>
                    {
                        callback.Invoke(true);
                    }))
                .Append(transform.DOMove(new Vector3(10, 10, 0), 2f, false) // Di chuyển đến vị trí mới
                    .SetEase(Ease.OutBounce)).OnComplete(()=>
                        {
                            Reset();
                            gameObject.SetActive(false);
                        });
        }

        public virtual void  SetBoxActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SpawningStar(int index)
        {
            //star[index].SetActive(true);
            //start.setposition = hold[index] position
            //particle shiny play
        }
        // Hàm di chuyển Screw vào một lỗ trống trong CrewBox
        public virtual void AddScrew(Screw.Screw screw)
        {
            
            // Nếu màu screw không khớp, kết thúc ngay
            if (screw.Color != color)
            {
                Debug.LogWarning("Screw color mismatch!");
                return;
            }
            StartCoroutine(WaitForBoxStopAndAddScrew(()=>AddScrewToSlot(screw)));
        }
        
        private IEnumerator WaitForBoxStopAndAddScrew(Action callback)
        {
            yield return new WaitUntil(() => !isMoving);
            callback?.Invoke();
            // Now that the screw has stopped moving, add it
        }
        private void AddScrewToSlot(Screw.Screw screw)
        {
            // Nếu đã biết vị trí lỗ trống
            if (nextEmptyIndex >= 0 && nextEmptyIndex < holdScrews.Count)
            {
                // Kiểm tra xem vị trí này có thực sự trống không
                if (holdScrews[nextEmptyIndex].IsEmpty())
                {
                    holdScrews[nextEmptyIndex].AddScrew(screw);
                    UpdateNextEmptyIndex(); // Cập nhật vị trí trống tiếp theo
                    return;
                }
            }

            // Nếu không có vị trí trống hoặc chỉ số bị sai
            // Tìm lỗ trống theo cách thủ công từ đầu
            for (int i = 0; i < holdScrews.Count; i++)
            {
                if (holdScrews[i].IsEmpty())
                {
                    holdScrews[i].AddScrew(screw);
                    nextEmptyIndex = i;
                    UpdateNextEmptyIndex(); // Cập nhật vị trí trống tiếp theo
                    return;
                }
            }

            // Nếu không có lỗ trống nào
            Debug.LogWarning("All screw holes are filled! " + gameObject.name + " at hold ");
        }


        private void UpdateNextEmptyIndex()
        {
            nextEmptyIndex = -1; // Đặt mặc định không có lỗ trống
            for (int i = 0; i < holdScrews.Count; i++)
            {
                if (holdScrews[i].IsEmpty())
                {
                    nextEmptyIndex = i;
                    break;
                }
            }

            if (nextEmptyIndex != -1) return;
            Debug.Log("All screw holes are now filled!");
            onScrewBoxFull.Invoke(true); // Gọi sự kiện khi tất cả lỗ đã đầy
        }
        // Hàm để thay đổi màu của CrewBox
        public void SetBoxColor(Color newColor)
        {
            // Đặt màu cho box (có thể thêm logic cập nhật màu)
            render.material.color = newColor;
                
        }

    }
}
