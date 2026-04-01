using Core.Match;
using Enums;
using Ingame.Pools;
using Ingame.Screw;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    public class Box : FSMSystem, IMatchContainer, IResetable
    {
        #region Inspector

        [SerializeField] private ColorEnum boxColor;
        [SerializeField] private int capacity = 3;

        #endregion

        #region Properties

        public ColorEnum Color => boxColor;
        public bool IsFull => storage != null && storage.IsFull;
        public bool IsLocked => lockController != null && lockController.IsLocked;
        public bool IsMoving => mover != null && mover.IsMoving;
        public bool IsBusy => IsMoving || (stateController != null && stateController.CurrentState == BoxState.Full);
        public int RemainingCapacity => storage.RemainingCapacity;

        #endregion

        #region Events

        public event Action<Box> OnBoxReady;
        public event Action<Box> OnBoxFull;
        public event Action<Box> OnBoxRemoved;


        private event System.Action<Core.Match.IMatchContainer> _onCompleted;

        #endregion

        #region Components

        private BoxStateController stateController;
        private BoxScrewStorage storage;
        private BoxLockController lockController;
        private IBoxAnimator animator;
        private IBoxReward rewardSpawner;
        private IMovable mover;
        private BoxRenderer boxRenderer;

        #endregion

        #region Unity

        private void Awake()
        {
            stateController = GetComponent<BoxStateController>();
            storage = GetComponent<BoxScrewStorage>();
            lockController = GetComponent<BoxLockController>();
            animator = GetComponent<IBoxAnimator>();
            rewardSpawner = GetComponent<IBoxReward>();
            mover = GetComponent<IMovable>();
            boxRenderer = GetComponent<BoxRenderer>();
            storage.Initialize(capacity, boxColor);
        }

        #endregion

       #region Initialization

        public void Initialize(ColorEnum color, int customCapacity = -1)
        {
            boxColor = color;

            if (customCapacity > 0)
                capacity = customCapacity;

            animator?.KillAllAnimations();

            storage.Initialize(capacity, boxColor);
            boxRenderer?.SetColor(color);
            stateController.SetState(BoxState.Ready);

            OnBoxReady?.Invoke(this);
        }

        /// <summary>
        /// Called by BoxQueue when the box has finished moving into slot / is about to be used.
        /// Ensure any stored screws are active and properly configured.
        /// </summary>
        public void OnActivated()
        {
            try
            {
                storage?.ActivateAllScrews();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Box] OnActivated failed: {ex.Message}");
            }
        }

        #endregion

        #region Screw Handling

        /// <summary>
        /// Add screw với animation jump (flow bình thường).
        /// </summary>
        public bool TryAddScrew(ScrewController screw)
        {
            if (IsLocked) return false;
            if (!stateController.IsReady) return false;

            bool added = storage.TryAdd(screw, isTele: false, onComplete: () =>
            {
                if (storage.IsFull)
                    HandleFull();
            });

            return added;
        }

        /// <summary>
        /// Add screw KHÔNG animate — set position ngay lập tức.
        /// Dùng cho hidden screw resolve trước khi box move vào slot.
        /// </summary>
        public bool TryAddScrewImmediate(ScrewController screw)
        {
            if (IsLocked) return false;
            if (!stateController.IsReady) return false;

            bool added = storage.TryAdd(screw, isTele: true, onComplete: () =>
            {
                if (storage.IsFull)
                    HandleFull();
            });

            return added;
        }

        public bool TryAddScrews(List<ScrewController> screws, bool isTele = false)
        {
            if (IsLocked) return false;
            if (!stateController.IsReady) return false;

            bool allAdded = true;
            foreach (var screw in screws)
            {
                bool added = storage.TryAdd(screw, isTele,onComplete: () =>
                {
                    if (storage.IsFull)
                        HandleFull();
                });

                if (!added)
                {
                    allAdded = false;
                    break;
                }
            }

            return allAdded;
        }

     
        #endregion

        #region Full Flow

        private void HandleFull()
        {
            stateController.SetState(BoxState.Full);

            animator.PlayCloseAnimation(() =>
            {
                // Lấy vị trí từng holdSlot đang có screw → spawn star tại đó
                var positions = storage.GetOccupiedSlotWorldPositions();
                rewardSpawner?.SpawnReward(positions);

                animator.PlayExitAnimation(() =>
                {
                    OnBoxFull?.Invoke(this);
                });
            });
        }

        #endregion

        #region Lock

        public void Lock() => lockController.Lock();
        public void Unlock() => lockController.Unlock();

        #endregion

        #region Movement

        public void MoveTo(Vector3 target, float duration, Action onComplete = null)
        {
            stateController.SetState(BoxState.Moving);

            mover?.MoveTo(target, duration, DG.Tweening.Ease.OutCubic, () =>
            {
                stateController.SetState(BoxState.Ready);
                onComplete?.Invoke();
            });
        }

        #endregion

        #region Remove

        public void Remove()
        {
            stateController.SetState(BoxState.Removed);
            OnBoxRemoved?.Invoke(this);
        }

        #endregion

        #region Reset

        public void Clear()
        {
            storage.Clear();
            stateController.SetState(BoxState.Idle);
        }

        #endregion

        #region Utilities
        private Vector3 GetExitPosition()
        {
            // Move box to the right and down (adjust as needed for your layout)
            return transform.position + new Vector3(5f, -3f, 0f);
        }
        #endregion

        #region IMatchContainer

        string IMatchContainer.AcceptedTag
            => Color.ToString().ToLower();

        int IMatchContainer.Count
            => capacity - RemainingCapacity;          // capacity là field private đã có

        int IMatchContainer.Capacity
            => capacity;

        int IMatchContainer.RemainingCapacity
            => RemainingCapacity;                     // property đã có

        bool IMatchContainer.IsFull => IsFull;
        bool IMatchContainer.IsLocked => IsLocked;
        bool IMatchContainer.IsMoving => IsMoving;

        Vector3 IMatchContainer.Position
            => transform.position;

        event System.Action<Core.Match.IMatchContainer> IMatchContainer.OnCompleted
        {
            add => _onCompleted += value;
            remove => _onCompleted -= value;
        }

        bool IMatchContainer.TryAdd(IMatchItem item)
        {
            if (item is not Ingame.Screw.ScrewController screw) return false;
            return TryAddScrew(screw);
        }

        int IMatchContainer.TryAddRange(IEnumerable<IMatchItem> items)
        {
            int count = 0;
            foreach (var item in items)
                if (((IMatchContainer)this).TryAdd(item)) count++;
            return count;
        }

        internal void ClearActionBoxFull()
        {
            OnBoxFull = null;
        }

        public void OnReset()
        {
            Clear();
            _onCompleted = null;
            OnBoxReady = null;
            OnBoxFull = null;
            OnBoxRemoved = null;
        }

        #endregion
    }
}
