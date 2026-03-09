namespace Gameplay.StateMachine
{
    /// <summary>
    /// Contract cho mỗi state trong GameStateMachine.
    /// Mỗi state tự quản lý logic enter/tick/exit của mình.
    /// </summary>
    public interface IGameState
    {
        /// <summary>Tên để debug/log</summary>
        GameplayState StateType { get; }

        /// <summary>Gọi khi vào state này</summary>
        void OnEnter(GameplayState previousState);

        /// <summary>Gọi mỗi frame khi state đang active</summary>
        void OnTick(float deltaTime);

        /// <summary>Gọi khi rời state này</summary>
        void OnExit(GameplayState nextState);
    }
}
