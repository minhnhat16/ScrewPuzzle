using UnityEngine;

public class FSMSystem : MonoBehaviour
{
    public string state;

    public IFSMState currentState;
    public void GotoState(IFSMState newState)
    {
        this.state = newState.ToString();
        if (currentState != null)
            currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
    public void GotoState(IFSMState newState, object data)
    {
        this.state = newState.ToString();
        if (currentState != null)
            currentState.OnExit();
        currentState = newState;
        currentState.OnEnter(data);
    }

    private void Update()
    {
        if (currentState != null)
            currentState.OnUpdate();
        OnSystemUpdate();
    }
    private void LateUpdate()
    {
        currentState?.OnLateUpdate();
        OnSystemLateUpdate();
    }
    private void FixedUpdate()
    {
        currentState?.OnFixedUpdate();
        OnSystemFixedUpdate();
    }
    /// <summary>
    /// option Unity mess Update for child
    /// </summary>
    public virtual void OnSystemUpdate() { }
    /// <summary>
    /// option Unity mess LateUpdate for child
    /// </summary>
    public virtual void OnSystemLateUpdate() { }
    /// <summary>
    /// option Unity mess FixedUpdate for child
    /// </summary>
    public virtual void OnSystemFixedUpdate() { }

    public void OnAnimEnter()
    {
        currentState?.OnAnimEnter();
    }

    public void OnAnimExit()
    {
        currentState?.OnAnimExit();
    }

    public void OnAnimUpdate()
    {
        currentState?.OnAnimUpdate();
    }

}




