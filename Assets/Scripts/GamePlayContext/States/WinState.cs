using Gameplay.StateMachine;
using UnityEngine;

public class WinState : BaseGameState
{
    public override GameplayState StateType => GameplayState.Win;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        // Không freeze — animation ăn mừng cần chạy
        Time.timeScale = 1f;
    }
}