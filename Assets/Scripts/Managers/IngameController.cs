using Ingame;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

        [Header("State")]
        [SerializeField] private bool isGameOver;
        [SerializeField] private bool isPaused;

        [Header("Star")]
        [SerializeField] private int currentStar;
        [SerializeField] private int totalStarInLevel;

        public UnityEvent<float> OnStarChanged = new();
        public UnityEvent<bool> OnLevelCompleted = new();
        public UnityEvent OnGameOverEvent = new();

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
        }

        private void OnDisable()
        {
            OnLevelCompleted.RemoveListener(HandleLevelComplete);
        }

        #endregion

        #region Level Flow

        public void StartLevel()
        {
            isGameOver = false;
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
            if (isGameOver) return;

            isGameOver = true;
            player.IsInputLocked = true;

            OnGameOverEvent?.Invoke();
            _gameFlow.HandleGameOver();
        }

        public void RestartLevel(int levelId)
        {
            isGameOver = false;
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
            isPaused = true;
            Time.timeScale = 0;
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1;
        }

        internal void Revive()
        {
            _gameFlow.HandleRevive();
        }

        #endregion
    }
}