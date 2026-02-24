public interface ILockable
{
    bool IsLocked { get; }
    void SetLocked(bool locked);
}