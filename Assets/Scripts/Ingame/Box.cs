using ConfigFile;
using DG.Tweening;
using Enums;
using Managers;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static SoundManager;
using Sequence = DG.Tweening.Sequence;

namespace Ingame
{
    public class Box : FSMSystem, IResetable
    {
        public BoxConfig config; // ScriptableObject chứa cấu hình cho CrewBox
        [SerializeField] private SpriteRenderer render;
        [SerializeField] private SpriteRenderer renderUpper;
        [SerializeField] private Transform _transform;
        [SerializeField] private Transform _anchor;
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 scale;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private ColorEnum color;

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

        public Animator animator;
        public float moveDuration = 1f;  // Thời gian di chuyển nắp
        public float squashAmount = 0.9f;  // Độ co giãn
        public float squashDuration = 0.2f;
        public Vector3 initialPosition;
        public bool isMoving;
        public UnityEvent<int, List<Vector3>> spawnStartEvent = new();
        public List<HoldScrew> holdScrews; // Mảng các lỗ Screw
        public UnityEvent<bool> onScrewBoxFull;
        public BoxSlot boxSlot;
        private bool isLocked;

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
        public bool IsLocked { get => isLocked; set => isLocked = value; }

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
            OnReset();
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
            isLocked = false;
            var upperGameObj = renderUpper.gameObject;
            upperGameObj.transform.localPosition = 10 * Vector3.up;
            renderUpper.enabled = false;
            var renderGObj = render.gameObject;
            TunOffScrews();
            color = ColorEnum.Empty;
            renderUpper.color = Color.ToColor();
            render.enabled = true;
            renderUpper.gameObject.SetActive(false);
            renderGObj.SetActive(true);
            renderGObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            renderUpper.gameObject.SetActive(false);
            foreach (var h in holdScrews)
            {
                h.ClearScrewOnHold(); // should put reset here
            }
            ;
            gameObject.SetActive(false);
        }

        // Hàm kiểm tra xem các lỗ trong CrewBox có đầy đủ Screw không
        public bool AreAllHolesFilled()
        {
            return holdScrews.All(screw => !screw.IsEmpty());
        }
        public void SetIsLocked(bool isLocked)
        {
            this.isLocked = isLocked;
            renderUpper.enabled = IsLocked;
            renderUpper.gameObject.SetActive(IsLocked);

            if (IsLocked)
            {
                string path = $"{GameConstants.BOX_SPRITE_PATH}/box_them";
                var sprite = Resources.Load<Sprite>($"{path}");
                renderUpper.sprite = sprite;
                renderUpper.transform.localPosition = Vector2.zero;
                renderUpper.color = ColorEnum.White.ToColor();
                renderUpper.transform.localScale = Vector2.one;
                render.sprite = ColorEnumExtensions.ToBoxSprite(ColorEnum.Brown);
                render.enabled = !IsLocked;
            }
            else
            {
                renderUpper.transform.localScale = Vector2.one;

            }

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
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !isMoving);
            BoxQueue.ins.DeactivateAndMoveQueue(this);
        }

        public void CloseBox(float time = 0.5f, Action<bool> callback = null)
        {
            DoUpperBoxMove((boxFull) =>
            {
               SoundHelper.PlaySFX(SFX.BoxClose);
                callback?.Invoke(boxFull);
            }, time);
        }

        private void TunOffScrews()
        {
            if(holdScrews.Count ==0) return;

            //Debug.Log("Turn off Screww");
            foreach (var t in holdScrews)
            {
                if(t.Screw == null) continue;
                ScrewPool.Instance.Pool.ReturnToPool(t.Screw);
            }
        }
        private void DoUpperBoxMove(Action<bool> callback, float time = 0.5f)
        {
            Sequence mySequence = DOTween.Sequence();
            renderUpper.gameObject.SetActive(true);
            //renderUpper.enabled = true;
            var color = ColorEnumExtensions.ToColor(this.color);
            color.a = 0.4f;
            renderUpper.color = color;
            int totalHold = holdScrews.Count;

            Vector3 pos = CameraMain.instance.GetTopRight();
            var holdPosition = holdScrews.Select(p => p.transform.position)
                                          .ToList();
            mySequence.Append(renderUpper.transform.DOLocalMoveY(0, time)
                .SetEase(Ease.InCirc).OnComplete(() =>
                {
                    //TunOffScrews();
                    //Debug.Log($"Processing Hold Screw at { totalHold}");
                }))
                .Append(transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), time, 1)
                    .OnStart(() => spawnStartEvent?.Invoke(totalHold, holdPosition))
                    .SetEase(Ease.InBack).OnComplete(() =>
                    {

                        callback.Invoke(true);
                    }))
                .Append(transform.DOMove(pos, time * 2, false)
                    .SetEase(Ease.OutSine)).OnComplete(() =>
                    {
                        Reset();
                        gameObject.SetActive(false);
                    });

        }

        public virtual void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SpawningStar(int totalHold, List<Vector3> positions)
        {
            if (totalHold <= 0)
            {
                //Debug.LogWarning("No screws to spawn stars for.");
                return;
            }
            //Debug.Log("SpawningStar");
            if (gameObject.activeSelf)
            {
                StartCoroutine(PoppingStar(totalHold, positions));

            }
        }

        private IEnumerator PoppingStar(int totalHold, List<Vector3> positions, Action callback = null)
        {
            int newStarAdded = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var star = StarPool.Instance.pool.SpawnNonGravity();
                star.SetStarPos(position);
                Vector3 starScale = GameManager.instance.StarScale;
                var gameView = ViewManager.Instance.currentView;
                var startRect = ViewManager.Instance.GetUIObject<StarBottleFill>(gameView).GetComponent<RectTransform>();

                Vector3 newPosition = ViewManager.Instance.UIToWorld(startRect, Camera.main);
                yield return new WaitForSeconds(0.05f);
                star.PopingStar(starScale, newPosition, () =>
                {


                    Debug.Log("Star pop complete");
                    newStarAdded++;
                });
            }
            Debug.Log($"Stars popping initiated. Waiting for all {holdScrews.Count} stars to complete. And new start added {newStarAdded}");

            //Debug.Log($"Stars popping initiated. Waiting for all {holdScrews.Count} stars to complete.");
            yield return new WaitUntil(() => newStarAdded >= holdScrews.Count);



            //Debug.Log("All stars completed popping!");
        }



        // Hàm di chuyển Screw vào một lỗ trống trong CrewBox
        public virtual void AddScrew(Screw.Screw screw, out bool canAdd, bool isTele = false)
        {
            isAddingScrew = true;
            canAdd = false;

            // Nếu màu screw không khớp, kết thúc ngay
            if (screw.Color != color)
            {
                Debug.LogWarning("Screw color mismatch!");
                isAddingScrew = !isAddingScrew;
                return;
            }
            if (gameObject.activeSelf )
            {
                var added = false;

                StartCoroutine(WaitForBoxStopAndAddScrew(() =>
                {
                    AddScrewToSlot(screw, out added, isTele);

                    isAddingScrew = false;
                }));
                canAdd = added;

            }
            else
            {
                AddScrewToSlot(screw, out canAdd, isTele);
                isAddingScrew = false;
            }
        }

        public virtual void AddScrew(List<Screw.Screw> screws, bool isTele = false)
        {
            if (screws == null || screws.Count == 0)
                return;

            // Nếu box đang di chuyển → chờ đứng yên
            if (isMoving)
            {
                StartCoroutine(WaitForBoxStopAndAddList(screws, isTele));
            }
            else
            {

                AddScrewListImmediate(screws, isTele);
            }
        }
        private IEnumerator WaitForBoxStopAndAddList(List<Screw.Screw> screws, bool isTele)
        {
            // chờ box đứng yên
            yield return new WaitUntil(() => !isMoving);

            AddScrewListImmediate(screws, isTele);
        }

        private void AddScrewListImmediate(List<Screw.Screw> screws, bool isTele)
        {
            if (isMoving && IsLocked) return;
            var screwManager = LevelManager.ins.ScrewManager;

            foreach (var screw in screws)
            {
                bool canAdd;
                screw.gameObject.SetActive(true);

                AddScrewToSlot(screw, out canAdd, isTele);
            }

            // chỉ remove 1 lần – đúng thời điểm
            screwManager.RemoveScrew(screws);
        }
        private IEnumerator WaitForBoxStopAndAddScrew(Action callback)
        {
            yield return new WaitUntil(() => !isMoving);
            callback?.Invoke();
            // Now that the screw has stopped moving, add it
        }


        private void AddScrewToSlot(Screw.Screw screw, out bool canAdd, bool isTele = false)
        {
            canAdd = false;
            if (isBoxFull || isLocked)
            {
                canAdd = false;
                return;
            }
            bool isKnowPosition = nextEmptyIndex >= 0 && nextEmptyIndex < holdScrews.Count;
            gameObject.SetActive(true);
            Debug.Log("Next empty index has know position " + isKnowPosition);

            if (isKnowPosition)
            {
                // Kiểm tra xem vị trí này có thực sự trống không
                if (holdScrews[nextEmptyIndex].IsEmpty())
                {

                    Debug.Log("Adding screw at" + nextEmptyIndex);
                    canAdd = true;
                    screw.SetSortingOrderAndLayer(render.sortingOrder+2,render.sortingLayerName);
                    holdScrews[nextEmptyIndex].AddScrew(screw, isTele, (onComplete) =>
                    {
                        ArrayScrew.Instance.RemoveScrewOutHold(screw);
                    });
                    UpdateNextEmptyIndex(); // Cập nhật vị trí trống tiếp theo
                    return;
                }
            }

            for (var i = 0; i < holdScrews.Count; i++)
            {

                if (!holdScrews[i].IsEmpty()) continue;
                holdScrews[i].AddScrew(screw, isTele, (onComplete) =>
                {
                    ArrayScrew.Instance.RemoveScrewOutHold(screw);
                });
                nextEmptyIndex = i;
                UpdateNextEmptyIndex();
                canAdd = true;
                return;
            }
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


            Debug.Log("Next empty index " + nextEmptyIndex);
        }

        // Hàm để thay đổi màu của CrewBox
        public void SetBoxColor(ColorEnum color)
        {
            // Đặt màu cho box (có thể thêm logic cập nhật màu)
            this.color = color;
            render.enabled =true;
            render.sprite = color.ToBoxSprite();
        }
        public void FindScrew()
        {
            if (!isActiveAndEnabled) return;
            StartCoroutine(FindScrewCoroutine());
        }
        public void ClearScrewOnHold()
        {
            foreach (var h in holdScrews)
            {
                h.ClearScrewOnHold(); // should put reset here
            };
        }
        public IEnumerator FindScrewCoroutine()
        {
            while (gameObject.activeInHierarchy)
            {

                var screws = ArrayScrew.Instance.Screws.Where(s => s.Color == color && !s.IsMoving() && s.isActiveAndEnabled).ToList();

                if (screws.Count == 0)
                {
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    AddScrew(screws);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            Debug.Log("GameObject is inactive. Stopping FindScrewCoroutine.");
        }

        public void OnReset()
        {
            Reset();
            holdScrews.RemoveAll(h => !h.IsEmpty());
            SetIsLocked(false);
            StopAllCoroutines();
            ClearScrewOnHold();
        }
    }
}
