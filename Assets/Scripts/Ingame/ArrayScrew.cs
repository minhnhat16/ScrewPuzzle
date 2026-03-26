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
        private IPlayer _player; // ← thêm

        public void Inject(MatchRouter router, ScrewManager screwManager, IContainerQueue containerQueue, IPlayer player = null)
        {
            _router = router;
            _screwManager = screwManager;
            _containerQueue = containerQueue;
            _player = player;
        }

        // ─────────────────────────────────────────
        // ITempQueue
        // ─────────────────────────────────────────

        public int ActiveSlotCount => activeSlotCount;
        public bool IsFull => ActiveHolds().All(h => !h.IsEmpty());
        public bool HasAny => HeldScrews.Count > 0;

        public event Action OnQueueFull;

        public void Enqueue(IMatchItem item)
        {
            if (item is not ScrewController screw) return;
            if (!screw.TryLockForMove()) return;

            var result = _router.TryRoute(item, out var container);
            if (result == MatchRouter.RouteResult.RoutedToContainer)
            {
                SoundHelper.PlaySFX(SFX.ScrewClicked);
                // Chỉ remove screw khi đã add vào box thành công
                var box = container as Box;
                if (box != null && box.TryAddScrew(screw))
                {
                    _screwManager.RemoveScrew(screw);
                    return;
                }

                // Route result có box nhưng add fail tại frame hiện tại
                // → phải release lock để screw không bị kẹt không click được.
                screw.ResetClickedFlag();
                screw.ReleaseLockForMove();
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
            // Ẩn visual từng screw
            foreach (var screw in copy)
            {
                if (screw == null) continue;
                screw.SetActive(false);
                yield return null;
            }

            var lm = LevelManager.ins.layerManager;
            if (copy.Count > 0)
            {
                lm.RemoveScrewsOnDict(copy);
            }

            // Add into BoxQueue._hiddenByColor (nguồn 1)
            // → ResolveAllHiddenForBox sẽ pick up khi box màu phù hợp spawn
            foreach (var screw in copy)
                BoxQueue.ins.TryProcessItemScrew(screw);

            // Clear holds
            foreach (var hold in holdScrews.ToList())
            {
                hold.RemoveScrew();
                yield return null;
            }

            HeldScrews.Clear();
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
        }

        public void SetupSlots(int count)
        {
            activeSlotCount = count;
            for (int i = 0; i < holdScrews.Count; i++)
                holdScrews[i].gameObject.SetActive(i < count);
        }

        // ─────────────────────────────────────────
        // IArrayScrew — explicit implementation
        // ─────────────────────────────────────────

        int IArrayScrew.ActiveHoldCount => ActiveHolds().Count();

        public List<ScrewController> HeldScrews => _heldScrews;

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
                screw.ResetClickedFlag();
                screw.ReleaseLockForMove();
                return;
            }

            if (screw.GetColor() == ColorEnum.Rainbow)
            {
                SpecialBoxManager.ins.AddSingle(screw);
                _screwManager.RemoveScrew(screw);
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
            foreach (var s in screws) Dequeue(s);
        }

        void IArrayScrew.AddOneHold() => AddSlot();
        void IArrayScrew.ShowArrayActive(int activeCount) => SetupSlots(activeCount);

        /// <summary>
        /// Lấy tối đa <paramref name="maxCount"/> screw cùng màu <paramref name="color"/>
        /// ra khỏi array (xóa hold, trả về list để BoxQueue add vào box).
        /// </summary>
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
                HoldAlignment();

            return taken;
        }
        // ─────────────────────────────────────────
        // Game Active
        // ─────────────────────────────────────────

        private bool _isGameActive;
        private Coroutine _fullCheckCoroutine;

        public void SetGameActive(bool active)
        {
            _isGameActive = active;

            Debug.Log($"[ArrayScrew] SetGameActive: active={active}, _isGameActive={_isGameActive}");
            if (!active && _fullCheckCoroutine != null)
            {
                StopCoroutine(_fullCheckCoroutine);
                _fullCheckCoroutine = null;
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
            // Remove from LayerManager.screwDict so visibility controller won't re-activate this screw
            var lm = LevelManager.ins.layerManager;

            HeldScrews.Add(screw);
            screw.SetSortingOrderAndLayer(4, "Box");
            hold.AddScrew(screw, false, _ =>
            {
                HandleTutorialForHold(hold);
                if (IsFull) _player?.LockInput();
                TriggerFullCheck();
            });
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
                    // Đợi cho đến khi không còn box nào đang move
                    while (_containerQueue.HasMovingBox())
                    {
                        Debug.Log("[ArrayScrew] CheckFullCoroutine: waiting for moving containers to stop...");
                        yield return new WaitForSeconds(2f);
                    }

                    yield return new WaitForSeconds(3f);

                    bool stillFull = IsFull;
                    bool containerMoving = _containerQueue != null && _containerQueue.HasMovingBox();

                    Debug.Log($"[ArrayScrew] CheckFullCoroutine: stillFull={stillFull}, containerMoving={containerMoving}");

                    if (stillFull && !containerMoving)
                    {
                        // Vẫn full sau 2s → game over / revive flow
                        OnQueueFull?.Invoke();
                        yield break;
                    }
                    else if (!stillFull)
                    {
                        // Slot đã được giải phóng (box spawned, screw taken) → unlock input
                        _player?.UnlockInput();
                        Debug.Log("[ArrayScrew] Array no longer full — input unlocked.");
                    }
                }

                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator ClearCoroutine()
        {
            foreach (var screw in HeldScrews.ToList())
            {
                ScrewPool.Instance.Pool.ReturnToPool(screw);
                yield return null;
            }
            foreach (var hold in holdScrews)
            {
                hold.RemoveScrew();
                yield return null;
            }
            HeldScrews.Clear();
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
        // Thêm vào phần IArrayScrew explicit implementation

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
        // IResetable
        // ─────────────────────────────────────────
        public void ClearHeldScrewImidiate()
        {
            foreach (var screw in HeldScrews.ToList())
                ScrewPool.Instance.Pool.ReturnToPool(screw);
            foreach (var hold in holdScrews)
                hold.RemoveScrew();
            HeldScrews.Clear();
        }
        public void OnReset()
        {
            totalWidth = GameConstants.ArrayWidth;
            SetupSlots(5);
            HoldAlignment();
            ClearHeldScrewImidiate();
        }


    }
}
