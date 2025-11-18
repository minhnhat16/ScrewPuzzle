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
        public UnityEvent<int> spawnStartEvent = new();
         public List<HoldScrew> holdScrews; // Mảng các lỗ Screw
         public UnityEvent<bool> onScrewBoxFull;
        [SerializeField] private ColorEnum color;
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
            var upperGameObj = renderUpper.gameObject;
            upperGameObj.transform.localPosition = 10 * Vector3.up;
            renderUpper.enabled = false;

            var renderGObj = render.gameObject;
            TunOffScrews();
            color= ColorEnum.Empty;
            renderUpper.color = Color.ToColor();
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
            this.isLocked = isLocked;
          
            if (IsLocked)
            {
                string path = $"{GameConstants.BOX_SPRITE_PATH}/box_them";
                var sprite = Resources.Load<Sprite>($"{path}");
                renderUpper.sprite = sprite;
                renderUpper.transform.localPosition = Vector2.zero;
                renderUpper.color = ColorEnum.White.ToColor();
                renderUpper.transform.localScale = Vector2.one;
                renderUpper.enabled = IsLocked;
                render.sprite = ColorEnumExtensions.ToBoxSprite(ColorEnum.Brown);
                render.enabled = !IsLocked;
            }
            else
            {
                renderUpper.transform.localScale = Vector2.one * 3.5f;

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
            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(()=>!isMoving);
            BoxQueue.Instance.DeactivateAndMoveQueue(this);
        }

        public void CloseBox(float time = 0.5f,Action<bool> callback = null)
        {
            DoUpperBoxMove((boxFull) =>
            {
                callback?.Invoke(boxFull);
            }, time);
        }

        private void TunOffScrews()
        {

            //Debug.Log("Turn off Screww");
            foreach (var t in holdScrews)
            {
                ScrewPool.Instance.Pool.ReturnToPool(t.Screw);
            }
        }
        private void DoUpperBoxMove(Action<bool> callback, float time = 0.5f)
        {
            Sequence mySequence = DOTween.Sequence();
            renderUpper.enabled = true;
            var color= ColorEnumExtensions.ToColor(this.color);
            color.a = 0.4f;
            renderUpper.color = color;
            int totalHold = holdScrews.Count;
            // Debug trước khi sử dụng giá trị này
            //Debug.Log("Total Hold Screws: " + totalHold);

            mySequence.Append(renderUpper.transform.DOLocalMoveY(0, time)
                .SetEase(Ease.InCirc).OnComplete(() =>
                {
                    //Debug.Log("OnComplete: TunOffScrews & SpawningStar");
                    //TunOffScrews();
                    spawnStartEvent?.Invoke(totalHold);
                    //Debug.Log($"Processing Hold Screw at { totalHold}");
                }))
                .Append(transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f),time, 1)
                    .SetEase(Ease.InBack).OnComplete(() =>
                    {
                        callback.Invoke(true);
                    }))
                .Append(transform.DOMove(new Vector3(10, 10, 0), time * 4, false)
                    .SetEase(Ease.OutBounce)).OnComplete(() =>
                    {
                        Reset();

                        gameObject.SetActive(false);
                    });
        }

        public virtual void SetActive(bool isActive)
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

                Vector3 newPosition = new(CameraMain.instance.GetTop(), CameraMain.instance.GetRight());


                Debug.Log("Position target " + newPosition);
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
            if (gameObject.activeSelf)
            {
                var added = false;

                StartCoroutine(WaitForBoxStopAndAddScrew(() =>
                {
                    AddScrewToSlot(screw, out added,isTele);

                    isAddingScrew = false;
                }));
                canAdd = added;

            }
            else
            {
                AddScrewToSlot(screw, out canAdd,isTele);
                isAddingScrew = false;
            }
        }

        public virtual void AddScrew(List<Screw.Screw> screws,bool isTele = false)
        {
            bool canAdd;
            var screwManager = LevelManager.Instance.ScrewManager;
            //Debug.Log("Add many screw");
            if (isActiveAndEnabled)
            {
                StartCoroutine(WaitForBoxStopAndAddScrew(() =>
                {
                    foreach (var screw in screws)
                    {
                        if(!screw.isActiveAndEnabled) screw.gameObject.SetActive(true);    
                        AddScrewToSlot(screw, out canAdd, isTele);
                        //GameUtils.LogAndSelect("can add active " + canAdd, this.gameObject);
                    }
                }));
            }
            else
            {
                foreach (var screw in screws)
                {
                    if (!screw.isActiveAndEnabled) screw.gameObject.SetActive(true);
                    AddScrewToSlot(screw, out canAdd);
                   // GameUtils.LogAndSelect("can add inactive" + canAdd, this.gameObject);
                }
            }
            screwManager.RemoveScrew(screws);

        }
        private IEnumerator WaitForBoxStopAndAddScrew(Action callback)
        {
            yield return new WaitUntil(() => !isMoving);
            callback?.Invoke();
            // Now that the screw has stopped moving, add it
        }


        private void AddScrewToSlot( Screw.Screw screw, out bool canAdd, bool isTele = false)
        {
            canAdd = false;
            if(isBoxFull)
            {
                canAdd = false;
                return;
            }
            bool isKnowPosition = nextEmptyIndex >= 0 && nextEmptyIndex < holdScrews.Count;

            Debug.Log("Next empty index has know position " + isKnowPosition);

            if (isKnowPosition)
            {

                // Kiểm tra xem vị trí này có thực sự trống không
                if (holdScrews[nextEmptyIndex].IsEmpty())
                {

                    Debug.Log("Adding screw at" + nextEmptyIndex);
                    canAdd = true;
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
                Debug.Log("Adding screw at 2" + i);

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
        private void AddScrewToSlot(Screw.Screw screw, out bool canAdd)
        {

            Debug.Log("Add screw to box");
            canAdd = false;
            if (IsBoxFull)
            {
                Debug.Log("Box full");
                canAdd = false;
                return;
            };

            bool isKnowPosition = nextEmptyIndex >= 0 && nextEmptyIndex < holdScrews.Count;
            Debug.Log("Next empty index has know position " + isKnowPosition);

            if (isKnowPosition)
            {

                // Kiểm tra xem vị trí này có thực sự trống không
                if (holdScrews[nextEmptyIndex].IsEmpty())
                {

                    Debug.Log("Adding screw at" + nextEmptyIndex);
                    canAdd = true;
                    holdScrews[nextEmptyIndex].AddScrew(screw, false,    (onComplete) =>
                    {
                        ArrayScrew.Instance.RemoveScrewOutHold(screw);
                    });
                    UpdateNextEmptyIndex(); // Cập nhật vị trí trống tiếp theo
                    return;
                }
            }
           
            for (var i = 0; i < holdScrews.Count; i++)
            {
                Debug.Log("Adding screw at 2" + i);

                if (!holdScrews[i].IsEmpty()) continue;
                holdScrews[i].AddScrew(screw, false,(onComplete) =>
                {
                    ArrayScrew.Instance.RemoveScrewOutHold(screw);

                });
                nextEmptyIndex = i;
                UpdateNextEmptyIndex(); 
                canAdd = true;
                return;
            }
            Debug.LogError("All screw holes are filled! " + gameObject.name + " at hold ");
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
            render.sprite = color.ToBoxSprite();
        }


        public void Clear()
        {
           
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
                //Debug.Log("reset hold screw " + h.Screw);
            }
            ;
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
            holdScrews.RemoveAll(h => !h.IsEmpty());
            SetIsLocked(false);
        }
    }
}
