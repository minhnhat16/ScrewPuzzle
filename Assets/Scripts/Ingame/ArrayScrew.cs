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
using UnityEngine;
using SFX = SoundManager.SFX;

namespace Ingame
{
    /// <summary>
    /// TempQueue cho screw game.
    /// Implement ITempQueue — logic routing delegate hoàn toàn cho MatchRouter.
    /// ArrayScrew chỉ lo: visual slot, alignment, animation.
    /// </summary>
    public class ArrayScrew : SingletonMono<ArrayScrew>, IResetable, ITempQueue
    {

        // ─────────────────────────────────────────
        // Inspector
        // ─────────────────────────────────────────

        [SerializeField] private int activeSlotCount = 7;
        [SerializeField] private float totalWidth;
        [SerializeField] private List<HoldScrew> holdScrews;

        // ─────────────────────────────────────────
        // Internal state
        // ─────────────────────────────────────────

        private readonly List<ScrewController> _heldScrews = new();

        // ─────────────────────────────────────────
        // Injected
        // ─────────────────────────────────────────

        private MatchRouter _router;
        private ScrewManager _screwManager;
        private IContainerQueue _containerQueue;

        public void Inject(MatchRouter router, ScrewManager screwManager, IContainerQueue containerQueue)
        {
            _router = router;
            _screwManager = screwManager;
            _containerQueue = containerQueue;
        }

        // ─────────────────────────────────────────
        // ITempQueue
        // ─────────────────────────────────────────

        public int ActiveSlotCount => activeSlotCount;
        public bool IsFull => ActiveHolds().All(h => !h.IsEmpty());
        public bool HasAny => _heldScrews.Count > 0;

        public event Action OnQueueFull;

        public void Enqueue(IMatchItem item)
        {
            if (item is not ScrewController screw) return;

            if (!screw.TryLockForMove()) return;

            // Shortcut: thử route thẳng vào container
            var result = _router.TryRoute(item, out var container);

            if (result == MatchRouter.RouteResult.RoutedToContainer)
            {
                SoundHelper.PlaySFX(SFX.ScrewClicked);
                _screwManager.RemoveScrew(screw);
                return;
            }

            // Không route được → tìm slot trống để hold
            var emptyHold = FindEmptyHold();
            if (emptyHold == null)
            {
                // Không có slot → trả lại state
                screw.ResetClickedFlag();
                screw.ReleaseLockForMove();
                return;
            }

            // Rainbow screw → special box, không vào hold thường
            if (screw.GetColor() == ColorEnum.Rainbow)
            {
                SpecialBoxManager.ins.AddSingle(screw);
                _screwManager.RemoveScrew(screw);
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
            _heldScrews.Remove(screw);
        }

        public void Clear()
        {
            if (_heldScrews.Count == 0) return;
            StartCoroutine(ClearCoroutine());
        }

        public IEnumerator ClearToHidden()
        {
            if (_screwManager == null) yield break;

            var copy = _heldScrews.ToList();

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

            _heldScrews.Clear();
        }

        public void AddSlot()
        {
            activeSlotCount++;
            var hold = holdScrews.FirstOrDefault(h => !h.gameObject.activeSelf);
            if (hold == null) return;

            hold.gameObject.SetActive(true);
            totalWidth += 0.5f;
            HoldAlignment();
        }

        public void SetupSlots(int count)
        {
            activeSlotCount = count;
            for (int i = 0; i < holdScrews.Count; i++)
                holdScrews[i].gameObject.SetActive(i < count);
        }

        // ─────────────────────────────────────────
        // Game Active (từ IngameController qua OnStateChanged)
        // ─────────────────────────────────────────

        private bool _isGameActive;
        private Coroutine _fullCheckCoroutine;

        public void SetGameActive(bool active)
        {
            _isGameActive = active;
            if (!active && _fullCheckCoroutine != null)
            {
                StopCoroutine(_fullCheckCoroutine);
                _fullCheckCoroutine = null;
            }
        }

        // ─────────────────────────────────────────
        // Queries (giữ nguyên cho backward compat)
        // ─────────────────────────────────────────

        public ColorEnum GetDominantColor()
        {
            if (_heldScrews.Count == 0) return ColorEnum.Clear;
            return _heldScrews
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

        private HoldScrew FindEmptyHold()
            => holdScrews.FirstOrDefault(h => h.gameObject.activeSelf && h.IsEmpty());

        private IEnumerable<HoldScrew> ActiveHolds()
            => holdScrews.Where(h => h.gameObject.activeSelf);

        private void AddToHoldFlow(ScrewController screw, HoldScrew hold)
        {
            screw.SetSortingOrderAndLayer(4, "Box");

            hold.AddScrew(screw, false, _ =>
            {
                HandleTutorialForHold(hold);
                TriggerFullCheck();
            });

            _heldScrews.Add(screw);
            _screwManager.RemoveScrew(screw);
        }

        private void TriggerFullCheck()
        {
            if (_fullCheckCoroutine != null)
                StopCoroutine(_fullCheckCoroutine);
            _fullCheckCoroutine = StartCoroutine(CheckFullCoroutine());
        }

        private IEnumerator CheckFullCoroutine()
        {
            while (_isGameActive)
            {
                if (IsFull)
                {
                    yield return new WaitForSeconds(2f);

                    bool stillFull = IsFull;
                    bool containerMoving = _containerQueue != null && _containerQueue.HaseMovingContainer;

                    if (stillFull && !containerMoving)
                    {
                        OnQueueFull?.Invoke();
                        yield break;
                    }
                }

                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator ClearCoroutine()
        {
            foreach (var screw in _heldScrews.ToList())
            {
                ScrewPool.Instance.Pool.ReturnToPool(screw);
                yield return null;
            }
            foreach (var hold in holdScrews)
            {
                hold.RemoveScrew();
                yield return null;
            }
            _heldScrews.Clear();
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

        private Coroutine _alignCoroutine;

        internal void HoldAlignment(Action callback = null)
        {
            if (_alignCoroutine != null) StopCoroutine(_alignCoroutine);
            _alignCoroutine = StartCoroutine(AlignCoroutine(0f, callback));
        }

        private IEnumerator AlignCoroutine(float duration, Action callback)
        {
            var active = ActiveHolds().ToList();
            if (active.Count == 0) yield break;

            float spacing = Mathf.Max(0.7f, totalWidth / (active.Count + 1));
            float startX = -spacing * (active.Count - 1) / 2f;

            var from = active.Select(h => h.transform.localPosition).ToList();
            var to = active.Select((h, i) =>
                new Vector3(startX + spacing * i, h.transform.localPosition.y, h.transform.localPosition.z)
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

        public void OnReset()
        {
            totalWidth = GameConstants.ArrayWidth;
            SetupSlots(5);
            Clear();
        }
    }
}