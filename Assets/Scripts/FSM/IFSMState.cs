public interface IFSMState
{
    public void OnEnter();
    public void OnEnter(object data);
    public void OnUpdate();
    public void OnLateUpdate();
    public void OnFixedUpdate();
    public void OnExit();

    public void OnAnimEnter();
    public void OnAnimUpdate();
    public void OnAnimExit();

}