using System;

public class LockStateHandler
{
    public bool IsLocked { get; private set; }

    private readonly Action _onLock;
    private readonly Action _onUnlock;

    public LockStateHandler(Action onLock, Action onUnlock)
    {
        _onLock = onLock;
        _onUnlock = onUnlock;
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;

        if (locked)
            _onLock?.Invoke();
        else
            _onUnlock?.Invoke();
    }
}