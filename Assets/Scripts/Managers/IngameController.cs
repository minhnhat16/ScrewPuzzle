using Enums;
using Ingame;
using Ingame.Screw;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class IngameController : SingletonMono<IngameController>
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour boxQueueBehaviour;
        [SerializeField] private MonoBehaviour gameFlowBehaviour;
        [SerializeField] private Player player;

        private IBoxQueue _boxQueue;
        private IGameFlowService _gameFlow;
        private ItemController itemController;
        private ScrewManager screwManager;

        [Header("State")]
        [SerializeField] private bool isGameOver;
        [SerializeField] private bool isPaused;

        [Header("Star")]
        [SerializeField] private int currentStar;
        [SerializeField] private int totalStarInLevel;


        [Header("Item Event")]
        public UnityEvent<ItemType, Vector3> OnItemInvoke;

        public UnityEvent<float> OnStarChanged = new();
        public UnityEvent<bool> OnLevelCompleted = new();
        public UnityEvent OnGameOverEvent = new();

        public bool IsGameOver { get => isGameOver; set => isGameOver = value; }
        public bool IsPaused { get => isPaused; set => isPaused = value; }
        public BoxQueue BoxQueue => (BoxQueue)_boxQueue;

        public ScrewManager ScrewManager => screwManager;

        #region Initialization

        public override void Awake()
        {
            base.Awake();
            _boxQueue = (IBoxQueue)boxQueueBehaviour;
            _gameFlow = (IGameFlowService)gameFlowBehaviour;
        }

        private void OnEnable()
        {
            OnLevelCompleted.AddListener(HandleLevelComplete);
            OnItemInvoke.AddListener(InvokeItem);
            SpecialBoxManager.ins.OnScrewCollected += HandleScrewCollected;
            SpecialBoxManager.ins.OnBoxColorCountChanged += HandleColorChanged;
        }

        private void OnDisable()
        {
            OnLevelCompleted.RemoveListener(HandleLevelComplete);
            OnItemInvoke.AddListener(InvokeItem);
        }

        #endregion

        #region Level Flow

        public void StartLevel()
        {
            IsGameOver = false;
            currentStar = 0;
            player.IsInputLocked = false;

            _boxQueue.Initialize(false);
        }

        private void HandleLevelComplete(bool completed)
        {
            if (!completed) return;

            player.IsInputLocked = true;
            _gameFlow.CompleteLevel();
        }

        public void TriggerGameOver()
        {
            if (IsGameOver) return;

            IsGameOver = true;
            player.IsInputLocked = true;

            OnGameOverEvent?.Invoke();
            _gameFlow.HandleGameOver();
        }

        public void RestartLevel(int levelId)
        {
            IsGameOver = false;
            currentStar = 0;

            _gameFlow.RestartLevel(levelId);
        }

        #endregion

        #region Star System

        public void AddStar(int amount)
        {
            currentStar += amount;

            float percent = (float)currentStar / totalStarInLevel;
            OnStarChanged?.Invoke(percent);
        }

        #endregion

        #region Pause

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0;
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1;
        }

        internal void Revive()
        {
            bool hasActiveBox = _boxQueue.ActiveBoxCount > 4;
            if (hasActiveBox)
            {
                _gameFlow.HandleRevive();
            }
            else
            {
                _gameFlow.HandleGameOver();
            }
        }


        public void ReturnHome()
        {
            DialogManager.ins.HideAllDialog();
            LoadSceneManager.ins.LoadSceneByName("Buffer", () =>
            {
                ViewManager.Instance.SwitchView(ViewIndex.MainScreenView);
            });
        }
        #endregion


        #region Item Invocation
        public void InvokeItem(ItemType type, Vector3 position)
        {
            if (itemController.IsItemExecuting) return;

            switch (type)
            {
                case ItemType.Breaker:
                    itemController.GotoState(itemController.RemovePartState);
                    itemController.SetSelected(true);
                    break;

                case ItemType.AddBox:
                    itemController.GotoState(itemController.AddBoxState);
                    break;
                case ItemType.Magnet:
                    itemController.GotoState(itemController.MagnetState);
                    break;
                case ItemType.Drill:
                    itemController.GotoState(itemController.AddOneHold);
                    break;
                case ItemType.Gold:
                    break;
                case ItemType.Ticket:
                    break;
            }
        }

        internal void Lose()
        {
        }
        #endregion

        #region Special Box Handlers
        private void HandleScrewCollected(ColorEnum color)
        {
            AddStar(1);
            SideMissionManager.ins.UpdateMission(1);
            MissionManager.ins.ProcessCollectScrew(color, 1);
        }

        private void HandleColorChanged(ColorEnum color, int count)
        {
            ViewManager.Instance.UpdateSpecialBoxCount(color, count);
        }
        #endregion

        #region Box Flow

        private void ResolveHiddenForBox(Box box)
        {
            if (box == null || box.IsFull || box.IsLocked)
                return;

            int capacityLeft = box.RemainingCapacity;

            var screws = screwManager.PopHiddenScrew(box.Color, capacityLeft);

            if (screws == null || screws.Count == 0)
                return;

            foreach (var screw in screws)
            {
                bool added = box.TryAddScrew(screw);

                if (!added)
                {
                    screwManager.AddHiddenScrews(new List<ScrewController> { screw });
                    break;
                }
            }
        }
        #endregion
    }
}