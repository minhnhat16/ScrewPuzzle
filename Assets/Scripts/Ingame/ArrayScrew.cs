using Enums;
using Ingame.Screw;
using Managers;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace Ingame
{
    public class ArrayScrew : MonoBehaviour, IResetable
    {
        public static ArrayScrew Instance;
        [SerializeField] private int coutHoldActive;
        [SerializeField] private float totalWidth;
        //[SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<HoldScrew> holdScrews; // Mảng các HoldScrew (ô chứa screw)
        [SerializeField] private List<Screw.Screw> screws; // Mảng các HoldScrew (ô chứa screw)
        private Coroutine alignmentCoroutine;
        private Coroutine holdRoutine;
        private bool stopCheckHold = false;

        public UnityEvent onHoldScrewsFull = new(); // Sự kiện khi holdScrews đầy
        public List<Screw.Screw> Screws
        {
            get => screws;
            set => screws = value;
        }
        private void OnEnable()
        {
            onHoldScrewsFull.AddListener(ScrewFullEvent);
        }

        private void OnDisable()
        {
            onHoldScrewsFull.RemoveListener(ScrewFullEvent);
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Instance = this;
        }

        private void Start()
        {
            //spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            coutHoldActive = 7;
            HoldAlignment();
        }


        public void SpawnNewHold()
        {
            Debug.Log("Spawn new hold");
            coutHoldActive++;
            var hold = holdScrews.FirstOrDefault(hold => !hold.gameObject.activeSelf);
            if (hold == null) return;
            hold.gameObject.SetActive(true);

            HoldAlignment(() =>
            {
            });
        }
        public void ShowArrayScrew()
        {
            coutHoldActive = 5;
            //spriteRenderer.enabled = true;
            for (int i = 0; i < coutHoldActive; i++)
            {
                holdScrews[i].gameObject.SetActive(true);
            }
        }
        public void ShowArrayActive(int activeCount)
        {
            coutHoldActive = activeCount;
            for (int i = 0; i < holdScrews.Count; i++)
            {
                holdScrews[i].gameObject.SetActive(i < activeCount);
            }
        }
        public void HoldAlignment(Action callback = null)
        {
            if (holdScrews.Count == 0) return;

            if (alignmentCoroutine != null)
            {
                StopCoroutine(alignmentCoroutine);
            }
            alignmentCoroutine = StartCoroutine(HoldAlignmentCoroutine(0, callback));
        }

        private IEnumerator HoldAlignmentCoroutine(float duration = 0.5f, Action callBack = null)
        {
            var activeHolds = holdScrews.Where(hold => hold.gameObject.activeSelf).ToList();
            if (activeHolds.Count == 0) yield break;

            float totalWidth = this.totalWidth;
            float minSpacing = 0.7f;
            float spacing = Mathf.Max(minSpacing, totalWidth / (activeHolds.Count + 1));

            // ✅ Căn giữa
            float totalOccupiedWidth = spacing * (activeHolds.Count - 1);
            float startX = -totalOccupiedWidth / 2f;


            List<Vector3> initialPositions = activeHolds.Select(h => h.transform.localPosition).ToList();
            List<Vector3> targetPositions = new List<Vector3>();

            for (int i = 0; i < activeHolds.Count; i++)
            {
                Vector3 localPos = activeHolds[i].transform.localPosition;
                var targetPos = new Vector3(startX + spacing * i, localPos.y, localPos.z);
                targetPositions.Add(targetPos);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                for (int i = 0; i < activeHolds.Count; i++)
                {
                    activeHolds[i].transform.localPosition = Vector3.Lerp(initialPositions[i], targetPositions[i], t);
                }

                yield return null;
            }

            for (int i = 0; i < activeHolds.Count; i++)
                activeHolds[i].transform.localPosition = targetPositions[i];

            alignmentCoroutine = null;
            callBack?.Invoke();
        }

        // Hàm thêm Screw vào một ô trống trong holdScrew
        private void ScrewFullEvent()
        {

            IngameController.ins.GameEndInvoker();
        }
        // Hàm kiểm tra xem tất cả các ô trong holdScrews đã đầy chưa
        private IEnumerator CheckHoldCoroutine()
        {
            stopCheckHold = false;

            while (!stopCheckHold && !IngameController.ins.IsGameOver)
            {
                bool allFull = holdScrews.All(h => h != null && !h.IsEmpty());

                if (allFull)
                {
                    Debug.Log("All holdScrews are full!");
                    yield return new WaitForSeconds(2f);

                    allFull = holdScrews.All(h => h != null && !h.IsEmpty());

                    if (allFull && BoxQueue.ins.MovingBox == false)
                    {
                        stopCheckHold = true;     // <--- STOP TẠI ĐÂY
                        onHoldScrewsFull?.Invoke();
                        yield break;              // <--- Thoát coroutine
                    }
                }

                yield return new WaitForSeconds(2f);
            }
        }


        private void CheckIfHoldScrewsFull()
        {
            if (holdRoutine != null)
                StopCoroutine(holdRoutine);

            holdRoutine = StartCoroutine(CheckHoldCoroutine());
        }
        public void RemoveScrewOutHold(Screw.Screw screw)
        {
            var hold = holdScrews.Find(h => h.IsContain(screw));
            if (hold == null) return;
            hold.ClearScrewOnHold();
            screws.Remove(screw);
        }

        public void RemoveListScrewOutHold(List<Screw.Screw> screws)
        {
            foreach (var screw in screws)
            {
                RemoveScrewOutHold(screw);
            }
        }
        public void AddScrew(Screw.Screw screw)
        {
            // Kiểm tra trạng thái của screw (ngăn click nhiều lần)
            if (screw.OnScrewClicked())
                return;

            // Nếu không có box phù hợp, thêm vào holdScrew
            var emptyHoldScrew = FindEmptyHoldScrew();
            if (emptyHoldScrew != null)
            {
                AddScrewToHoldScrew(screw, emptyHoldScrew);
            }
            else
            {
                // Nếu không có holdScrew trống, reset trạng thái của screw
                screw.ResetClickedFlag();
            }
        }


        private HoldScrew FindEmptyHoldScrew()
        {
            return holdScrews.FirstOrDefault(hold => hold.IsEmpty());
        }

        private void AddScrewToHoldScrew(Screw.Screw screw, HoldScrew holdScrew)
        {
            // Tìm box phù hợp cho screw
            var suitableBox = BoxQueue.ins.FindSuitableBox(screw);
            bool canAdd = false;
            if (suitableBox != null)
            {
                // Thêm screw vào box phù hợp
                BoxQueue.ins.AddScrewToBox(screw, suitableBox, out canAdd);
            }
            if (canAdd) return;
            holdScrew.AddScrew(screw, false, (onMoved) =>
            {
                // Kiểm tra nếu tất cả holdScrew đã đầy
                CheckIfHoldScrewsFull();
            });

            // Thêm screw vào danh sách tạm thời
            screws.Add(screw);
            var screwMng = LevelManager.ins.ScrewManager;
            screwMng.RemoveScrew(screw);

        }

        public void ClearAllScrewsOnArray()
        {
            if (screws.Count == 0) return;
            StartCoroutine(SetScrewInActive());
        }

        private IEnumerator SetScrewInActive()
        {
            for (int i = 0; i < screws.Count; i++)
            {
                var screw = screws[i];
                ScrewPool.Instance.Pool.ReturnToPool(screw);
                yield return null;
            }


            for (int j = 0; j < holdScrews.Count; j++)
            {
                holdScrews[j].ClearScrewOnHold();
                yield return null;
            }
            screws.Clear();
        }

        public ColorEnum GetMostestColorInArray()
        {
            if (screws.Count == 0)
            {
                return ColorEnum.Clear;
            }
            var t = screws.Max(screw => screw.Color);
            return t;
        }

        public void OnReset()
        {
            holdRoutine = null;
            ShowArrayActive(5);
        }
        //private void OnValidate()
        //{
        //    HoldAlignment();
        //}
    }
}
