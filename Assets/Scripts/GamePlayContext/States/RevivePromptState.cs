using Gameplay.StateMachine;
using UnityEngine;

public class RevivePromptState : BaseGameState
{
    public override GameplayState StateType => GameplayState.RevivePrompt;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        // Không freeze — dialog revive có countdown animation
        Time.timeScale = 1f;
    }
}