using Gameplay.StateMachine;
using UnityEngine;

public class PlayingState : BaseGameState
{
    public override GameplayState StateType => GameplayState.Playing;

    private float _elapsedTime;
    public float ElapsedTime => _elapsedTime;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        Time.timeScale = 1f;

        // Chỉ reset timer khi bắt đầu level mới
        // Không reset khi resume từ Pause / ItemUsing / RevivePrompt
        bool isResume = previousState == GameplayState.Paused
                     || previousState == GameplayState.ItemUsing
                     || previousState == GameplayState.RevivePrompt;

        if (!isResume)
            _elapsedTime = 0f;
    }

    public override void OnTick(float deltaTime)
    {
        _elapsedTime += deltaTime;
    }
}