using Ingame.Board;
using Ingame.Pools;
using LevelSystem.Core;
using System.Collections;
using UnityEngine;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 1: Spawn và khởi tạo LevelObject từ pool.
    /// Chỉ chịu trách nhiệm 1 việc duy nhất: tạo container cho level.
    /// </summary>
    public class InitLevelObjectStep : ILevelLoadStep
    {
        public string StepName => "Init Level Object";

        private readonly Transform _parent;

        public InitLevelObjectStep(Transform parent)
        {
            _parent = parent;
        }

        public IEnumerator Execute(LevelContext ctx)
        {
            var levelObject = LevelObjectPool.Instance.pool.SpawnNonGravity();
            levelObject.transform.SetParent(_parent);
            levelObject.transform.localPosition = Vector3.zero;

            var layerManager = levelObject.GetComponent<LayerManager>();

            // Clear children từ pool object cũ
            foreach (Transform child in levelObject.transform)
            {
                Object.Destroy(child.gameObject);
                layerManager.ClearPartDict();
            }

            ctx.LevelObject = levelObject;
            ctx.LayerManager = layerManager;

            yield return new WaitForEndOfFrame();
        }
    }
}