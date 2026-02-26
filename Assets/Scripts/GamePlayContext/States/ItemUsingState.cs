using Gameplay.StateMachine;
using UnityEngine;

public class ItemUsingState : BaseGameState
{
    public override GameplayState StateType => GameplayState.ItemUsing;

    public override void OnEnter(GameplayState previousState)
    {
        base.OnEnter(previousState);
        Time.timeScale = 1f;
    }

    public override void OnExit(GameplayState nextState)
    {
        base.OnExit(nextState);
        // ItemController sẽ tự dọn highlight/preview
        // khi lắng nghe OnStateChanged
    }
}