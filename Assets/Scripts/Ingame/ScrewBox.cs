using ConfigFile;
using DG.Tweening;
using Enums;
using Managers;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Sequence = DG.Tweening.Sequence;

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
        [SerializeField] private Vector3 scale;
        [SerializeField] private Collider2D _collider;

        [SerializeField] private bool isBoxFull;
        [SerializeField] private bool isAddingScrew = false;
        [SerializeField] private int totalHold;

        public bool IsAddingScrew
        {
            get => isAddingScrew;
            set => isAddingScrew = value;
        }

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
        public UnityEvent<int> spawnStartEvent = new();
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
        public ColorEnum Color
        {
            get => color;
            set => color = value;
        }
        public SpriteRenderer Render
        {
            get => render;
            set => render = value;
        }

        public Vector3 Position
        {
            get => _anchor.transform.position;
            set => transform.position = value;
        }
        public int TotalHold { get => totalHold; set => totalHold = value; }
        public void Awake()
        {
            scale = transform.localScale;
        }
        public virtual void OnEnable()
        {
            _transform = transform.GetComponent<Transform>();
            spawnStartEvent.RemoveAllListeners();
            spawnStartEvent.AddListener(SpawningStar);

        }
        public virtual void OnDisable()
        {
            spawnStartEvent.RemoveAllListeners();

        }
        public void OnInit(Vector3 position, ColorEnum color, bool isBoxFull, int totalHold)
        {
            this.color = color;
            this.Position = position;
            this.isBoxFull = isBoxFull;

            BoxUtils.SetBoxColor(this, color);
        }
        public virtual void Start()
        {

            // SetBoxColor(UnityEngine.Color.white);
        }

        public void Reset()
        {
            _transform.localScale = scale;
            color = ColorEnum.Empty;
            var upperGameObj = renderUpper.gameObject;
            upperGameObj.transform.localPosition = 10 * Vector3.up;
            renderUpper.enabled = false;

            var renderGObj = render.gameObject;

            renderGObj.SetActive(true);
            renderGObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            foreach (var h in holdScrews)
            {
                h.ClearScrewOnHold(); // should put reset here
                //Debug.Log("reset hold screw " + h.Screw);
            }
            ;

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
            //Debug.Log("Open ads dialog");
        }
        // khi box đầy thuc hien ham sau 
        protected virtual void BoxFullInvoker(bool isFull)
        {
            //Debug.Log("Box full invoker " + gameObject.name + "\t" + isBoxFull);
            if (!isFull) return;
            isBoxFull = true;
            // set box active fasle
            //Debug.Log("Box full invoker " + gameObject.name);
            StartCoroutine(InactiveBoxCoroutine());
        }

        IEnumerator InactiveBoxCoroutine()
        {
            yield return new WaitForSeconds(1f);
            BoxQueue.Instance.DeactivateAndMoveQueue(this);
        }

        public void CloseBox(Action<bool> callback)
        {
            DoUpperBoxMove((boxFull) =>
            {
                // spawnStartEvent?.Invoke(holdScrews.Count);
                callback?.Invoke(boxFull);
            });
        }

        private void TunOffScrews()
        {

            //Debug.Log("Turn off Screww");
            foreach (var t in holdScrews)
            {
                ScrewPool.Instance.Pool.ReturnToPool(t.Screw);
            }
        }
        private void DoUpperBoxMove(Action<bool> callback)
        {
            Sequence mySequence = DOTween.Sequence();
            renderUpper.enabled = true;

            int totalHold = holdScrews.Count;
            // Debug trước khi sử dụng giá trị này
            //Debug.Log("Total Hold Screws: " + totalHold);

            mySequence.Append(renderUpper.transform.DOLocalMoveY(0, 0.5f)
                .SetEase(Ease.InCirc).OnComplete(() =>
                {
                    //Debug.Log("OnComplete: TunOffScrews & SpawningStar");
                    TunOffScrews();
                    spawnStartEvent?.Invoke(totalHold);
                    //Debug.Log($"Processing Hold Screw at { totalHold}");
                }))
                .Append(transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.5f, 1)
                    .SetEase(Ease.InBack).OnComplete(() =>
                    {
                        callback.Invoke(true);
                    }))
                .Append(transform.DOMove(new Vector3(10, 10, 0), 2f, false)
                    .SetEase(Ease.OutBounce)).OnComplete(() =>
                    {
                        Reset();
                        gameObject.SetActive(false);
                    });
        }

        public virtual void SetBoxActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SpawningStar(int totalHold)
        {
            if (totalHold <= 0)
            {
                //Debug.LogWarning("No screws to spawn stars for.");
                return;
            }
            //Debug.Log("SpawningStar");

            StartCoroutine(PoppingStar(totalHold));
        }

        private IEnumerator PoppingStar(int totalHold)
        {
            int newStarAdded = 0;

            foreach (var hold in holdScrews)
            {
                var star = StarPool.Instance.pool.SpawnNonGravity();
                star.SetStarPos(hold.transform.position);

                Vector3 newPosition = new Vector3(CameraMain.instance.GetTop(), CameraMain.instance.GetRight());
                Vector3 starScale = GameManager.instance.StarScale;

                star.PopingStar(starScale, newPosition, () =>
                {
                    //Debug.Log("Callback triggered for a star!");
                });
                newStarAdded++;
            }

            //Debug.Log($"Stars popping initiated. Waiting for all {holdScrews.Count} stars to complete.");
            yield return new WaitUntil(() => newStarAdded == holdScrews.Count);

            //Debug.Log("All stars completed popping!");
            IngameController.Instance.StarChanging(holdScrews.Count);
        }



        // Hàm di chuyển Screw vào một lỗ trống trong CrewBox
        public virtual void AddScrew(Screw.Screw screw)
        {
            isAddingScrew = true;
            // Nếu màu screw không khớp, kết thúc ngay
            if (screw.Color != color)
            {
                Debug.LogWarning("Screw color mismatch!");
                isAddingScrew = !isAddingScrew;
                return;
            }
            StartCoroutine(WaitForBoxStopAndAddScrew(() =>
            {
                AddScrewToSlot(screw);
                isAddingScrew = false;
            }));
        }

        public virtual void AddScrew(List<Screw.Screw> screws)
        {
            //Debug.Log("Add many screw");
            StartCoroutine(WaitForBoxStopAndAddScrew(() =>
            {
                foreach (var screw in screws)
                {
                    AddScrewToSlot(screw);

                }
            }));

        }
        private IEnumerator WaitForBoxStopAndAddScrew(Action callback)
        {
            yield return new WaitUntil(() => !isMoving);
            callback?.Invoke();
            // Now that the screw has stopped moving, add it
        }
        private void AddScrewToSlot(Screw.Screw screw)
        {
            if (IsBoxFull) return;
            // Nếu đã biết vị trí lỗ trống
            if (nextEmptyIndex >= 0 && nextEmptyIndex < holdScrews.Count)
            {
                // Kiểm tra xem vị trí này có thực sự trống không
                if (holdScrews[nextEmptyIndex].IsEmpty())
                {
                    holdScrews[nextEmptyIndex].AddScrew(screw, (onComplete) =>
                    {
                        ArrayScrew.Instance.RemoveScrewOutHold(screw);
                    });
                    UpdateNextEmptyIndex(); // Cập nhật vị trí trống tiếp theo
                    return;
                }
            }

            // Nếu không có vị trí trống hoặc chỉ số bị sai
            // Tìm lỗ trống theo cách thủ công từ đầu
            for (var i = 0; i < holdScrews.Count; i++)
            {
                if (!holdScrews[i].IsEmpty()) continue;
                holdScrews[i].AddScrew(screw, (onComplete) =>
                {
                    ArrayScrew.Instance.RemoveScrewOutHold(screw);

                });
                nextEmptyIndex = i;
                UpdateNextEmptyIndex(); // Cập nhật vị trí trống tiếp theo
                return;
            }
            // Nếu không có lỗ trống nào
            Debug.LogWarning("All screw holes are filled! " + gameObject.name + " at hold ");
        }


        private void UpdateNextEmptyIndex()
        {
            // Only run the logic if the state actually needs updating
            if (nextEmptyIndex != -1 && holdScrews[nextEmptyIndex].IsEmpty())
            {
                // No need to recalculate; the current `nextEmptyIndex` is still valid
                return;
            }

            // Reset the index and search for the next empty hole
            nextEmptyIndex = -1;
            for (int i = 0; i < holdScrews.Count; i++)
            {
                if (holdScrews[i].IsEmpty())
                {
                    nextEmptyIndex = i;
                    break;
                }
            }

            // If no empty slots remain, invoke the event
            if (nextEmptyIndex == -1)
            {
                //Debug.Log("All screw holes are now filled!");
                onScrewBoxFull?.Invoke(true); // Ensure the event is only invoked once
            }
        }

        // Hàm để thay đổi màu của CrewBox
        public void SetBoxColor(ColorEnum color)
        {
            // Đặt màu cho box (có thể thêm logic cập nhật màu)
            render.sprite = color.ToBoxSprite();
        }

        public void FindScrew()
        {
            if (!enabled) return;
            //Debug.Log("Start Find Screw");
            StartCoroutine(FindScrewCoroutine());
        }
        public void ClearScrewOnHold()
        {
            foreach (var h in holdScrews)
            {
                h.ClearScrewOnHold(); // should put reset here
                //Debug.Log("reset hold screw " + h.Screw);
            }
            ;
        }
        public IEnumerator FindScrewCoroutine()
        {
            while (gameObject.activeInHierarchy) // Kiểm tra nếu game object vẫn còn active
            {
                // Debug.Log("Finding Screw...");

                // Tìm các screw có màu giống với box
                var screws = ArrayScrew.Instance.Screws.Where(s => s.Color == color && !s.IsMoving() && s.isActiveAndEnabled).ToList();

                // Nếu không có screw nào khớp, đợi một khoảng thời gian trước khi tìm lại
                if (screws.Count == 0)
                {
                    yield return new WaitForSeconds(1f); // Có thể tùy chỉnh thời gian chờ
                }
                else
                {
                    // Thêm screw vào box và remove screw khỏi danh sách
                    AddScrew(screws);
                    yield return new WaitForSeconds(0.5f); // Đợi một chút sau khi thêm screw
                }
            }

            Debug.Log("GameObject is inactive. Stopping FindScrewCoroutine.");
        }
    }
}
