using UnityEngine;

public class BoxLockController : MonoBehaviour
{
    public bool IsLocked { get; private set; }

    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;
}