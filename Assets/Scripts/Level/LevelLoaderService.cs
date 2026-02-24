using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoaderService : MonoBehaviour, ILevelLoader
{
    public LevelManager levelManager;

    public IEnumerator LoadLevelRoutine(int levelId, Action onLoaded = null)
    {
        if (levelManager == null)
        {
            Debug.LogError("[LevelLoaderService] levelManager is null");
            onLoaded?.Invoke();
            yield break;
        }

        bool done = false;
        // Use the public LoadLevel API which starts the internal coroutine and calls back when finished.

        Debug.Log($"[LevelLoaderService] Starting load of level {levelId}");
        levelManager.LoadLevel(levelId, () => done = true);

        // Wait until LevelManager signals completion.
        yield return new WaitUntil(() => done);

        onLoaded?.Invoke();
    }

    public Level.Level GetLevelData(int levelId)
    {
        return levelManager.GetLevelData(levelId);
    }
}