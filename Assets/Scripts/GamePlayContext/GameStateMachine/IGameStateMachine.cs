using System;

namespace Gameplay.StateMachine
{
    /// <summary>
    /// Interface mà các system khác dùng để tương tác với StateMachine.
    /// Tránh coupling trực tiếp vào GameStateMachineBootstrapper.
    /// </summary>
    public interface IGameStateMachine
    {
        GameplayState Current { get; }

        /// <summary>Fire khi state thay đổi. (previousState, newState)</summary>
        event Action<GameplayState, GameplayState> OnStateChanged;

        bool TransitionTo(GameplayState next);

        bool IsPlaying();
        bool IsIn(GameplayState state);
        bool CanTransition(GameplayState from, GameplayState to);
    }
}