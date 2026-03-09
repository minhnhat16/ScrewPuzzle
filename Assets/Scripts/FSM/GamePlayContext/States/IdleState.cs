using Gameplay.StateMachine;
using UnityEngine;

public class IdleState : BaseGameState
{
    public override GameplayState StateType => GameplayState.Idle;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        //Time.timeScale = 1f;
    }
}