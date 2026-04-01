using Enums;
using Ingame.Screw;
using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ingame.Board
{
    public class LayerManager : MonoBehaviour, ILayerManager
    {
        [SerializeField] List<BaseLayer> layers = new List<BaseLayer>();
        [SerializeField] private Dictionary<string, BasePart> partDict = new();
        [SerializeField] List<BasePart> parts = new List<BasePart>();
        public Dictionary<int, List<ScrewController>> screwDict = new();
        public Dictionary<BaseLayer, List<ScrewController>> screwDictByLayer = new();

        public List<BasePart> Parts { get => parts; set => parts = value; }
        public List<BaseLayer> Layers { get => layers; set => layers = value; }

        [SerializeField] private Queue<BaseLayer> layerQueue = new Queue<BaseLayer>();
        public LayerVisibilityController visibilityController;

        private void Start()
        {
            GetChildrenInRange();
            foreach (var layer in layers)
                layerQueue.Enqueue(layer);
        }

        private void GetChildrenInRange()
        {
            layers.Clear();
            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = gameObject.transform.GetChild(i);
                var baseLayer = child.GetComponent<BaseLayer>();
                if (baseLayer != null)
                {
                    layers.Add(baseLayer);
                    LayerUtils.ApplyParentLayer(child.gameObject);
                }
            }
        }

        // ─── ILayerManager ─────────────────────────────────────────

        /// <summary>
        /// Thêm screw vào screwDicse sau khi spawn và register screw.
        /// </summary>
        public void AddScrewToDict(ScrewController screw, int sortingOrder)
        {
            if (screw == null) return;

            if (!screwDict.ContainsKey(sortingOrder))
                screwDict[sortingOrder] = new List<ScrewController>();

            if (!screwDict[sortingOrder].Contains(screw))
                screwDict[sortingOrder].Add(screw);
        }

        /// <summary>
        /// Xóa screw khỏi screwDict theo sortingOrder (layer index).
        /// Gọi từ ScrewInteractionService khi player tap screw.
        /// </summary>
        public void RemoveScrew1(ScrewController screw, int sortingOrder)
        {

            Debug.Log($"Attempting to remove screw {screw.GetColor()} from layer {sortingOrder} and screw layer name {screw.GetSortingLayerName()} ");
            if (!screwDict.TryGetValue(sortingOrder, out var listScrews) || listScrews == null)
                return;

            listScrews.Remove(screw);
            Debug.Log($"Removed screw {screw.GetColor()} from layer {sortingOrder}");
            if (listScrews.Count == 0)
                screwDict.Remove(sortingOrder);
        }

        public void RemoveScrew(ScrewController screw)
        {
            int? keyToRemove = null;

            foreach (var kvp in screwDict)
            {
                var list = kvp.Value;
                if (list == null) continue;

                if (list.Remove(screw))
                {
                    Debug.Log($"Removed screw {screw.GetColor()} from layer {kvp.Key}");

                    if (list.Count == 0)
                        keyToRemove = kvp.Key;

                    break;
                }
            }

            if (keyToRemove.HasValue)
                screwDict.Remove(keyToRemove.Value);
        }
        // ─── Layer Control ──────────────────────────────────────────

        public void OnLayerCleared(BaseLayer clearedLayer)
        {
            if (!gameObject.activeSelf) return;


            Debug.Log($"[LayerManager] OnLayerCleared called for layer: {clearedLayer?.name ?? "null"}");
            // ✅ Guard: tránh start coroutine với layer đã invalid
            if (clearedLayer == null || !clearedLayer.IsLayerClear) return;

            StartCoroutine(ShowNext(clearedLayer));
        }

        public IEnumerator ShowNext(BaseLayer layer)
        {
            yield return new WaitForSeconds(1.5f);

            // ✅ Re-validate sau khi wait — layer có thể đã bị reset/pool trong 1.5s
            if (layer == null || visibilityController == null) yield break;

            // ✅ Check layer vẫn còn trong indexedLayers (chưa bị clear từ Reset())
            if (!visibilityController.indexedLayers.Contains(layer))
            {
                Debug.LogWarning($"[LayerManager] ShowNext: layer {layer.name} no longer in indexedLayers, skipping.");
                yield break;
            }

            // ✅ Check gameObject vẫn còn valid (chưa bị pool destroy)
            if (layer.gameObject == null || !layer.IsLayerClear)
            {
                Debug.LogWarning($"[LayerManager] ShowNext: layer no longer valid, skipping.");
                yield break;
            }


            if (!layer.IsLayerClear) yield break;
            visibilityController.PopLayer(layer);
        }

        public IEnumerator ChangePartState(float timeout = 0.5f)
        {
            foreach (var part in parts)
                part.UpdateFallingState();

            float timer = 0f;
            while (timer < timeout)
            {
                if (parts.TrueForAll(p => !p.IsFalling))
                    yield break;
                timer += Time.deltaTime;
                yield return null;
            }

            Debug.LogWarning("⚠ Falling timeout – force stop");
        }

        // ─── Part Management ────────────────────────────────────────

        public void AddPart(BasePart part)
        {
            if (part == null) return;

            if (partDict.ContainsKey(part.uniqueID))
            {
                Debug.LogWarning($"[LayerManager] AddPart: duplicate uniqueID '{part.uniqueID}' " +
                                 $"— overriding stale entry. Old={partDict[part.uniqueID]?.name}, New={part.name}");
                partDict[part.uniqueID] = part; // override stale ref
                return;
            }

            // Register and log for debug when running repeated loads
            partDict.TryAdd(part.uniqueID, part);
            Debug.Log($"[LayerManager] AddPart: registered part '{part.uniqueID}' -> {part.name}");
        }

        public void CoverDictToList()
        {
            foreach (var part in partDict)
                parts.Add(part.Value);
        }

        public BasePart GetPartByKey(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId)) return null;

            // Fast path: dictionary
            if (partDict != null && partDict.TryGetValue(uniqueId, out BasePart part) && part != null)
                return part;

            // Fallback: maybe dict wasn't populated but parts list contains the object (pool / init-order)
            if (parts != null)
            {
                part = parts.FirstOrDefault(p => p != null && p.uniqueID == uniqueId);
                if (part != null)
                {
                    Debug.LogWarning($"[LayerManager] GetPartByKey: recovered part '{uniqueId}' from Parts list. Re-registering into partDict.");
                    try
                    {
                        AddPart(part);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[LayerManager] GetPartByKey: re-register failed: {ex.Message}");
                    }
                    return part;
                }
            }

            Debug.LogWarning($"[LayerManager] GetPartByKey: part '{uniqueId}' not found (dict and Parts list empty).");
            return null;
        }

        public void RemovePart(string uniqueId)
        {
            if (!partDict.ContainsKey(uniqueId)) return;
            partDict.Remove(uniqueId);
        }

        // ─── Layer Activation ───────────────────────────────────────

        public void ActiveLayer(int idLayer)
        {
            if (idLayer >= layers.Count) return;
            layers[idLayer].gameObject.SetActive(true);
        }

        public void ActivateSingleLayer(int idLayer)
        {
            idLayer--;
            if (idLayer >= layers.Count || idLayer < 0)
            {
                ActiveAllLayers();
                return;
            }
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].gameObject.SetActive(idLayer == i);
                LayerUtils.ActiveObjectInLayer(idLayer == i, i, this);
            }
        }

        public void ActiveAllLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].gameObject.SetActive(true);
                LayerUtils.ActiveObjectInLayer(true, i, this);
            }
        }

        // ─── Screw Dict Helpers ─────────────────────────────────────

        public void RemoveScrewsOnDict(List<ScrewController> screws)
        {
            if (screws == null) return;

            var group = screws
                .Where(s => s != null)
                .GroupBy(s => s.GetSortingOrder());

            foreach (var g in group)
            {
                int layer = g.Key;
                if (!screwDict.TryGetValue(layer, out var list)) continue;
                foreach (var screw in g)
                    list.Remove(screw);
                if (list.Count == 0)
                    screwDict.Remove(layer);
            }
        }

        public List<ScrewController> GetScrewByPart(BasePart part)
        {
            if (part == null)
            {
                Debug.LogWarning("GetScrewByPart called with null part");
                return new List<ScrewController>();
            }

            var body = part.Body;
            if (body == null) return new List<ScrewController>();

            // screwDict key = SortingOrder (không phải physics layer)
            // Scan tất cả bucket để tìm screw connect đúng body
            var result = new List<ScrewController>();
            foreach (var kvp in screwDict)
            {
                if (kvp.Value == null) continue;
                foreach (var s in kvp.Value)
                {
                    if (s == null) continue;
                    if (s.IsInHold) continue;
                    if (s.IsDetachedFromBoard) continue;
                    if (!s.isActiveAndEnabled) continue;
                    if (s.hingeController.BodyConnect == body)
                        result.Add(s);
                }
            }

            string reString = result.Count > 0
                ? string.Join(", ", result.Select(s => s.GetColor()))
                : "no screws";

            Debug.Log($"GetScrewByPart: found {result.Count} screws connected to part '{part.name}' (body: {body.name}): {reString}");
            return result;
        }
        public bool PartHasNoScrewConnected(BasePart part)
        {
            if (part == null) return true;

            var body = part.Body;
            if (body == null) return true;

            foreach (var kvp in screwDict)
            {
                if (kvp.Value == null) continue;
                if (kvp.Value.Any(s => s != null
                                    && !s.IsDetachedFromBoard
                                    && s.hingeController != null
                                    && s.hingeController.BodyConnect == body))
                    return false;
            }
            return true;
        }

        public HashSet<ColorEnum> GetUniqueScrewColorsByLayer(int layer)
        {
            if (!screwDict.TryGetValue(layer, out var screws) || screws == null)
                return new HashSet<ColorEnum>();
            return screws.Where(s => s != null).Select(s => s.GetColor()).ToHashSet();
        }

        public List<ColorEnum> GetScrewColorsByLayer(int layer)
        {
            if (layer < 0 || !screwDict.TryGetValue(layer, out var screws) || screws == null)
                return new List<ColorEnum>();
            return screws.Where(s => s != null).Select(s => s.GetColor()).ToList();
        }

        // ─── Reset ─────────────────────────────────────────────────

        public void ClearPartDict()
        {
            parts.Clear();
            partDict.Clear();
        }

        public void Reset()
        {
            // ✅ Cancel tất cả ShowNext coroutine đang chờ 1.5s
            // Tránh PopLayer() chạy sau Reset() với stale layer reference
            StopAllCoroutines();

            ResetAllParts();
            ResetAllLayer();
            ClearPartDict();
            partDict.Clear();
            parts.Clear();
            screwDict.Clear();
            screwDictByLayer.Clear();
            layerQueue.Clear();
            layers.Clear(); // ✅ thêm — layers list vẫn còn ref sau ResetAllLayer

            if (visibilityController != null)
            {
                visibilityController.indexedLayers.Clear();
                visibilityController.layerQueue.Clear();
            }
        }

        private void ResetAllLayer()
        {
            foreach (var layer in layers)
            {
                layer.Reset();
                LayerPool.Instance.pool.ReturnToPool(layer);
            }
        }

        private void ResetAllParts()
        {
            PartPool.Instance.ReturnAll(Parts);
        }

        // ─── Queries ───────────────────────────────────────────────

        internal int GetTopVisibleLayer()
        {
            if (screwDict == null || screwDict.Count == 0) return -1;

            int top = int.MaxValue;
            foreach (var kv in screwDict)
            {
                int layer = kv.Key;
                var list = kv.Value;
                if (list == null || list.Count == 0) continue;

                bool hasValid = list.Any(s => s != null && !s.IsInHold);
                if (!hasValid) continue;
                if (layer < top) top = layer;
            }

            return top == int.MaxValue ? -1 : top;
        }
    }
}
