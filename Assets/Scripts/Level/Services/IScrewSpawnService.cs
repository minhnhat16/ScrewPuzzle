using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using System.Collections;
using UnityEngine;

/// <summary>
/// Contract cho service spawn Screw vào scene và gắn hinge.
/// Implementation cụ thể: ScrewSpawnService.
/// </summary>
public interface IScrewSpawnService
{
    /// <summary>
    /// Spawn tất cả screw từ LevelData, gắn hinge vào part tương ứng,
    /// và đăng ký vào ScrewManager + LayerManager.screwDict.
    /// </summary>
    IEnumerator SpawnScrews(
        Level.Level levelData,
        LayerManager layerManager,
        ScrewManager screwManager,
        Transform screwParent);
}