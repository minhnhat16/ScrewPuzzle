
using System.Collections.Generic;
using UnityEngine;

public interface IBoxReward
{
    /// <summary>
    /// Spawn star tại các vị trí chỉ định (mỗi holdSlot 1 star) rồi fly đến anchor.
    /// </summary>
    void SpawnReward(List<UnityEngine.Vector3> spawnPositions);
}