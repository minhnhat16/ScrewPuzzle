using Core.Match;
using Enums;
using Gameplay.StateMachine;
using Ingame;
using Ingame.Screw;
using System;
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

        /// <summary>State hiện tại của game — dùng để ghi nhớ trước khi pause.</summary>
        public GameplayState CurrentState => _stateMachine.Current;

        /// <summary>
        /// True khi đang ở ItemUsing state VÀ Breaker đang selected (chờ player tap part).
        /// Dùng để BasePart.OnTap() check mà không cần inject IInteractionService.
        /// </summary>
        public bool IsItemExecutingBreaker =>
            _stateMachine.IsIn(GameplayState.ItemUsing) &&
            _itemController.IsItemSelected &&
            _itemController.CurrentState is RemovePartState;

        /// <summary>Expose ItemController để BasePart.OnTap() forward tap.</summary>
        public ItemController ItemController => _itemController;

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
                player: player,
                stateMachine: stateMachineBoot
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
            // Decide locks per-state:
            // - Playing: allow screw input, block item input
            // - ItemUsing: allow item input, block screw input
            // - Others: block both
            bool lockScrew = true;
            bool lockItem = true;

            if (next == GameplayState.Playing)
            {
                lockScrew = false;
                lockItem = true;
            }
            else if (next == GameplayState.ItemUsing)
            {
                lockScrew = true;
                lockItem = false;
            }

            player.IsScrewInputLocked = lockScrew;
            player.IsItemInputLocked = lockItem;

            // Ensure physical colliders reflect input lock for screws — defensive: prevents any bypass.
            _screwManager?.SetAllScrewsInteractable(!player.IsScrewInputLocked);

            Debug.Log($"[IngameController] State changed from {prev} to {next}. " +
                      $"ScrewInputLocked: {lockScrew}, ItemInputLocked: {lockItem}");
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

            // Debug: log current state trước khi transition
            Debug.Log($"[IngameController] StartLevel — current state: {_stateMachine.Current}");

            if (!_stateMachine.TransitionTo(GameplayState.Loading))
            {
                //Debug.LogError($"[IngameController] Failed to transition to Loading. " +
                //               $"Current state: {_stateMachine.Current}, " +
                //               $"CanTransition: {_stateMachine.CanTransition(_stateMachine.Current, GameplayState.Loading)}");
                return;
            }

            if (!_stateMachine.TransitionTo(GameplayState.Playing))
            {
                Debug.LogError($"[IngameController] Failed to transition to Playing. " +
                               $"Current state: {_stateMachine.Current}");
                return;
            }

            Debug.Log("[IngameController] Level started - now in Playing state");
        }

        private void HandleLevelComplete(bool completed)
        {
            if (!completed) return;
            _stateMachine.TransitionTo(GameplayState.Win);
            MissionManager.ins.ProcessLevelComplete();
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
            _itemController?.ResetRuntimeState();
            _gameFlow.RestartLevel(levelId);
        }

        // ─────────────────────────────────────────
        // Pause / Resume / Revive / Return
        // ─────────────────────────────────────────

        public void Pause() => _stateMachine.TransitionTo(GameplayState.Paused);
        public void Resume() => _stateMachine.TransitionTo(GameplayState.Playing);

        /// <summary>
        /// Resume về state cụ thể thay vì luôn luôn về Playing.
        /// Dùng khi dialog close cần trả về đúng state trước khi pause.
        /// </summary>
        public void ResumeTo(GameplayState targetState)
        {
            if (targetState == GameplayState.Paused) return; // guard
            _stateMachine.TransitionTo(targetState);
        }

        public void DeclineRevive() => _stateMachine.TransitionTo(GameplayState.Lose);

        /// <summary>
        /// Revive từ ReviveDialog — show dialog, player chọn trong đó.
        /// </summary>
        public void Revive()
        {
            _gameFlow.HandleRevive(); // → ShowReviveDialog
        }

        /// <summary>
        /// Revive trực tiếp không qua dialog (từ LoseDialog Watch ads).
        /// Unlock input + unlock box + về Playing.
        /// </summary>
        public void ReviveDirectly()
        {
            // unlock both screw and item input when reviving directly
            player.IsScrewInputLocked = false;
            player.IsItemInputLocked = false;

            // Make sure screw colliders are enabled when unlocking
            _screwManager?.SetAllScrewsInteractable(true);

            _containerQueue.UnlockNext();
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
            Debug.Log("game state on invoke item: " + _stateMachine.Current);
            if (!_stateMachine.IsPlaying()) return;
            if (_itemController.IsItemExecuting) return;

            _stateMachine.TransitionTo(GameplayState.ItemUsing);

            IItem item = type switch
            {
                ItemType.Breaker => _itemController.RemovePartState,
                ItemType.AddBox => _itemController.AddBoxState,
                ItemType.Magnet => _itemController.MagnetState,
                ItemType.Drill => _itemController.AddOneHold,
                _ => null
            };

            if (item == null)
            {
                Debug.LogWarning($"[IngameController] Không tìm thấy item state cho {type}");
                _stateMachine.TransitionTo(GameplayState.Playing);
                return;
            }

            _itemController.GotoState((IFSMState)item);
            _itemController.SetSelected(true);
            item.Use(position);
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

            Debug.Log($"[IngameController] AddStar: currentStar = {currentStar}, totalStarInLevel = {totalStarInLevel}");
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
        /// <summary>
        /// Set tổng số star cần đạt trong level.
        /// totalStarInLevel = số box records + (1 nếu có side mission, 0 nếu không)
        /// Gọi từ InitSpecialMissionStep trong pipeline load level.
        /// </summary>
        public void SetTotalStar(int total)
        {
            totalStarInLevel = Mathf.Max(1, total);  // tối thiểu 1 để tránh div/0 trong AddStar
            Debug.Log($"[IngameController] totalStarInLevel = {totalStarInLevel}");
        }
    }
}
