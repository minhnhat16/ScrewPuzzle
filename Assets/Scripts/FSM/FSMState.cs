
using UnityEngine;

public class FSMState<T> : IFSMState where T : FSMSystem
{
    protected T sys;

    public virtual void Setup(T sys)
    {
        this.sys = sys;
    }

    public virtual void OnEnter() { }
    public virtual void OnEnter(object data) { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnExit() { }

    public virtual void OnAnimEnter() { }

    public virtual void OnAnimUpdate() {}

    public virtual void OnAnimExit() { }
    public virtual void OnCollisionStay2D(Collision2D collision) { }

}
