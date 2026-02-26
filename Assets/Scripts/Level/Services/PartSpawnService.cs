using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using PoolManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn tất cả BaseLayer và BasePart từ LevelData vào LayerManager.
/// Logic lấy từ GameObjectToLevelConverter.LoadGameObjectFromLevel(),
/// refactor sang service để dùng trong pipeline.
/// </summary>
public class PartSpawnService : IPartSpawnService
{
    private readonly IPartSpriteService _spriteService;

    public PartSpawnService(IPartSpriteService spriteService)
    {
        _spriteService = spriteService;
    }

    public IEnumerator SpawnLayers(Level.Level levelData, LayerManager layerManager)
    {
        if (levelData == null || layerManager == null)
        {
            Debug.LogError("[PartSpawnService] levelData or layerManager is null.");
            yield break;
        }

        layerManager.screwDict = new Dictionary<int, List<ScrewController>>();

        var listBaseLayer = new List<BaseLayer>();

        int idLayer = 0;
        foreach (var layerData in levelData.layers)
        {
            // Init screw dict slot
            layerManager.screwDict.Add(layerData.layerId, new List<ScrewController>());

            // Spawn layer từ pool
            var layerObject = LayerPool.Instance.pool.SpawnNonGravity();
            var layerName = $"Layer {layerData.layerId + 1}";
            layerObject.gameObject.name = layerName;
            layerObject.transform.SetParent(layerManager.transform);
            layerObject.gameObject.layer = LayerMask.NameToLayer(layerName);

            var layerComponent = layerObject.GetComponent<BaseLayer>();

            // Spawn parts
            foreach (var partData in layerData.parts)
            {
                var partObject = PartPool.Instance.pool.SpawnNonGravity();
                if (partObject == null) continue;

                partObject.transform.SetParent(layerObject.transform);
                partObject.transform.SetPositionAndRotation(partData.partPosition, partData.partRotation);
                partObject.transform.localScale = partData.partLocalScale;

                var partComponent = partObject.GetComponent<BasePart>();
                partComponent.uniqueID = partData.partName;
                partObject.gameObject.name = partData.partName;

                // Sprite
                var sprite = _spriteService.GetPartSprite(
                    levelData.levelId,
                    partData.spriteName,
                    partData.layer,
                    outline: false
                );
                partComponent.Renderer.sprite = sprite;

                // Color
                if (ColorUtility.TryParseHtmlString("#" + partData.colorString, out Color color))
                    partComponent.Renderer.color = color;

                // Register
                layerManager.AddPart(partComponent);
                partComponent.ResetAndReapplyPolygonCollider();

                var layerLayerName = LayerMask.LayerToName(layerObject.gameObject.layer);
                partComponent.SetSortingLayer(layerLayerName);
                partComponent.gameObject.layer = layerObject.gameObject.layer;

                yield return null; // Không block frame
            }

            listBaseLayer.Add(layerComponent);
            idLayer++;
        }

        layerManager.Layers = listBaseLayer;
        layerManager.CoverDictToList();
    }
}