using System;
using System.Collections;

public interface ILevelLoader
{
    // Start the level load coroutine (returns IEnumerator so callers can yield)
    IEnumerator LoadLevelRoutine(int levelId, Action onLoaded = null);

    // Convenience (sync) lookup
    Level.Level GetLevelData(int levelId);
}