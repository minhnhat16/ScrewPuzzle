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
        public void RemoveScrewOnDict(ScrewController screw, int sortingOrder)
        {

            Debug.Log($"Attempting to remove screw {screw.GetColor()} from layer {sortingOrder} and screw layer name {screw.GetSortingLayerName()} ");
            if (!screwDict.TryGetValue(sortingOrder, out var listScrews) || listScrews == null)
                return;

            listScrews.Remove(screw);
            Debug.Log($"Removed screw {screw.GetColor()} from layer {sortingOrder}");
            if (listScrews.Count == 0)
                screwDict.Remove(sortingOrder);
        }

        // ─── Layer Control ──────────────────────────────────────────

        public void OnLayerCleared(BaseLayer clearedLayer)
        {
            if (!gameObject.activeSelf) return;
            StartCoroutine(ShowNext(clearedLayer));
        }

        public IEnumerator ShowNext(BaseLayer layer)
        {
            yield return new WaitForSeconds(1.5f);

            // Dùng PopLayer thay vì SetActive + ShowNextLayer riêng lẻ
            // — PopLayer sẽ null slot trong indexedLayers, advance window, rồi ApplyVisibility
            if (visibilityController != null)
                visibilityController.PopLayer(layer);
            else
            {
                // Fallback nếu không có visibilityController
                layer.gameObject.SetActive(false);
            }
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
            if (partDict.ContainsKey(part.uniqueID)) return;
            partDict.TryAdd(part.uniqueID, part);
        }

        public void CoverDictToList()
        {
            foreach (var part in partDict)
                parts.Add(part.Value);
        }

        public BasePart GetPartByKey(string uniqueId)
            => partDict.TryGetValue(uniqueId, out BasePart part) ? part : null;

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
                    if (s.hingeController.BodyConnect == body)
                        result.Add(s);
                }
            }
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
            ResetAllParts();
            ResetAllLayer();
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