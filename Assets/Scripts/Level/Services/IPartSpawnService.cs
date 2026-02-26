using Ingame.Board;
using Level;
using System.Collections;
using UnityEngine;

/// <summary>
/// Contract cho service spawn Part (board pieces) vào scene.
/// Implementation cụ thể: PartSpawnService.
/// Tách interface để:
///  - Test dễ (mock)
///  - Swap cách spawn (pool, addressables...) không ảnh hưởng pipeline
/// </summary>
public interface IPartSpawnService
{
    /// <summary>
    /// Spawn tất cả layer + part từ LevelData vào parent transform.
    /// Trả về LayerManager đã được setup.
    /// </summary>
    IEnumerator SpawnLayers(Level.Level levelData, LayerManager layerManager);
}