using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using PoolManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrator: coordinates LayerSpawnService + PartSetupService
/// to spawn all layers and parts from LevelData into the scene.
///
/// Each sub-service has one clear responsibility:
///   LayerSpawnService  → spawn + setup layer GameObject from pool
///   PartSetupService   → apply data (sprite/color/collider) to a BasePart
///   PartSpawnService   → loop, spawn parts from pool, register to LayerManager
/// </summary>
public class PartSpawnService : IPartSpawnService
{
    private readonly LayerSpawnService _layerSpawner;
    private readonly PartSetupService _partSetup;

    public PartSpawnService(IPartSpriteService spriteService)
    {
        _layerSpawner = new LayerSpawnService();
        _partSetup = new PartSetupService(spriteService);
    }

    public IEnumerator SpawnLayers(Level.Level levelData, LayerManager layerManager)
    {
        if (levelData == null || layerManager == null)
        {
            Debug.LogError("[PartSpawnService] levelData or layerManager is null.");
            yield break;
        }

        // Fresh start
        layerManager.screwDict = new Dictionary<int, List<ScrewController>>();

        var spawnedLayers = new List<BaseLayer>();

        foreach (var layerData in levelData.layers)
        {
            // 1. Spawn layer from pool
            var layerComponent = _layerSpawner.SpawnLayer(layerData, layerManager.transform, layerManager);
            if (layerComponent == null) continue;

            // 2. Spawn and setup parts for this layer
            yield return SpawnPartsForLayer(layerData, layerComponent, layerManager, levelData.levelId);

            spawnedLayers.Add(layerComponent);
        }

        // Finalize layer manager
        layerManager.Layers = spawnedLayers;
        layerManager.CoverDictToList();

        Debug.Log($"[PartSpawnService] Done — {spawnedLayers.Count} layers, " +
                  $"{layerManager.Parts?.Count ?? 0} parts spawned.");
    }

    private IEnumerator SpawnPartsForLayer(
        LayerData layerData,
        BaseLayer layerComponent,
        LayerManager layerManager,
        int levelId)
    {
        var sortingLayerName = LayerMask.LayerToName(layerComponent.gameObject.layer);

        foreach (BodyPartScriptable partData in layerData.parts)
        {
            var partObject = PartPool.Instance.pool.SpawnNonGravity();
            if (partObject == null)
            {
                Debug.LogWarning($"[PartSpawnService] Pool returned null for part {partData.partName}.");
                continue;
            }

            partObject.transform.SetParent(layerComponent.transform);
            partObject.gameObject.layer = layerComponent.gameObject.layer;

            if (!partObject.TryGetComponent<BasePart>(out var partComponent))
            {
                Debug.LogError($"[PartSpawnService] BasePart missing on pool object {partObject.name}.");
                continue;
            }

            _partSetup.Setup(partComponent, partData, levelId, sortingLayerName);

            // ── Đăng ký vào CẢ HAI nơi ──────────────────────────────
            layerManager.AddPart(partComponent);   // LayerManager.parts — cho physics/query
            layerComponent.parts.Add(partComponent); // BaseLayer.parts  — cho listener + clear check

            yield return null;
        }

        Debug.Log($"[PartSpawnService] Layer '{layerComponent.name}' — spawned {layerComponent.parts.Count} parts.");
    }
}