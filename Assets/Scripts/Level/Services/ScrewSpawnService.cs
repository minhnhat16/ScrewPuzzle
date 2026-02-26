using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn tất cả ScrewController từ LevelData, gắn HingeJoint2D vào part,
/// và đăng ký vào ScrewManager + LayerManager.screwDict.
///
/// Logic lấy từ GameObjectToLevelConverter (phần screw), refactor sang service.
/// </summary>
public class ScrewSpawnService : IScrewSpawnService
{
    private const string SCREW_PREFAB_PATH = "Prefabs/ScrewController";

    public IEnumerator SpawnScrews(
        Level.Level levelData,
        LayerManager layerManager,
        ScrewManager screwManager,
        Transform screwParent)
    {
        if (levelData?.screws == null || levelData.screws.Count == 0)
        {
            Debug.LogWarning("[ScrewSpawnService] No screws in level data.");
            yield break;
        }

        var screwPrefab = Resources.Load<GameObject>(SCREW_PREFAB_PATH);
        if (screwPrefab == null)
        {
            Debug.LogError($"[ScrewSpawnService] Prefab not found at Resources/{SCREW_PREFAB_PATH}");
            yield break;
        }

        foreach (var screwData in levelData.screws)
        {
            if (screwData == null) continue;

            // Spawn screw
            var screwGO = Object.Instantiate(screwPrefab, screwParent);
            screwGO.transform.localPosition = screwData.screwPosition;

            var screwComponent = screwGO.GetComponent<ScrewController>();
            if (screwComponent == null)
            {
                Debug.LogError("[ScrewSpawnService] ScrewController not found on prefab.");
                Object.Destroy(screwGO);
                continue;
            }

            // Set color
            var color = (ColorEnum)screwData.idColor;
            screwComponent.ChangeScrewColor(color);

            // Create hinge
            var hingeConnection = screwData.hinge;
            var connectedPart = layerManager.GetPartByKey(hingeConnection.bodyPartUniqueID);

            if (connectedPart == null)
            {
                Debug.LogWarning($"[ScrewSpawnService] Part not found: {hingeConnection.bodyPartUniqueID}");
                continue;
            }

            screwComponent.CreateHinge(connectedPart.GetComponent<Rigidbody2D>(), hingeConnection);

            // Register in screwDict
            int partLayerID = connectedPart.PartLayer() - 10;
            if (!layerManager.screwDict.ContainsKey(partLayerID))
                layerManager.screwDict[partLayerID] = new List<ScrewController>();

            if (!layerManager.screwDict[partLayerID].Contains(screwComponent))
                layerManager.screwDict[partLayerID].Add(screwComponent);

            // Register in ScrewManager
            screwManager.AddScrew(screwComponent);

            yield return null;

            // Init physics / visuals sau khi đã được add vào scene
            screwGO.GetComponent<MonoBehaviour>()?.StartCoroutine(screwComponent.Init());
        }
    }
}