using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single responsibility: spawn BaseLayer từ pool và setup transform/name/layer mask.
/// Không biết gì về Part, Sprite hay Screw.
/// </summary>
public class LayerSpawnService
{

    private static Vector3 position = new (0, -2, 0);
    /// <summary>
    /// Spawn một BaseLayer từ pool, set parent, name, Unity layer mask.
    /// Tạo slot trong screwDict cho layer này.
    /// </summary>
    public BaseLayer SpawnLayer(LayerData layerData, Transform parent, LayerManager layerManager)
    {
        if (layerData == null || parent == null || layerManager == null)
        {
            Debug.LogError("[LayerSpawnService] Null argument(s) in SpawnLayer.");
            return null;
        }

        var layerObject = LayerPool.Instance.pool.SpawnNonGravity();
        if (layerObject == null)
        {
            Debug.LogError($"[LayerSpawnService] Pool returned null for layer {layerData.layerId}.");
            return null;
        }

        // Name + parent + Unity layer
        var layerName = $"Layer {layerData.layerId + 1}";
        layerObject.gameObject.name = layerName;
        layerObject.transform.SetParent(parent);
        layerObject.transform.SetLocalPositionAndRotation(position, Quaternion.identity);   
        layerObject.gameObject.layer = LayerMask.NameToLayer(layerName);

        // Reserve screw dict slot so ScrewSpawnService can fill it later
        if (!layerManager.screwDict.ContainsKey(layerData.layerId))
            layerManager.screwDict[layerData.layerId] = new List<ScrewController>();

        return layerObject.GetComponent<BaseLayer>();
    }
}
