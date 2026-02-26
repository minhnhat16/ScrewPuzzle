using Enums;
using Ingame.Screw;
using System;
using UnityEngine;

namespace Ingame
{
    public class Box : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private ColorEnum boxColor;
        [SerializeField] private int capacity = 3;

        #endregion

        #region Properties

        public ColorEnum Color => boxColor;
        public bool IsFull => storage.IsFull;
        public bool IsLocked => lockController.IsLocked;
        public bool IsMoving => mover != null && mover.IsMoving;
        public int RemainingCapacity => storage.RemainingCapacity;

        #endregion

        #region Events

        public event Action<Box> OnBoxReady;
        public event Action<Box> OnBoxFull;
        public event Action<Box> OnBoxRemoved;

        #endregion

        #region Components

        private BoxStateController stateController;
        private BoxScrewStorage storage;
        private BoxLockController lockController;
        private IBoxAnimator animator;
        private IBoxReward rewardSpawner;
        private IMovable mover;

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

            storage.Initialize(capacity, boxColor);
        }

        #endregion

        #region Initialization

        public void Initialize(ColorEnum color, int customCapacity = -1)
        {
            boxColor = color;

            if (customCapacity > 0)
                capacity = customCapacity;

            storage.Initialize(capacity, boxColor);
            stateController.SetState(BoxState.Ready);

            OnBoxReady?.Invoke(this);
        }

        #endregion

        #region Screw Handling

        public bool TryAddScrew(ScrewController screw)
        {
            if (IsLocked) return false;
            if (!stateController.IsReady) return false;

            bool added = storage.TryAdd(screw);
            if (!added) return false;

            if (storage.IsFull)
                HandleFull();

            return true;
        }

        #endregion

        #region Full Flow

        private void HandleFull()
        {
            stateController.SetState(BoxState.Full);

            animator?.PlayCloseAnimation();
            rewardSpawner?.SpawnReward(transform.position);

            OnBoxFull?.Invoke(this);
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
    }
}