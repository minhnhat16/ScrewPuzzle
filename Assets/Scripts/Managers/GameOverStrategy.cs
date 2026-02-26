using Managers;

public class GameOverStrategy : IHoldFullStrategy
{
    public void OnHoldFull() => IngameController.ins.GameEndInvoker();
}