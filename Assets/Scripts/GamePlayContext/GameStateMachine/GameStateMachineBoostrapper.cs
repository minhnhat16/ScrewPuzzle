using UnityEngine;

namespace Gameplay.StateMachine
{
    /// <summary>
    /// MonoBehaviour duy nhất liên quan đến StateMachine.
    /// Gắn vào cùng GameObject với IngameController.
    ///
    /// Nhiệm vụ:
    ///   1. Khởi tạo và đăng ký tất cả state
    ///   2. Tick state active mỗi frame
    ///   3. Expose IGameStateMachine cho IngameController
    ///
    /// Các system khác KHÔNG reference class này trực tiếp —
    /// chúng nhận IGameStateMachine qua SerializeField hoặc DI.
    /// </summary>
    public class GameStateMachineBootstrapper : MonoBehaviour, IGameStateMachine
    {
        // ─────────────────────────────────────────
        // IGameStateMachine implementation
        // ─────────────────────────────────────────

        public GameplayState Current => _machine.Current;

        public event System.Action<GameplayState, GameplayState> OnStateChanged
        {
            add => _machine.OnStateChanged += value;
            remove => _machine.OnStateChanged -= value;
        }

        public bool TransitionTo(GameplayState next) => _machine.TransitionTo(next);
        public bool IsPlaying() => _machine.IsPlaying();
        public bool IsIn(GameplayState state) => _machine.IsIn(state);
        public bool CanTransition(GameplayState from, GameplayState to) => _machine.CanTransition(from, to);

        // ─────────────────────────────────────────
        // Internal
        // ─────────────────────────────────────────

        private GameStateMachine _machine;

        private void Awake()
        {
            _machine = new GameStateMachine();

            _machine.RegisterState(new IdleState());
            _machine.RegisterState(new LoadingState());
            _machine.RegisterState(new PlayingState());
            _machine.RegisterState(new PausedState());
            _machine.RegisterState(new WinState());
            _machine.RegisterState(new LoseState());
            _machine.RegisterState(new RevivePromptState());
            _machine.RegisterState(new ItemUsingState());

            _machine.Start(GameplayState.Idle);
        }

        private void Update()
        {
            _machine.Tick(Time.deltaTime);
        }
    }
}