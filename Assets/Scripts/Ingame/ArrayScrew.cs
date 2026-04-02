using Core.Match;
using Enums;
using Ingame.Screw;
using Managers;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using SFX = SoundManager.SFX;

namespace Ingame
{
    public class ArrayScrew : SingletonMono<ArrayScrew>, IResetable, ITempQueue, IArrayScrew
    {
        // ─────────────────────────────────────────
        // Inspector
        // ─────────────────────────────────────────

        [SerializeField] private int activeSlotCount = 7;
        [SerializeField] private float totalWidth;
        [SerializeField] private List<HoldScrew> holdScrews;
        [SerializeField] private float arrayTopOffset = 5.8f;

        // ─────────────────────────────────────────
        // Internal state
        // ─────────────────────────────────────────

        private readonly List<ScrewController> _heldScrews = new();
        private Coroutine _alignCoroutine;
        private Coroutine _fullCheckCoroutine;

        private bool _isGameActive;
        private bool _hasTriggeredFullEvent;
        // ─────────────────────────────────────────
        // Injected
        // ─────────────────────────────────────────

        private MatchRouter _router;
        private ScrewManager _screwManager;
        private IContainerQueue _containerQueue;
        private IPlayer _player;

        public void Inject(MatchRouter router, ScrewManager screwManager, IContainerQueue containerQueue, IPlayer player = null)
        {
            if (_containerQueue != null)
                _containerQueue.OnAllBoxesStopped -= HandleAllStopped;

            _router = router;
            _screwManager = screwManager;
            _containerQueue = containerQueue;
            _player = player;

            if (_containerQueue != null)
                _containerQueue.OnAllBoxesStopped += HandleAllStopped;
        }

        private void OnDisable()
        {
            if (_containerQueue != null)
                _containerQueue.OnAllBoxesStopped -= HandleAllStopped;
        }

        // ─────────────────────────────────────────
        // ITempQueue
        // ─────────────────────────────────────────

        public int ActiveSlotCount => activeSlotCount;
        public bool IsFull => ActiveHolds().All(h => !h.IsEmpty());
        public bool HasAny => HeldScrews.Count > 0;
        public List<ScrewController> HeldScrews => _heldScrews;

        public event Action OnQueueFull;

        public void Enqueue(IMatchItem item)
        {
            if (item is not ScrewController screw) return;
            if (!screw.TryLockForMove()) return;

            var result = _router.TryRoute(item, out var container);
            if (result == MatchRouter.RouteResult.RoutedToContainer)
            {
                SoundHelper.PlaySFX(SFX.ScrewClicked);
                _screwManager.RemoveScrew(screw);

                RequestEvaluateFullState();
                return;
            }

            var emptyHold = FindEmptyHold();
            if (emptyHold == null)
            {
                screw.ResetClickedFlag();
                screw.ReleaseLockForMove();
                return;
            }

            if (screw.GetColor() == ColorEnum.Rainbow)
            {
                SpecialBoxManager.ins.AddSingle(screw);
                _screwManager.RemoveScrew(screw);

                RequestEvaluateFullState();
                return;
            }

            SoundHelper.PlaySFX(SFX.ScrewClicked);
            AddToHoldFlow(screw, emptyHold);
        }

        public void Dequeue(IMatchItem item)
        {
            if (item is not ScrewController screw) return;

            var hold = holdScrews.Find(h => h.Screw == screw);
            if (hold == null) return;

            hold.RemoveScrew();
            HeldScrews.Remove(screw);

            // Reset state flags — screw rời array hold
            screw.ResetHoldState();

            HoldAlignment();
            RequestEvaluateFullState();
        }

        public void Clear()
        {
            if (HeldScrews.Count == 0) return;
            StartCoroutine(ClearCoroutine());
        }

        public IEnumerator ClearToHidden(Action<bool> isClear)
        {
            var copy = HeldScrews.ToList();
            if (copy.Count == 0)
            {
                isClear?.Invoke(false);
                yield break;
            }

            foreach (var screw in copy)
            {
                if (screw == null) continue;
                screw.SetActive(false);
                screw.ResetHoldState();     // Tránh lỗi đè khay khi reload level
                screw.ReleaseLockForMove(); // Mở khoá trạng thái bắt đinh
                yield return null; // tạo độ trễ nhẹ cho cảm giác UI clear
            }

            var lm = LevelManager.ins.layerManager;
            if (copy.Count > 0)
                lm.RemoveScrewsOnDict(copy);

            var sm = LevelManager.ins.ScrewManager;
            var hiddenScrewsToSM = new List<ScrewController>();

            foreach (var screw in copy)
            {
                if (screw.GetColor() == ColorEnum.Rainbow)
                {
                    SpecialBoxManager.ins.AddSingle(screw);
                }
                else
                {
                    // Thử tìm Box phù hợp đang trống
                    var box = BoxQueue.ins.FindSuitableBox(screw.GetColor());
                    
                    // BoxQueue sau này lấy hidden screw từ `ScrewManager.PopHiddenScrew`
                    // Do đó, nếu không có hộp, ta ADD HẲN VÀO ScrewManager:
                    if (box != null && box.TryAddScrew(screw))
                    {
                        continue;
                    }
                    else
                    {
                        screw.MarkDetachedFromBoard();
                        hiddenScrewsToSM.Add(screw); // Lưu lại để đẩy vào bộ nhớ Hidden
                    }
                }
            }

            // Đồng bộ hoá giấu vít vào chung khu vực mà Game đang quản lý Hidden Screw lấy ra
            if (hiddenScrewsToSM.Count > 0)
            {
                sm.AddHiddenScrews(hiddenScrewsToSM);
            }

            foreach (var hold in holdScrews.ToList())
            {
                hold.RemoveScrew();
                yield return null;
            }

            HeldScrews.Clear();
            HoldAlignment();

            ResetFullEventFlag();
            RequestEvaluateFullState();

            isClear?.Invoke(true);
        }

        public void AddSlot()
        {
            activeSlotCount++;

            var hold = holdScrews.FirstOrDefault(h => !h.gameObject.activeSelf);
            if (hold == null) return;

            hold.gameObject.SetActive(true);
            totalWidth += 0.5f;
            HoldAlignment();

            ResetFullEventFlag();
            RequestEvaluateFullState();
        }

        public void SetupSlots(int count)
        {
            activeSlotCount = count;

            for (int i = 0; i < holdScrews.Count; i++)
                holdScrews[i].gameObject.SetActive(i < count);

            ResetFullEventFlag();
            RequestEvaluateFullState(0f);
        }

        // ─────────────────────────────────────────
        // IArrayScrew — explicit implementation
        // ─────────────────────────────────────────

        int IArrayScrew.ActiveHoldCount => ActiveHolds().Count();
        bool IArrayScrew.HasAny() => HeldScrews.Count > 0;

        event Action IArrayScrew.OnArrayFull
        {
            add => OnQueueFull += value;
            remove => OnQueueFull -= value;
        }

        void IArrayScrew.AddScrew(ScrewController screw)
        {
            if (screw == null) return;
            if (!screw.TryLockForMove()) return;

            var result = _router.TryRoute(screw, out _);
            if (result == MatchRouter.RouteResult.RoutedToContainer)
            {
                SoundHelper.PlaySFX(SFX.ScrewClicked);
                _screwManager.RemoveScrew(screw);

                RequestEvaluateFullState();
                return;
            }

            if (screw.GetColor() == ColorEnum.Rainbow)
            {
                SpecialBoxManager.ins.AddSingle(screw);
                _screwManager.RemoveScrew(screw);
                
                RequestEvaluateFullState();
                return;
            }

            var hold = FindEmptyHold();
            if (hold == null)
            {
                Debug.LogWarning("[ArrayScrew] AddScrew: không còn hold trống.");
                screw.ResetClickedFlag();
                screw.ReleaseLockForMove();
                return;
            }

            SoundHelper.PlaySFX(SFX.ScrewClicked);
            AddToHoldFlow(screw, hold);
        }

        void IArrayScrew.RemoveScrew(ScrewController screw) => Dequeue(screw);

        void IArrayScrew.RemoveScrews(IEnumerable<ScrewController> screws)
        {
            foreach (var s in screws)
                Dequeue(s);
        }

        void IArrayScrew.AddOneHold() => AddSlot();
        void IArrayScrew.ShowArrayActive(int activeCount) => SetupSlots(activeCount);

        List<ScrewController> IArrayScrew.TakeByColor(ColorEnum color, int maxCount)
        {
            var taken = new List<ScrewController>();
            if (maxCount <= 0) return taken;

            var matching = HeldScrews
                .Where(s => s != null && s.GetColor() == color)
                .Take(maxCount)
                .ToList();

            foreach (var screw in matching)
            {
                var hold = holdScrews.FirstOrDefault(h => h.Screw == screw);
                if (hold != null) hold.RemoveScrew();

                HeldScrews.Remove(screw);

                // Reset state flags — screw rời array hold, chuẩn bị vào box hold
                screw.ResetHoldState();

                taken.Add(screw);
            }

            if (taken.Count > 0)
            {
                HoldAlignment();
                RequestEvaluateFullState();
            }

            return taken;
        }

        Dictionary<ColorEnum, int> IArrayScrew.GetHeldColorCounts()
        {
            var result = new Dictionary<ColorEnum, int>();

            foreach (var screw in HeldScrews)
            {
                if (screw == null) continue;

                var color = screw.GetColor();
                if (!result.ContainsKey(color))
                    result[color] = 0;

                result[color]++;
            }

            return result;
        }

        HashSet<ColorEnum> IArrayScrew.GetHeldColors()
        {
            var result = new HashSet<ColorEnum>();

            foreach (var screw in HeldScrews)
            {
                if (screw != null)
                    result.Add(screw.GetColor());
            }

            return result;
        }

        // ─────────────────────────────────────────
        // Game Active
        // ─────────────────────────────────────────

        public void SetGameActive(bool active)
        {
            _isGameActive = active;

            Debug.Log($"[ArrayScrew] SetGameActive: active={active}, _isGameActive={_isGameActive}");

            if (!active)
            {
                if (_fullCheckCoroutine != null)
                {
                    StopCoroutine(_fullCheckCoroutine);
                    _fullCheckCoroutine = null;
                }

                ResetFullEventFlag();
            }
            else
            {
                RequestEvaluateFullState();
            }
        }

        // ─────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────

        public ColorEnum GetDominantColor()
        {
            if (HeldScrews.Count == 0) return ColorEnum.Clear;

            return HeldScrews
                .Where(s => s != null)
                .GroupBy(s => s.GetColor())
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
        }

        public Vector3 GetLastHoldPosition()
        {
            var hold = holdScrews.LastOrDefault(h => h.gameObject.activeSelf);
            return hold == null ? Vector3.zero : hold.transform.position + Vector3.up * 0.5f;
        }

        // ─────────────────────────────────────────
        // Internal
        // ─────────────────────────────────────────

        private void Start()
        {
            SetupSlots(activeSlotCount);
            HoldAlignment();
        }

        private HoldScrew FindEmptyHold() =>
            holdScrews.FirstOrDefault(h => h.gameObject.activeSelf && h.IsEmpty());

        private IEnumerable<HoldScrew> ActiveHolds() =>
            holdScrews.Where(h => h.gameObject.activeSelf);

        private void AddToHoldFlow(ScrewController screw, HoldScrew hold)
        {
            var lm = LevelManager.ins.layerManager;

            HeldScrews.Add(screw);
            screw.SetSortingOrderAndLayer(4, "Box");

            hold.AddScrew(screw, false, _ =>
            {
                HandleTutorialForHold(hold);

                if (IsFull)
                    _player?.LockInput();

                RequestEvaluateFullState();
            });
        }

        private void HandleAllStopped()
        {
            Debug.Log($"[ArrayScrew] HandleAllStopped triggered. _isGameActive={_isGameActive}, IsFull={IsFull}");

            if (!_isGameActive) return;

            RequestEvaluateFullState(0.2f);
        }

        private void RequestEvaluateFullState(float delay = 1f)
        {
            if (!_isGameActive) return;

            if (_fullCheckCoroutine != null)
                StopCoroutine(_fullCheckCoroutine);

            _fullCheckCoroutine = StartCoroutine(EvaluateFullStateCoroutine(delay));
        }

        private IEnumerator EvaluateFullStateCoroutine(float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            _fullCheckCoroutine = null;

            bool isFull = IsFull;
            bool containerMoving = _containerQueue != null && _containerQueue.HasMovingBox();

            Debug.Log($"[ArrayScrew] EvaluateFullState => isFull={isFull}, containerMoving={containerMoving}, triggered={_hasTriggeredFullEvent}");

            if (!isFull)
            {
                ResetFullEventFlag();
                _player?.UnlockInput();
                Debug.Log("[ArrayScrew] Array no longer full — input unlocked.");
                yield break;
            }

            _player?.LockInput();

            if (containerMoving)
            {
                Debug.Log("[ArrayScrew] Array full but containers are still moving. Waiting...");
                yield break;
            }

            if (_hasTriggeredFullEvent)
            {
                Debug.Log("[ArrayScrew] Full event already triggered. Skip duplicate invoke.");
                yield break;
            }

            _hasTriggeredFullEvent = true;
            Debug.Log("[ArrayScrew] Queue full confirmed. Invoke OnQueueFull.");
            OnQueueFull?.Invoke();
        }

        private void ResetFullEventFlag()
        {
            _hasTriggeredFullEvent = false;
        }

        private IEnumerator ClearCoroutine()
        {
            foreach (var screw in HeldScrews.ToList())
            {
                ScrewPool.Instance.ReturnScrewToPool(screw);
                yield return null;
            }

            foreach (var hold in holdScrews)
            {
                hold.RemoveScrew();
                yield return null;
            }

            HeldScrews.Clear();
            HoldAlignment();

            ResetFullEventFlag();
            RequestEvaluateFullState();
        }

        private void HandleTutorialForHold(HoldScrew hold)
        {
            if (!DataAPIController.instance.IsNewPlayer()) return;

            TutorialTargetRegistry.Register("array_1", hold.transform);
            TutorialEventBus.Emit("Screw.Selected", "blue_1");
        }

        // ─────────────────────────────────────────
        // Alignment
        // ─────────────────────────────────────────

        internal void HoldAlignment(Action callback = null)
        {
            if (_alignCoroutine != null)
                StopCoroutine(_alignCoroutine);

            _alignCoroutine = StartCoroutine(AlignCoroutine(0f, callback));
        }

        private IEnumerator AlignCoroutine(float duration, Action callback)
        {
            var active = ActiveHolds().ToList();
            if (active.Count == 0) yield break;

            float spacing = Mathf.Max(0.7f, totalWidth / (active.Count + 1));
            float startX = -spacing * (active.Count - 1) / 2f;

            // Cùng kiểu BoxSlot: anchor theo top camera và convert world->local
            float targetY = GetTopAnchoredLocalY(active[0].transform, arrayTopOffset);

            var from = active.Select(h => h.transform.localPosition).ToList();
            var to = active.Select((h, i) =>
                new Vector3(startX + spacing * i, targetY, h.transform.localPosition.z)
            ).ToList();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                for (int i = 0; i < active.Count; i++)
                    active[i].transform.localPosition = Vector3.Lerp(from[i], to[i], t);

                yield return null;
            }

            for (int i = 0; i < active.Count; i++)
                active[i].transform.localPosition = to[i];

            _alignCoroutine = null;
            callback?.Invoke();
        }

        // ─────────────────────────────────────────
        // IResetable
        // ─────────────────────────────────────────

        public void ClearHeldScrewImidiate()
        {
            foreach (var screw in HeldScrews.ToList())
                ScrewPool.Instance.ReturnScrewToPool(screw);

            foreach (var hold in holdScrews)
                hold.RemoveScrew();

            HeldScrews.Clear();
            ResetFullEventFlag();
        }

        public void OnReset()
        {
            totalWidth = GameConstants.ArrayWidth;
            SetupSlots(5);
            HoldAlignment();
            ClearHeldScrewImidiate();
        }

        private static float GetTopAnchoredLocalY(Transform target, float topOffset)
        {
            if (CameraMain.instance == null || CameraMain.instance.main == null)
                return target.localPosition.y;

            float targetWorldY = CameraMain.instance.GetTop() - topOffset;

            var parent = target.parent;
            if (parent == null) return targetWorldY;

            Vector3 worldPos = target.position;
            worldPos.y = targetWorldY;

            return parent.InverseTransformPoint(worldPos).y;
        }
    }
}