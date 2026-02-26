using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StateMachine
{
    /// <summary>
    /// Bộ máy quản lý state của gameplay.
    /// - Chỉ có 1 state active tại một thời điểm
    /// - Validate transition hợp lệ trước khi chuyển
    /// - Fire event khi state thay đổi để các system khác lắng nghe
    /// </summary>
    public class GameStateMachine
    {
        // ─────────────────────────────────────────
        // State hiện tại
        // ─────────────────────────────────────────
        private GameplayState _currentStateType = GameplayState.Idle;
        private IGameState _currentState;

        public GameplayState Current => _currentStateType;

        // ─────────────────────────────────────────
        // Registry: map enum → IGameState instance
        // ─────────────────────────────────────────
        private readonly Dictionary<GameplayState, IGameState> _states
            = new Dictionary<GameplayState, IGameState>();

        // ─────────────────────────────────────────
        // Transition whitelist: state nào được phép đi tới state nào
        // ─────────────────────────────────────────
        private static readonly Dictionary<GameplayState, HashSet<GameplayState>> _allowedTransitions
            = new Dictionary<GameplayState, HashSet<GameplayState>>
        {
            // Idle: chỉ đi vào Loading khi bắt đầu load level
            { GameplayState.Idle,         new HashSet<GameplayState> { GameplayState.Loading } },

            // Loading: xong thì sang Playing
            { GameplayState.Loading,      new HashSet<GameplayState> { GameplayState.Playing } },

            // Playing: các nhánh thoát ra
            //   Paused       → player bấm pause
            //   Win          → tất cả screw xong
            //   RevivePrompt → array screw full, box queue chưa full
            //   Lose         → array screw full VÀ box queue cũng full (không còn revive)
            //   ItemUsing    → player chọn dùng item
            { GameplayState.Playing,      new HashSet<GameplayState> { GameplayState.Paused, GameplayState.Win, GameplayState.Lose, GameplayState.RevivePrompt, GameplayState.ItemUsing } },

            // Paused: resume về Playing, hoặc thoát hẳn về Idle (Return Home)
            { GameplayState.Paused,       new HashSet<GameplayState> { GameplayState.Playing, GameplayState.Idle } },

            // Win: về Idle (chuẩn bị level tiếp) hoặc Load level mới
            { GameplayState.Win,          new HashSet<GameplayState> { GameplayState.Idle, GameplayState.Loading } },

            // Lose: về Idle (menu) hoặc Load lại level
            { GameplayState.Lose,         new HashSet<GameplayState> { GameplayState.Idle, GameplayState.Loading } },

            // RevivePrompt: player chọn Revive → Playing, không Revive → Lose
            { GameplayState.RevivePrompt, new HashSet<GameplayState> { GameplayState.Playing, GameplayState.Lose } },

            // ItemUsing: item xong hoặc cancel → về Playing
            { GameplayState.ItemUsing,    new HashSet<GameplayState> { GameplayState.Playing } },
        };

        // ─────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────

        /// <summary>
        /// Fire khi state thay đổi.
        /// Param: (previousState, newState)
        /// </summary>
        public event Action<GameplayState, GameplayState> OnStateChanged;

        // ─────────────────────────────────────────
        // Setup
        // ─────────────────────────────────────────

        /// <summary>
        /// Đăng ký một state vào machine.
        /// Gọi trước khi dùng TransitionTo.
        /// </summary>
        public void RegisterState(IGameState state)
        {
            if (state == null)
            {
                Debug.LogError("[GameStateMachine] Không thể register null state.");
                return;
            }

            if (_states.ContainsKey(state.StateType))
            {
                Debug.LogWarning($"[GameStateMachine] State {state.StateType} đã được register. Ghi đè.");
            }

            _states[state.StateType] = state;
        }

        /// <summary>
        /// Khởi động machine với state đầu tiên (không qua transition validation).
        /// </summary>
        public void Start(GameplayState initialState = GameplayState.Idle)
        {
            if (!_states.TryGetValue(initialState, out var state))
            {
                Debug.LogError($"[GameStateMachine] State {initialState} chưa được register.");
                return;
            }

            _currentStateType = initialState;
            _currentState = state;
            _currentState.OnEnter(GameplayState.Idle);

            Debug.Log($"[GameStateMachine] Khởi động với state: {initialState}");
        }

        // ─────────────────────────────────────────
        // Runtime
        // ─────────────────────────────────────────

        /// <summary>
        /// Chuyển sang state mới.
        /// Trả về false nếu transition không hợp lệ.
        /// </summary>
        public bool TransitionTo(GameplayState next)
        {
            // Không làm gì nếu đang ở state đó rồi
            if (_currentStateType == next)
            {
                Debug.LogWarning($"[GameStateMachine] Đã ở state {next}, bỏ qua transition.");
                return false;
            }

            // Validate
            if (!CanTransition(_currentStateType, next))
            {
                Debug.LogError($"[GameStateMachine] Transition không hợp lệ: {_currentStateType} → {next}");
                return false;
            }

            // State mới phải được register
            if (!_states.TryGetValue(next, out var nextState))
            {
                Debug.LogError($"[GameStateMachine] State {next} chưa được register.");
                return false;
            }

            var previous = _currentStateType;

            // Exit state cũ
            _currentState?.OnExit(next);

            // Chuyển
            _currentStateType = next;
            _currentState = nextState;

            // Enter state mới
            _currentState.OnEnter(previous);

            // Notify
            OnStateChanged?.Invoke(previous, next);

            Debug.Log($"[GameStateMachine] {previous} → {next}");
            return true;
        }

        /// <summary>
        /// Gọi mỗi frame từ MonoBehaviour.
        /// </summary>
        public void Tick(float deltaTime)
        {
            _currentState?.OnTick(deltaTime);
        }

        // ─────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────

        public bool CanTransition(GameplayState from, GameplayState to)
        {
            return _allowedTransitions.TryGetValue(from, out var allowed)
                   && allowed.Contains(to);
        }

        public bool IsIn(GameplayState state) => _currentStateType == state;

        public bool IsPlaying() => _currentStateType == GameplayState.Playing;
    }
}