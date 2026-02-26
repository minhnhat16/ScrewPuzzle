using UnityEngine;

public enum BoxState
{
    Idle,
    Ready,
    Moving,
    Full,
    Removed
}

public class BoxStateController : MonoBehaviour
{
    public BoxState CurrentState { get; private set; }

    public bool IsReady => CurrentState == BoxState.Ready;

    public void SetState(BoxState state)
    {
        CurrentState = state;
    }
}