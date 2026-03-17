using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using PoolManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn tất cả ScrewController từ LevelData sử dụng ScrewPool,
/// gắn HingeJoint2D vào part, và đăng ký vào ScrewManager + LayerManager.screwDict.
/// </summary>
public class ScrewSpawnService : IScrewSpawnService
{
    public IEnumerator SpawnScrews(
        Level.Level levelData,
        LayerManager layerManager,
        ScrewManager screwManager,
        Transform screwParent)
    {
        if (levelData == null || levelData.screws == null || levelData.screws.Count == 0)
        {
            Debug.LogWarning("[ScrewSpawnService] No screws in level data.");
            yield break;
        }

        if (ScrewPool.Instance.Pool == null)
        {
            Debug.LogError("[ScrewSpawnService] ScrewPool.Instance or Pool is null.");
            yield break;
        }

        // Reset registry trước khi spawn level mới
        // tránh stale key từ level trước
        TutorialTargetRegistry.Clear();

        foreach (var screwData in levelData.screws)
        {
            if (screwData == null) continue;

            var screw = SpawnScrewFromPool(screwData, screwParent);
            if (screw == null) continue;

            var part = layerManager.GetPartByKey(screwData.hinge.bodyPartUniqueID);
            if (!ValidateAndSetupScrew(screw, screwData, part))
            {
                ScrewPool.Instance.Pool.ReturnToPool(screw);
                continue;
            }
            RegisterScrewInLayer(screw, part, layerManager);

            if (!string.IsNullOrEmpty(screwData.tutorialKey))
            {
                TutorialTargetRegistry.Register(screwData.tutorialKey, screw.transform);
                screw.tutorialKey = screwData.tutorialKey;
            }

            yield return screw.Init();
            screwManager.AddScrew(screw);
        }

    }

    private ScrewController SpawnScrewFromPool(ScrewScriptable screwData, Transform parent)
    {
        var screw = ScrewPool.Instance.Pool.SpawnNonGravity();
        screw.OnReset();
        if (screw == null)
        {
            Debug.LogError("[ScrewSpawnService] Failed to spawn screw from pool.");
            return null;
        }

        screw.gameObject.transform.SetParent(parent);
        screw.gameObject.transform.localPosition = screwData.screwPosition;

        return screw;
    }

    private bool ValidateAndSetupScrew(ScrewController screw, ScrewScriptable screwData, BasePart part)
    {
        if (part == null)
        {
            Debug.LogWarning($"[ScrewSpawnService] Part not found: {screwData.hinge.bodyPartUniqueID}");
            return false;
        }

        var color = (ColorEnum)screwData.idColor;
        screw.ChangeScrewColor(color);

        var rigidbody = part.GetComponent<Rigidbody2D>();

        // Hinge connect trước
        screw.CreateHinge(rigidbody, screwData.hinge);

        // Unfreeze sau khi hinge đã connect — part giờ được giữ bởi hinge
        //part.UnfreezeBody();

        return true;
    }

    private void RegisterScrewInLayer(ScrewController screw, BasePart part, LayerManager layerManager)
    {
        int layerID = part.PartLayer() - 10;

        if (!layerManager.screwDict.ContainsKey(layerID))
            layerManager.screwDict[layerID] = new List<ScrewController>();

        layerManager.screwDict[layerID].Add(screw);

        Debug.Log($"[ScrewSpawnService] Registered screw to layer {layerID}");
    }
}