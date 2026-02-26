using Enums;
using Ingame.Screw;
using Managers;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using SFX = SoundManager.SFX;
namespace Ingame
{
    public class ArrayScrew : MonoBehaviour, IResetable
    {
        public static ArrayScrew Instance;
        [SerializeField] private int coutHoldActive;
        [SerializeField] private float totalWidth;
        //[SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<HoldScrew> holdScrews; // Mảng các HoldScrew (ô chứa screw)
        [SerializeField] private List<ScrewController> screws; // Mảng các HoldScrew (ô chứa screw)
        private Coroutine alignmentCoroutine;
        private Coroutine holdRoutine;
        private bool stopCheckHold = false;

        public UnityEvent onHoldScrewsFull = new(); // Sự kiện khi holdScrews đầy
        public List<ScrewController> Screws
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
            totalWidth += 0.5f;
            HoldAlignment(() =>
            {
            });
        }
        public void ShowArrayScrew()
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            coutHoldActive = 5;
            //spriteRenderer.enabled = true;
            for (int i = 0; i < coutHoldActive; i++)
            {
                holdScrews[i].gameObject.SetActive(true);
            }

            var pos = transform.position;
            var y = CameraMain.instance.GetTop() - 5.5f;
            pos.y = y;
            transform.position = pos;
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
            IngameController.ins.Revive();
        }
        // Hàm kiểm tra xem tất cả các ô trong holdScrews đã đầy chưa
        private IEnumerator CheckHoldCoroutine()
        {
            stopCheckHold = false;
            var boxQueue = IngameController.ins.BoxQueue;
            while (!stopCheckHold && !IngameController.ins.IsGameOver)
            {
                bool allFull = holdScrews.All(h => h != null && !h.IsEmpty());

                if (allFull)
                {
                    //Debug.Log("All holdScrews are full!");
                    yield return new WaitForSeconds(2f);

                    allFull = holdScrews.All(h => h != null && !h.IsEmpty());

                    if (allFull && boxQueue.hasMovingBox == false)
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

            //Debug.Log("Check if holdScrews full" + holdRoutine);
            if (holdRoutine != null)
                StopCoroutine(holdRoutine);

            holdRoutine = StartCoroutine(CheckHoldCoroutine());
        }
        public void RemoveScrewOutHold(ScrewController screw)
        {
            var hold = holdScrews.Find(h => h.Screw == screw);
            if (hold == null) return;
            hold.RemoveScrew();
            screws.Remove(screw);
        }

        public void RemoveListScrewOutHold(List<ScrewController> screws)
        {
            foreach (var screw in screws)
            {
                RemoveScrewOutHold(screw);
            }
        }
        public void AddScrew(ScrewController screw)
        {
            if (screw == null) return;

            // Reserve the screw for move. This uses a separate reservation flag so
            // a prior OnScrewClicked() doesn't block locking for move.
            if (!screw.TryLockForMove())
            {
                Debug.Log($"[ArrayScrew] Screw {screw.name} cannot be reserved for move (blocked or reserved).");
                return;
            }

            // proceed: place into first empty hold or try boxes
            var emptyHoldScrew = FindEmptyHoldScrew();
            if (emptyHoldScrew != null)
            {
                SoundHelper.PlaySFX(SFX.ScrewClicked);
                AddScrewToHoldScrew(screw, emptyHoldScrew);
            }
            else
            {
                // No space — release the reservation and reset click state so user can try again
                screw.ResetClickedFlag();
                screw.ReleaseLockForMove();
            }
        }


        private HoldScrew FindEmptyHoldScrew()
        {
            return holdScrews.FirstOrDefault(hold => hold.IsEmpty());
        }

        private void AddScrewToHoldScrew(ScrewController screw, HoldScrew holdScrew)
        {
            var screwMng = LevelManager.ins.ScrewManager;

            if (TryAddToSuitableBox(screw))
                return;

            if (TryHandleRainbow(screw, screwMng))
                return;

            AddToHoldScrewFlow(screw, holdScrew, screwMng);
        }
        private bool TryAddToSuitableBox(ScrewController screw)
        {
            var boxQueue = IngameController.ins.BoxQueue;
            var suitableBox = boxQueue.FindSuitableBox(screw.GetColor());
            if (suitableBox == null)
                return false;

            HandleTutorialForBox(suitableBox);

            screw.SetSortingOrderAndLayer(4, "Box");

            bool canAdd;
            boxQueue.CanAddScrew(screw, suitableBox, out canAdd);

            if (canAdd)
                ParentTo(screw, suitableBox.transform);

            return true;
        }
        private void HandleTutorialForBox(Box suitableBox)
        {
            if (!DataAPIController.instance.IsNewPlayer())
                return;

            if (suitableBox.NextEmptyIndex < 1)
            {
                TutorialTargetRegistry.Register("box_1", suitableBox.transform);
                TutorialEventBus.Emit("Screw.Selected", "red_1");
            }
            else if (suitableBox.NextEmptyIndex > 1)
            {
                TutorialTargetRegistry.Register("box_close", suitableBox.transform);
                TutorialEventBus.Emit("Screw.Selected", "red_2");
            }
        }

        private bool TryHandleRainbow(ScrewController screw, ScrewManager screwMng)
        {
            if (screw.GetColor() != ColorEnum.Rainbow)
                return false;

            SpecialBoxManager.ins.AddSingle(screw);
            screwMng.RemoveScrew(screw);
            return true;
        }
        private void AddToHoldScrewFlow(
            ScrewController screw,
            HoldScrew holdScrew,
            ScrewManager screwMng)
        {
            screw.SetSortingOrderAndLayer(4, "Box");

            holdScrew.AddScrew(screw, false, (onMoved) =>
            {
                if (onMoved)
                    //ParentTo(screw, holdScrew.transform);

                HandleTutorialForHold(holdScrew);
                CheckIfHoldScrewsFull();
            });

            screws.Add(screw);
            screwMng.RemoveScrew(screw);
        }
        private void HandleTutorialForHold(HoldScrew holdScrew)
        {
            if (!DataAPIController.instance.IsNewPlayer())
                return;

            TutorialTargetRegistry.Register("array_1", holdScrew.transform);
            TutorialEventBus.Emit("Screw.Selected", "blue_1");
        }
        public void ClearAllScrewsOnArray()
        {
            if (screws.Count == 0) return;
            StartCoroutine(SetScrewInActive());
        }
        private void ParentTo(ScrewController screw, Transform parent)
        {
            screw.transform.SetParent(parent, false);
            screw.transform.localPosition = Vector3.zero;
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
                holdScrews[j].RemoveScrew();
                yield return null;
            }
            screws.Clear();
        }

        public ColorEnum GetMostestColorInArray()
        {
            if (screws == null || screws.Count == 0)
                return ColorEnum.Clear;

            return screws
                .Where(s => s != null)
                .GroupBy(s => s.GetColor())
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
        }



        public void OnReset()
        {
            totalWidth = GameConstants.ArrayWidth;
            holdRoutine = null;
            ShowArrayActive(5);
            ClearAllScrewsOnArray();
        }


        public void StartClearHiding()
        {
            StartCoroutine(ClearToHidding());
        }
        // Change the signature of ClearToHidding from void to IEnumerator
        public IEnumerator ClearToHidding()
        {
            var boxQueue = IngameController.ins.BoxQueue;

            var copy = screws.ToList();   // clone để tránh modify trong quá trình loop

            foreach (var screw in copy)
            {
                screw.SetActive(false);
                yield return null;
            }

            var screwManager = IngameController.ins.ScrewManager;
            screwManager.AddHiddenScrews(copy);
            foreach (var hold in holdScrews.ToList())
            {
                hold.RemoveScrew();
                yield return null;
            }

            screws.Clear();
        }
        internal Vector3 GetHoldPos()
        {
            var hold = holdScrews.Last(h => h.gameObject.activeSelf);
            Vector3 target = hold.transform.position;
            target.y += 0.5f;
            return target;
        }
    }
}