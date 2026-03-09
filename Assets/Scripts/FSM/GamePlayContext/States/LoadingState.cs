using Gameplay.StateMachine;
using UnityEngine;

public class LoadingState : BaseGameState
{
    public override GameplayState StateType => GameplayState.Loading;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        Time.timeScale = 1f;
    }
}