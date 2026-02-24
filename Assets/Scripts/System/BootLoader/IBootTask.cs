using System.Collections;

public interface IBootTask
{
    // Name for logging / diagnostics
    string Name { get; }

    // Execute the task (IEnumerator for Unity coroutines)
    IEnumerator Execute();
}