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
    public class ArrayScrew : MonoBehaviour, IResetable, IArrayScrew
    {
        public static ArrayScrew Instance;

        // ─────────────────────────────────────────
        // Inspector
        // ─────────────────────────────────────────

        [SerializeField] private int activeHoldCount;
        [SerializeField] private float totalWidth;
        [SerializeField] private List<HoldScrew> holdScrews;
        [SerializeField] private List<ScrewController> screws;

        // ─────────────────────────────────────────
        // Injected dependencies (set từ bên ngoài, không dùng singleton)
        // ─────────────────────────────────────────

        private IBoxQueue _boxQueue;
        private ScrewManager _screwManager;

        public void Inject(IBoxQueue boxQueue, ScrewManager screwManager)
        {
            _boxQueue = boxQueue;
            _screwManager = screwManager;
        }

        // ─────────────────────────────────────────
        // IArrayScrew — State
        // ─────────────────────────────────────────

        public int ActiveHoldCount => activeHoldCount;

        public bool IsFull => holdScrews
            .Where(h => h.gameObject.activeSelf)
            .All(h => h != null && !h.IsEmpty());

        public bool HasAny() => screws != null && screws.Count > 0;

        // ─────────────────────────────────────────
        // IArrayScrew — Events
        // ─────────────────────────────────────────

        /// <summary>
        /// Fire khi array full và box không đang di chuyển.
        /// IngameController lắng nghe → TriggerArrayScrewFull()
        /// </summary>
        public event Action OnArrayFull;

        // ─────────────────────────────────────────
        // Internal coroutine state
        // ─────────────────────────────────────────

        private Coroutine _alignmentCoroutine;
        private Coroutine _holdCheckCoroutine;
        private bool _stopCheckHold;
        private bool _isGameActive; // set bởi IngameController qua SetGameActive()

        // ─────────────────────────────────────────
        // Unity Lifecycle
        // ─────────────────────────────────────────

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            activeHoldCount = 7;
            HoldAlignment();
        }

        // ─────────────────────────────────────────
        // IArrayScrew — Screw Operations
        // ─────────────────────────────────────────

        public void AddScrew(ScrewController screw)
        {
            if (screw == null) return;

            if (!screw.TryLockForMove())
            {
                Debug.Log($"[ArrayScrew] {screw.name} không thể lock để move.");
                return;
            }

            var emptyHold = FindEmptyHold();
            if (emptyHold != null)
            {
                SoundHelper.PlaySFX(SFX.ScrewClicked);
                AddScrewToHold(screw, emptyHold);
            }
            else
            {
                // Không còn chỗ — trả lại state screw
                screw.ResetClickedFlag();
                screw.ReleaseLockForMove();
            }
        }

        public void RemoveScrew(ScrewController screw)
        {
            var hold = holdScrews.Find(h => h.Screw == screw);
            if (hold == null) return;

            hold.RemoveScrew();
            screws.Remove(screw);
        }

        public void RemoveScrews(IEnumerable<ScrewController> screwList)
        {
            foreach (var screw in screwList.ToList())
                RemoveScrew(screw);
        }

        public void Clear()
        {
            if (screws.Count == 0) return;
            StartCoroutine(ClearCoroutine());
        }

        public IEnumerator ClearToHidden()
        {
            if (_screwManager == null)
            {
                Debug.LogError("[ArrayScrew] _screwManager chưa được inject.");
                yield break;
            }

            var copy = screws.ToList();

            foreach (var screw in copy)
            {
                screw.SetActive(false);
                yield return null;
            }

            _screwManager.AddHiddenScrews(copy);

            foreach (var hold in holdScrews.ToList())
            {
                hold.RemoveScrew();
                yield return null;
            }

            screws.Clear();
        }

        // ─────────────────────────────────────────
        // IArrayScrew — Hold Operations
        // ─────────────────────────────────────────

        public void AddOneHold()
        {
            activeHoldCount++;
            var hold = holdScrews.FirstOrDefault(h => !h.gameObject.activeSelf);
            if (hold == null) return;

            hold.gameObject.SetActive(true);
            totalWidth += 0.5f;
            HoldAlignment();
        }

        public void ShowArrayActive(int count)
        {
            activeHoldCount = count;
            for (int i = 0; i < holdScrews.Count; i++)
                holdScrews[i].gameObject.SetActive(i < count);
        }

        // ─────────────────────────────────────────
        // IArrayScrew — Queries
        // ─────────────────────────────────────────

        public ColorEnum GetDominantColor()
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

        public Vector3 GetLastHoldPosition()
        {
            var hold = holdScrews.LastOrDefault(h => h.gameObject.activeSelf);
            if (hold == null) return Vector3.zero;

            return hold.transform.position + Vector3.up * 0.5f;
        }

        // ─────────────────────────────────────────
        // Game Active flag (thay thế IngameController.ins.IsGameOver)
        // ─────────────────────────────────────────

        /// <summary>
        /// IngameController gọi method này khi state thay đổi.
        /// Thay thế việc check IngameController.ins.IsGameOver trong coroutine.
        /// </summary>
        public void SetGameActive(bool active)
        {
            _isGameActive = active;

            if (!active)
                _stopCheckHold = true; // dừng check loop nếu game không còn active
        }

        // ─────────────────────────────────────────
        // Internal — Hold Logic
        // ─────────────────────────────────────────

        private HoldScrew FindEmptyHold()
            => holdScrews.FirstOrDefault(h => h.gameObject.activeSelf && h.IsEmpty());

        private void AddScrewToHold(ScrewController screw, HoldScrew hold)
        {
            if (_screwManager == null)
            {
                Debug.LogError("[ArrayScrew] _screwManager chưa được inject.");
                return;
            }

            // 1. Thử đưa thẳng vào box phù hợp
            if (TryAddToSuitableBox(screw)) return;

            // 2. Thử xử lý rainbow
            if (TryHandleRainbow(screw)) return;

            // 3. Thêm vào hold bình thường
            AddToHoldFlow(screw, hold);
        }

        private bool TryAddToSuitableBox(ScrewController screw)
        {
            if (_boxQueue == null) return false;

            var suitableBox = _boxQueue.box(screw.GetColor());
            if (suitableBox == null) return false;

            HandleTutorialForBox(suitableBox);

            screw.SetSortingOrderAndLayer(4, "Box");

            _boxQueue.CanAddScrew(screw, suitableBox, out bool canAdd);
            if (canAdd)
                ParentTo(screw, suitableBox.transform);

            return true;
        }

        private bool TryHandleRainbow(ScrewController screw)
        {
            if (screw.GetColor() != ColorEnum.Rainbow) return false;

            SpecialBoxManager.ins.AddSingle(screw);
            _screwManager.RemoveScrew(screw);
            return true;
        }

        private void AddToHoldFlow(ScrewController screw, HoldScrew hold)
        {
            screw.SetSortingOrderAndLayer(4, "Box");

            hold.AddScrew(screw, false, onMoved =>
            {
                HandleTutorialForHold(hold);
                TriggerFullCheck();
            });

            screws.Add(screw);
            _screwManager.RemoveScrew(screw);
        }

        // ─────────────────────────────────────────
        // Internal — Full Check
        // ─────────────────────────────────────────

        private void TriggerFullCheck()
        {
            if (_holdCheckCoroutine != null)
                StopCoroutine(_holdCheckCoroutine);

            _holdCheckCoroutine = StartCoroutine(CheckFullCoroutine());
        }

        private IEnumerator CheckFullCoroutine()
        {
            _stopCheckHold = false;

            while (!_stopCheckHold && _isGameActive)
            {
                bool allFull = IsFull;

                if (allFull)
                {
                    yield return new WaitForSeconds(2f);

                    // Kiểm tra lại sau delay — box có thể đã nhận screw trong lúc chờ
                    bool stillFull = IsFull;
                    bool boxMoving = _boxQueue != null && _boxQueue.hasMovingBox;

                    if (stillFull && !boxMoving)
                    {
                        _stopCheckHold = true;
                        OnArrayFull?.Invoke(); // IngameController xử lý
                        yield break;
                    }
                }

                yield return new WaitForSeconds(2f);
            }
        }

        // ─────────────────────────────────────────
        // Internal — Clear
        // ─────────────────────────────────────────

        private IEnumerator ClearCoroutine()
        {
            for (int i = 0; i < screws.Count; i++)
            {
                ScrewPool.Instance.Pool.ReturnToPool(screws[i]);
                yield return null;
            }

            foreach (var hold in holdScrews)
            {
                hold.RemoveScrew();
                yield return null;
            }

            screws.Clear();
        }

        // ─────────────────────────────────────────
        // Internal — Tutorial helpers
        // ─────────────────────────────────────────

        private void HandleTutorialForBox(Box suitableBox)
        {
            if (!DataAPIController.instance.IsNewPlayer()) return;

            if (suitableBox.RemainingCapacity < 1)
            {
                TutorialTargetRegistry.Register("box_1", suitableBox.transform);
                TutorialEventBus.Emit("Screw.Selected", "red_1");
            }
            else if (suitableBox.RemainingCapacity > 1)
            {
                TutorialTargetRegistry.Register("box_close", suitableBox.transform);
                TutorialEventBus.Emit("Screw.Selected", "red_2");
            }
        }

        private void HandleTutorialForHold(HoldScrew hold)
        {
            if (!DataAPIController.instance.IsNewPlayer()) return;

            TutorialTargetRegistry.Register("array_1", hold.transform);
            TutorialEventBus.Emit("Screw.Selected", "blue_1");
        }

        // ─────────────────────────────────────────
        // Internal — Alignment
        // ─────────────────────────────────────────

        private void HoldAlignment(Action callback = null)
        {
            if (_alignmentCoroutine != null)
                StopCoroutine(_alignmentCoroutine);

            _alignmentCoroutine = StartCoroutine(HoldAlignmentCoroutine(0f, callback));
        }

        private IEnumerator HoldAlignmentCoroutine(float duration, Action callback)
        {
            var activeHolds = holdScrews.Where(h => h.gameObject.activeSelf).ToList();
            if (activeHolds.Count == 0) yield break;

            float spacing = Mathf.Max(0.7f, totalWidth / (activeHolds.Count + 1));
            float totalOccupied = spacing * (activeHolds.Count - 1);
            float startX = -totalOccupied / 2f;

            var from = activeHolds.Select(h => h.transform.localPosition).ToList();
            var to = activeHolds.Select((h, i) =>
                new Vector3(startX + spacing * i, h.transform.localPosition.y, h.transform.localPosition.z)
            ).ToList();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                for (int i = 0; i < activeHolds.Count; i++)
                    activeHolds[i].transform.localPosition = Vector3.Lerp(from[i], to[i], t);
                yield return null;
            }

            for (int i = 0; i < activeHolds.Count; i++)
                activeHolds[i].transform.localPosition = to[i];

            _alignmentCoroutine = null;
            callback?.Invoke();
        }

        // ─────────────────────────────────────────
        // Internal — Utils
        // ─────────────────────────────────────────

        private void ParentTo(ScrewController screw, Transform parent)
        {
            screw.transform.SetParent(parent, false);
            screw.transform.localPosition = Vector3.zero;
        }

        // ─────────────────────────────────────────
        // IResetable
        // ─────────────────────────────────────────

        public void OnReset()
        {
            totalWidth = GameConstants.ArrayWidth;
            _holdCheckCoroutine = null;
            ShowArrayActive(5);
            Clear();
        }
    }
}