using Gameplay.StateMachine;
using UnityEngine;

public class PausedState : BaseGameState
{
    public override GameplayState StateType => GameplayState.Paused;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        Time.timeScale = 0f;
    }

    public override void OnExit(GameplayState nextState)
    {
        base.OnExit(nextState);
        Time.timeScale = 1f;
    }
}