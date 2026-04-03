# Asset Delivery System Guide

## Overview

The Asset Delivery System manages loading of level configs, box configs, and sprites from Addressables (asset delivery). This enables:

- **Lazy Loading**: Load assets on-demand as needed
- **Pre-Loading**: Pre-load critical assets at startup
- **Caching**: Automatic caching to avoid reloading
- **Memory Management**: Clean unload of unused assets

## Architecture

### Core Components

1. **AssetDeliveryService** (`Assets\Scripts\System\AssetDelivery\AssetDeliveryService.cs`)
   - Central service for all asset loading
   - Handles Addressables communication
   - Manages cache and handles
   - Singleton pattern for easy access

2. **AssetDeliveryBootstrapper** (`Assets\Scripts\System\BootLoader\AssetDeliveryBootstrapper.cs`)
   - Initializes asset delivery at startup
   - Pre-loads critical assets
   - Configurable labels for pre-loading

3. **LoadLevelAssetsStep** (`Assets\Scripts\Level\Steps\LoadLevelAssetsStep.cs`)
   - Integrated into level load pipeline
   - Loads level-specific assets before spawning
   - Ensures assets are available before level starts

## Setup Instructions

### 1. Addressables Configuration

First, configure your assets in the Addressables Groups window:

#### Level Configs
- **Label**: `LevelConfigs`
- **Assets**: `Level_1`, `Level_2`, `Level_3`, etc.
- **Path Format**: `Assets/Addressables/LevelConfigs/Level_1.asset`

#### Box Configs  
- **Label**: `BoxConfigs`
- **Assets**: All box configuration ScriptableObjects
- **Path Format**: `Assets/Addressables/BoxConfigs/BoxConfig_*.asset`

#### Level Sprites
- **Label**: `LevelSprites`
- **Assets**: All sprite sheets and individual sprites
- **Path Format**: `Assets/Addressables/Sprites/Level_*/...`

#### Level Data (Optional)
- **Label**: `LevelData`
- **Assets**: Level prefabs or composite data objects

### 2. Scene Setup

Add the bootstrapper to your boot/initialization scene:

```
1. Create empty GameObject "AssetDeliveryBootstrapper"
2. Add "AssetDeliveryBootstrapper" component
3. Configure pre-load labels in inspector (or use defaults)
4. Mark as DontDestroyOnLoad (automatic)
```

## Usage Examples

### Pre-Load All Level Configs at Startup

```csharp
var service = AssetDeliveryService.Instance;
StartCoroutine(service.LoadByLabelAsync("LevelConfigs", () =>
{
    Debug.Log("All level configs loaded");
}));
```

### Load Specific Level Assets

```csharp
var service = AssetDeliveryService.Instance;

// Load level 5 assets
var level = await service.LoadLevelAsync(5);
var sprite = await service.LoadSpriteAsync("SpriteKey");
var boxConfig = await service.LoadBoxConfigAsync("BoxConfigName");
```

### Pre-Warm Multiple Levels

```csharp
var service = AssetDeliveryService.Instance;
int[] levelIds = { 1, 2, 3, 4, 5 };
await service.PreWarmLevelsAsync(levelIds);
```

### Get Cached Asset Without Loading

```csharp
var service = AssetDeliveryService.Instance;

// Check if asset is already loaded
if (service.IsCached("LevelSprites_1"))
{
    var sprite = service.GetCachedAsset<Sprite>("LevelSprites_1");
}
```

## Integration with Level Loading

The asset delivery is automatically integrated into the level load pipeline:

```csharp
// LevelManager.cs load pipeline:
var pipeline = new LevelLoadPipeline()
    .AddStep(new LoadLevelAssetsStep())      // ? Assets loaded first
    .AddStep(new LoadPsbStep())              // ? PSB sprites
    .AddStep(new InitLevelObjectStep(transform))
    .AddStep(new ResolveLevelDataStep(_repository.Levels))
    // ... rest of pipeline
```

This ensures:
1. Assets are loaded before any level initialization
2. Sprites and configs are available for level setup
3. No missing asset errors during gameplay

## Performance Optimization

### Pre-Load Strategy

Pre-load heavy assets at startup:
- All level configs (small, reused often)
- All box configs (small, reused often)
- Optional: First few level sprites

Leave sprites and large assets for lazy-loading.

### Batch Loading

The system loads up to 5 assets in parallel for efficiency:

```csharp
// Internal batch loading (automatic)
const int batchSize = 5;  // Configurable in code
```

### Memory Management

**Clear unused assets after level completion:**

```csharp
// Clear specific asset
AssetDeliveryService.Instance.UnloadAsset("SpriteKey");

// Clear all cached assets
AssetDeliveryService.Instance.ClearCache();
```

**Recommended cleanup points:**
- After level loses/completes (before next level)
- On app quit
- When switching scenes

## Troubleshooting

### Assets Not Loading

1. **Check Addressables are built**: Window > Asset Management > Addressables Groups > Build > Build
2. **Verify labels**: Ensure labels match constants in code
3. **Check asset keys**: Asset keys must match Addressables primary keys
4. **Enable debug logs**: 

```csharp
Debug.Log(AssetDeliveryService.Instance.GetCacheInfo());
```

### Memory Issues

If memory usage is high:
1. Clear cache more frequently
2. Reduce pre-load label count
3. Implement per-level sprite unloading
4. Profile with Profiler window

### Performance Issues

If level loading is slow:
1. Check if loading is blocking main thread (should be async)
2. Profile download times (network/storage)
3. Consider pre-warming critical levels at app start
4. Adjust batch size for parallel loading

## Best Practices

1. **Use Labels Effectively**
   - Group related assets with labels
   - Pre-load small, frequently-used assets
   - Lazy-load large, rarely-used assets

2. **Cache Management**
   - Only cache what you need
   - Clear between levels to save memory
   - Use `GetCachedAsset<T>()` to check before loading

3. **Error Handling**
   - Always check if async tasks completed successfully
   - Provide fallbacks for missing assets
   - Log warnings for missing assets

4. **Organization**
   - Keep Addressables folder structure clean
   - Use consistent naming conventions
   - Document custom labels in code

## Example: Complete Level Loading Flow

```csharp
public void LoadLevel(int levelId)
{
    StartCoroutine(LoadLevelCoroutine(levelId));
}

private IEnumerator LoadLevelCoroutine(int levelId)
{
    // Pre-load critical assets
    yield return AssetDeliveryService.Instance
        .LoadByLabelAsync("LevelConfigs");

    // Load level-specific assets
    var levelBundle = await AssetDeliveryService.Instance
        .LoadLevelBundleAsync(levelId);

    if (levelBundle.LevelData == null)
    {
        Debug.LogError("Failed to load level assets");
        yield break;
    }

    // Level is ready to play
    InitializeLevel(levelBundle.LevelData);
}

private void CleanupAfterLevel()
{
    // Clear level-specific sprites
    AssetDeliveryService.Instance.ClearCache();
}
```

## Advanced: Custom Asset Types

To support custom ScriptableObject types:

```csharp
// Create your custom type
public class CustomLevelData : ScriptableObject { }

// Load it
var custom = await AssetDeliveryService.Instance
    .LoadAssetAsync<CustomLevelData>("CustomAssetKey");
```

The system automatically handles type detection and caching for any Object-derived type.
