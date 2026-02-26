using UnityEngine;

namespace Gameplay.StateMachine
{
    /// <summary>
    /// Base class ti?n l?i cho các state.
    /// Implement s?n các method r?ng ?? subclass ch? override cái c?n.
    /// </summary>
    public abstract class BaseGameState : IGameState
    {
        public abstract GameplayState StateType { get; }

        public virtual void OnEnter(GameplayState previousState)
        {
            Debug.Log($"[State] Enter: {StateType} (from {previousState})");
        }

        public virtual void OnTick(float deltaTime) { }

        public virtual void OnExit(GameplayState nextState)
        {
            Debug.Log($"[State] Exit: {StateType} (to {nextState})");
        }
    }
}