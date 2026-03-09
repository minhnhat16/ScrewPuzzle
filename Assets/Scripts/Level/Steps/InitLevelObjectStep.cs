using Ingame;
using Ingame.Board;
using LevelSystem.Core;
using PoolManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 1: Spawn LevelObject từ pool.
    /// Nếu LevelObject đang chứa layer/part từ level cũ → return về pool đúng thứ tự:
    ///   Screw → Hinge → Part → Layer (từ trong ra ngoài)
    /// KHÔNG Destroy — tránh mất object khỏi pool.
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

            // Return children về pool đúng thứ tự trước khi dùng lại
            ReturnChildrenToPool(levelObject.gameObject, layerManager);

            // Clear state của LayerManager
            layerManager.ClearPartDict();
            layerManager.Reset();

            ctx.LevelObject = levelObject;
            ctx.LayerManager = layerManager;

            // Chờ Destroy (nếu có) và physics settle
            yield return null;
        }

        // ─── Pool return helpers ────────────────────────────────────

        private void ReturnChildrenToPool(GameObject levelRoot, LayerManager layerManager)
        {
            // Thu thập tất cả layer trước khi iterate (tránh modify collection trong loop)
            var layers = new List<BaseLayer>();
            foreach (Transform child in levelRoot.transform)
            {
                if (child.TryGetComponent<BaseLayer>(out var layer)) 
                    layers.Add(layer);
            }

            foreach (var layer in layers)
            {
                ReturnLayerToPool(layer, layerManager);
            }
        }

        private void ReturnLayerToPool(BaseLayer layer, LayerManager layerManager)
        {
            // Thu thập parts trong layer này
            var parts = new List<BasePart>();
            foreach (Transform child in layer.transform)
            {
                if (child.TryGetComponent<BasePart>(out var part)) 
                    parts.Add(part);
            }

            foreach (var part in parts)
            {
                ReturnPartToPool(part);
            }

            // Return layer về pool sau khi đã clear screws và parts
            layer.transform.SetParent(LayerPool.Instance.pool.parent);
            LayerPool.Instance.pool.ReturnToPool(layer.GetComponent<BaseLayer>()
                .GetComponent<BaseLayer>()); // delegate tới pool
            LayerPool.Instance.pool.ReturnToPool(layer.gameObject.GetComponent<BaseLayer>());
        }

        private void ReturnPartToPool(BasePart part)
        {
            // 1. Return tất cả Hinge children về HingePool
            var hinges = new List<HingeObject>();
            foreach (Transform child in part.transform)
            {
                if (child.TryGetComponent<HingeObject>(out var hinge)) hinges.Add(hinge);
            }
            foreach (var hinge in hinges)
            {
                hinge.transform.SetParent(HingePool.Instance.pool.parent);
                HingePool.Instance.pool.ReturnToPool(hinge);
            }

            // 2. Reset body state trước khi return
            part.Reset();
            // 3. Return part về PartPool
            part.transform.SetParent(PartPool.Instance.pool.parent);
            PartPool.Instance.pool.ReturnToPool(part.GetComponent<BasePart>());
        }
    }
}