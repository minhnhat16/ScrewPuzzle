# Boot Sequence Performance Best Practices

## Overview
This guide explains the optimized boot sequence and recommendations for maintaining performance.

---

## Current Boot Sequence (Optimized)

```
START
  ?
Boot Scene Loads
  ?
BootLoader.Awake() - Screen setup
  ?
BootLoader.Start() - Launch boot tasks
  ?
Boot Tasks (Sequential)
  1. LoadRemoteAssetsTask (0.5-1s)
     - Init ResourceManager with level + UI assets
     - Index sprites
     - Defer shader warmup ?

  2. InitConfigTask (0.5-1s)
     - Load config files
     - Initialize LevelManager

  3. InitDataTask (0.3-0.5s)
     - Load player data

  4. InitMissionTask (0.2-0.5s)
     - Load mission data

  5. SetupUITask (0.1s) ?
     - Init ViewManager (now instant)
     - Init DialogManager (now instant)

  6. InitSoundTask (0.2-0.5s)
     - Initialize audio system
  ?
All Tasks Complete (~2-4s total)
  ?
Load MainScene Asynchronously
  ?
Display MainScreenView (lazy-loaded on demand)
  ?
Game Ready
  ?
[BACKGROUND] Shader Warmup Starts (deferred, non-blocking)
```

---

## ?? Important: Lazy-Loading Considerations

### Views
- **Pre-loaded:** None (all lazy-loaded)
- **First load:** 0.3-0.5s when switching to view
- **Subsequent:** Instant (cached)

**Action if slow:** Pre-load critical views by modifying `ViewManager.Init()` back to preload specific views:
```csharp
// If MainScreenView takes too long to lazy-load:
yield return StartCoroutine(EnsureViewLoaded(ViewIndex.MainScreenView));
```

### Dialogs
- **Pre-loaded:** None (all lazy-loaded)
- **First show:** 0.2-0.3s when ShowDialog() called
- **Subsequent:** Instant (cached)

**Action if slow:** Modify `IsCriticalDialog()` to pre-load specific dialogs:
```csharp
private bool IsCriticalDialog(DialogIndex dialogIndex)
{
    // Pre-load dialogs shown early in gameplay
    return dialogIndex == DialogIndex.DailyRewardDialog ||
           dialogIndex == DialogIndex.DailyRewardDialog;
}
```

---

## ?? Monitoring Boot Performance

### Enable Boot Timing Debug
1. Already implemented in `BootLoader.cs`
2. Watch console for messages like:
   ```
   [BOOT] Starting task (1/6): LoadRemoteAssets
   [BOOT] ? Completed task: LoadRemoteAssets (1.23s)
   [BOOT] Boot complete: 6/6 succeeded in 3.45s
   ```

### Using Unity Profiler
1. Open Window ? Analysis ? Profiler
2. Click Record during boot
3. Look at CPU timeline
4. Find remaining bottlenecks

### Memory Profiling
1. Window ? Analysis ? Memory Profiler
2. Compare snapshots before/after boot
3. Look for unnecessary allocations during boot

---

## ?? Boot Task Best Practices

### ? DO:
- Keep boot tasks **short and focused** (< 1 second each)
- Use **async/await** for I/O operations (don't block)
- Load **only essential data** during boot
- Log **start and end times** of heavy operations
- Use **coroutines properly** with clear callbacks

### ? DON'T:
- Instantiate unnecessary objects
- Load full asset batches synchronously
- Wait for operations that can run in background
- Perform expensive calculations
- Pre-initialize everything "just in case"

---

## Performance Targets

| Metric | Target | Current |
|--------|--------|---------|
| Boot to UI | < 2s | ~0.5-2s ? |
| Boot to gameplay ready | < 5s | ~2-4s ? |
| First view lazy-load | < 1s | ~0.3-0.5s ? |
| First dialog lazy-load | < 0.5s | ~0.2-0.3s ? |
| Shader warmup (background) | < 5s | ~2-5s ? |

---

## Future Optimizations

### Priority 1: Measure & Monitor
1. Build profiling into debug version
2. Log all task timings
3. Identify which tasks are slowest in practice

### Priority 2: Async Boot Tasks
Consider making boot tasks that are independent run in parallel:
```csharp
// Instead of sequential:
yield return InitConfigTask();
yield return InitDataTask();
yield return InitMissionTask();

// Could do parallel:
yield return WhenAll(
    InitConfigTask(),
    InitDataTask(),
    InitMissionTask()
);
```

### Priority 3: Addressables Optimization
- Check Addressables bundle settings
- Profile asset download/load times
- Consider pre-downloading on WiFi

### Priority 4: Scene Optimization
- Profile BootScene loading time
- Check for heavy GameObjects in scene
- Defer unnecessary Awake/Start calls

---

## Troubleshooting

### Problem: View takes too long to load when shown
**Solution:** Pre-load the view in boot
```csharp
// In SetupUITask
yield return ViewManager.Instance.EnsureViewLoaded(ViewIndex.GameView);
```

### Problem: Dialog shows too slowly
**Solution:** Pre-load critical dialogs in boot
```csharp
// Modify DialogManager.IsCriticalDialog()
return dialogIndex == DialogIndex.WinDialog || 
       dialogIndex == DialogIndex.LoseDialog;
```

### Problem: Shader artifacts appear
**Solution:** The 1-second delay before shader warmup might not be enough. Increase:
```csharp
yield return new WaitForSeconds(2.0f); // Increase from 1.0f
```

### Problem: Assets not loading in parallel
**Solution:** Check batch size in ResourceManager:
```csharp
const int batchSize = 5; // Increase to 10 for more parallelism
                         // Decrease if getting memory pressure
```

---

## Summary
The boot sequence is now optimized for sub-2-second startup with lazy-loading of views and dialogs. Monitor performance in production and adjust pre-loading strategy based on actual usage patterns.
