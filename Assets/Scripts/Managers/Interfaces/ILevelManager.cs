public interface ILevelManager
{
    int CurrentLevelId { get; }
    void LoadLevel(int levelId);
    void ResetLevel();
}