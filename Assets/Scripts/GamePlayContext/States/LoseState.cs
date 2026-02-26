using Gameplay.StateMachine;
using UnityEngine;

public class LoseState : BaseGameState
{
    public override GameplayState StateType => GameplayState.Lose;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        Time.timeScale = 1f;
    }
}