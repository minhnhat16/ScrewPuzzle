public interface IGameFlowService
{
    void CompleteLevel();
    void HandleGameOver();
    void HandleGameWin();
    void HandleRevive();
    void HandleReturnToMenu();
    void RestartLevel(int levelId);
}