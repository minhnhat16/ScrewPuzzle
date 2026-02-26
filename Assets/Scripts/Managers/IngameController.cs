using Core.Match;
using Enums;
using Gameplay.StateMachine;
using Ingame;
using Ingame.Screw;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class IngameController : SingletonMono<IngameController>
    {
        // ─────────────────────────────────────────
        // Inspector
        // ─────────────────────────────────────────

        [Header("State Machine")]
        [SerializeField] private GameStateMachineBootstrapper stateMachineBoot;

        [Header("References")]
        [SerializeField] private BoxQueue boxQueue;
        [SerializeField] private ArrayScrew arrayScrew;
        [SerializeField] private Player player;

        // ─────────────────────────────────────────
        // Private — tất cả dùng interface Core layer
        // ─────────────────────────────────────────

        private IGameStateMachine _stateMachine;
        private IContainerQueue _containerQueue;   // thay IBoxQueue
        private ITempQueue _tempQueue;         // thay IArrayScrew
        private IGameFlowService _gameFlow;
        private ItemController _itemController;
        private ScrewManager _screwManager;

        // ─────────────────────────────────────────
        // Star
        // ─────────────────────────────────────────

        [Header("Star")]
        [SerializeField] private int currentStar;
        [SerializeField] private int totalStarInLevel;

        // ─────────────────────────────────────────
        // Public Events
        // ─────────────────────────────────────────

        [Header("Events")]
        public UnityEvent<ItemType, Vector3> OnItemInvoke = new();
        public UnityEvent<float> OnStarChanged = new();
        public UnityEvent<bool> OnLevelCompleted = new();

        // ─────────────────────────────────────────
        // Public Properties
        // ─────────────────────────────────────────

        public bool IsGameOver => _stateMachine.IsIn(GameplayState.Lose);
        public bool IsPaused => _stateMachine.IsIn(GameplayState.Paused);
        public bool IsPlaying => _stateMachine.IsPlaying();

        // Giữ lại BoxQueue public property vì một số UI/system cũ vẫn cần
        public BoxQueue BoxQueue => boxQueue;
        public ScrewManager ScrewManager => _screwManager;

        // ─────────────────────────────────────────
        // Initialization
        // ─────────────────────────────────────────

        public override void Awake()
        {
            base.Awake();

            _stateMachine = stateMachineBoot;
            _containerQueue = boxQueue;              // BoxQueue implement IContainerQueue
            _itemController = GetComponentInChildren<ItemController>();
            _screwManager = GetComponentInChildren<ScrewManager>();
            _tempQueue = ArrayScrew.ins;   // ArrayScrew implement ITempQueue

            _gameFlow = new GameFlowService(
                containerQueue: _containerQueue,
                arrayScrew: _tempQueue,
                levelManager: LevelManager.ins,
                dialogService: DialogManager.ins,
                player: player
            );
        }

        private void OnEnable()
        {
            _stateMachine.OnStateChanged += HandleStateChanged;
            _tempQueue.OnQueueFull += TriggerQueueFull;

            OnLevelCompleted.AddListener(HandleLevelComplete);
            OnItemInvoke.AddListener(InvokeItem);

            SpecialBoxManager.ins.OnScrewCollected += HandleScrewCollected;
            SpecialBoxManager.ins.OnBoxColorCountChanged += HandleColorChanged;
        }

        private void OnDisable()
        {
            _stateMachine.OnStateChanged -= HandleStateChanged;
            _tempQueue.OnQueueFull -= TriggerQueueFull;

            OnLevelCompleted.RemoveListener(HandleLevelComplete);
            OnItemInvoke.RemoveListener(InvokeItem);

            SpecialBoxManager.ins.OnScrewCollected -= HandleScrewCollected;
            SpecialBoxManager.ins.OnBoxColorCountChanged -= HandleColorChanged;
        }

        // ─────────────────────────────────────────
        // State Change Handler
        // ─────────────────────────────────────────

        private void HandleStateChanged(GameplayState prev, GameplayState next)
        {
            player.IsInputLocked = next != GameplayState.Playing
                                && next != GameplayState.ItemUsing;

            ArrayScrew.ins.SetGameActive(
                next == GameplayState.Playing || next == GameplayState.ItemUsing
            );

            switch (next)
            {
                case GameplayState.Paused: _gameFlow.PauseGame(); break;
                case GameplayState.Win: _gameFlow.CompleteLevel(); break;
                case GameplayState.RevivePrompt: _gameFlow.HandleRevive(); break;
                case GameplayState.Lose: _gameFlow.HandleGameOver(); break;
            }
        }

        // ─────────────────────────────────────────
        // Level Flow
        // ─────────────────────────────────────────

        public void StartLevel()
        {
            currentStar = 0;
            _containerQueue.Initialize(false);
            _stateMachine.TransitionTo(GameplayState.Playing);
        }

        private void HandleLevelComplete(bool completed)
        {
            if (!completed) return;
            _stateMachine.TransitionTo(GameplayState.Win);
        }

        private void TriggerQueueFull()
        {
            if (!_stateMachine.IsPlaying()) return;

            bool canRevive = _containerQueue.ActiveCount < 4;
            _stateMachine.TransitionTo(canRevive
                ? GameplayState.RevivePrompt
                : GameplayState.Lose);
        }

        public void RestartLevel(int levelId)
        {
            currentStar = 0;
            _stateMachine.TransitionTo(GameplayState.Loading);
            _gameFlow.RestartLevel(levelId);
        }

        // ─────────────────────────────────────────
        // Pause / Resume / Revive / Return
        // ─────────────────────────────────────────

        public void Pause() => _stateMachine.TransitionTo(GameplayState.Paused);
        public void Resume() => _stateMachine.TransitionTo(GameplayState.Playing);
        public void DeclineRevive() => _stateMachine.TransitionTo(GameplayState.Lose);

        public void Revive()
        {
            _gameFlow.HandleRevive();
            _stateMachine.TransitionTo(GameplayState.Playing);
        }

        public void ReturnHome()
        {
            _stateMachine.TransitionTo(GameplayState.Idle);
            _gameFlow.HandleReturnToMenu();
        }

        // ─────────────────────────────────────────
        // Item
        // ─────────────────────────────────────────

        public void InvokeItem(ItemType type, Vector3 position)
        {
            if (!_stateMachine.IsPlaying()) return;
            if (_itemController.IsItemExecuting) return;

            _stateMachine.TransitionTo(GameplayState.ItemUsing);

            switch (type)
            {
                case ItemType.Breaker:
                    _itemController.GotoState(_itemController.RemovePartState);
                    _itemController.SetSelected(true);
                    break;
                case ItemType.AddBox:
                    _itemController.GotoState(_itemController.AddBoxState);
                    break;
                case ItemType.Magnet:
                    _itemController.GotoState(_itemController.MagnetState);
                    break;
                case ItemType.Drill:
                    _itemController.GotoState(_itemController.AddOneHold);
                    break;
            }
        }

        public void OnItemFinished()
        {
            if (_stateMachine.IsIn(GameplayState.ItemUsing))
                _stateMachine.TransitionTo(GameplayState.Playing);
        }

        // ─────────────────────────────────────────
        // Star
        // ─────────────────────────────────────────

        public void AddStar(int amount)
        {
            currentStar += amount;
            OnStarChanged?.Invoke((float)currentStar / totalStarInLevel);
        }

        // ─────────────────────────────────────────
        // Special Box
        // ─────────────────────────────────────────

        private void HandleScrewCollected(ColorEnum color)
        {
            AddStar(1);
            SideMissionManager.ins.UpdateMission(1);
            MissionManager.ins.ProcessCollectScrew(color, 1);
        }

        private void HandleColorChanged(ColorEnum color, int count)
            => ViewManager.Instance.UpdateSpecialBoxCount(color, count);

        // ─────────────────────────────────────────
        // Hidden screw resolve
        // ─────────────────────────────────────────

        private void ResolveHiddenForBox(Box box)
        {
            if (box == null || box.IsFull || box.IsLocked) return;

            var screws = _screwManager.PopHiddenScrew(box.Color, box.RemainingCapacity);
            if (screws == null || screws.Count == 0) return;

            foreach (var screw in screws)
            {
                if (!box.TryAddScrew(screw))
                {
                    _screwManager.AddHiddenScrews(new List<ScrewController> { screw });
                    break;
                }
            }
        }
    }
}