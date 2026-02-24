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
    public class LayerManager : MonoBehaviour
    {
        [SerializeField] List<BaseLayer> layers = new List<BaseLayer>();
        [SerializeField] private Dictionary<string, BasePart> partDict = new();
        [SerializeField] List<BasePart> parts = new List<BasePart>();
        public Dictionary<int, List<ScrewController>> screwDict = new();                 // existing usage (by layer index)
        public Dictionary<BaseLayer, List<ScrewController>> screwDictByLayer = new();   // new keyed-by-object map
        public List<BasePart> Parts
        {
            get => parts;
            set => parts = value;
        }

        public List<BaseLayer> Layers
        {
            get => layers;
            set => layers = value;
        }

        [SerializeField] private Queue<BaseLayer> layerQueue = new Queue<BaseLayer>();
        public LayerVisibilityController visibilityController;

        private void Start()
        {
            GetChildrenInRange();
            // Thêm tất cả layer vào hàng đợi
            foreach (var layer in layers)
            {
                layerQueue.Enqueue(layer);
            }

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

        // Gọi khi một layer thực hiện hành động clear
        public void OnLayerCleared(BaseLayer clearedLayer)
        {
            if(!gameObject.activeSelf) return;
            StartCoroutine(ShowNext(clearedLayer));

            //visibilityController.ShowNextLayer();
        }
        public IEnumerator ShowNext(BaseLayer layer )
        {

            yield return new WaitForSeconds(1.5f);
            layer.gameObject.SetActive(false);
            visibilityController.ShowNextLayer();

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


        public void AddPart(BasePart part)
        {
            if (part == null) return;

            if (partDict.ContainsKey(part.uniqueID))
            {
                return; // Exit the method if part already exists
            }

            partDict.TryAdd(part.uniqueID, part);
        }


        public void CoverDictToList()
        {
            foreach (var part in partDict)
            {
                parts.Add(part.Value);

            }
        }
        public BasePart GetPartByKey(string uniqueId)
        {
            return partDict.TryGetValue(uniqueId, out BasePart part) ? part : null;
        }

        public void RemovePart(string uniqueId)
        {
            if (!partDict.ContainsKey(uniqueId)) return;
            partDict.Remove(uniqueId);
        }

        /// <summary>
        /// Activate a layer by its index
        /// </summary>
        /// <param name="idLayer"></param>
        public void ActiveLayer(int idLayer)
        {
            if (idLayer >= layers.Count) return;
            layers[idLayer].gameObject.SetActive(true);
        }
        /// <summary>
        /// Activeate only one layer by its index, deactivate others
        /// </summary>
        /// <param name="idLayer"></param>
        public void ActivateSingleLayer(int idLayer)
        {
            idLayer--;
            Debug.Log("Active layer " + idLayer);
            if (idLayer >= layers.Count || idLayer < 0)
            {
                ActiveAllLayers();
                return;
            }
            ;
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].gameObject.SetActive(idLayer == i);
                LayerUtils.ActiveObjectInLayer(idLayer == i, i, this);

            }
            int idScrewLayer = idLayer--;
        }
        public void ActiveAllLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].gameObject.SetActive(true);
                LayerUtils.ActiveObjectInLayer(true, i, this);
            }
        }
        public void RemoveScrewOnDict(ScrewController screw, int layer)
        {
            screwDict.TryGetValue(layer, out List<ScrewController> listScrews);
            //Debug.Log("Remove screw on layer " + layer + " total screws " + (listScrews != null ? listScrews.Count.ToString() : "null"));
            if (listScrews == null) return;
            listScrews.Remove(screw);
            screwDict[layer] = listScrews;
        }
        public void RemoveScrewsOnDict(List<ScrewController> screws)
        {
            if (screws == null) return;

            var group = screws
                .Where(s => s != null)
                .GroupBy(s => s.GetSortingOrder());

            foreach (var g in group)
            {
                int layer = g.Key;

                if (!screwDict.TryGetValue(layer, out var list))
                    continue;

                foreach (var screw in g)
                    list.Remove(screw);
                if (list.Count == 0)
                    screwDict.Remove(layer);
            }
        }

        public List<ScrewController> GetScrewByPart(BasePart part)
        {
            // Check null trước khi đụng vào part.name
            if (part == null)
            {
                Debug.LogWarning("GetScrewByPart called with null part");
                return new List<ScrewController>();
            }

            Debug.Log("GetScrewByPart → part: " + part.name);

            int layer = part.PartLayer() - 10;

            // Bảo vệ trường hợp layer âm hoặc vượt ngoài dict
            if (layer < 0 || !screwDict.TryGetValue(layer, out var screwsInLayer) || screwsInLayer == null || screwsInLayer.Count == 0)
            {
                Debug.Log($"No screws found in layer {layer}");
                return new List<ScrewController>();
            }

            // Tìm tất cả screw có body nối với part.Body
            var body = part.Body;  // cache để tối ưu + sạch code

            var result = screwsInLayer
                .Where(s =>
                    s.hingeController?.BodyConnect != null &&
                    body != null &&
                    s.hingeController.BodyConnect == body)
                .ToList();

            return result;
        }

        public bool PartHasNoScrewConnected(BasePart part)
        {
            if (part == null) return true;

            int layer = part.PartLayer() - 10;

            if (!screwDict.TryGetValue(layer, out var screws) || screws == null)
                return true;  // Không có list → coi như không có screw kết nối

            var body = part.Body;

            // Kiểm tra có screw nào đang kết nối vào part không
            bool hasConnection = screws.Any(s =>
                s != null &&
                s.hingeController != null &&
                s.hingeController.BodyConnect != null &&
                s.hingeController.BodyConnect == body);

            return !hasConnection; // return true nếu KHÔNG có screw kết nối
        }

        public HashSet<ColorEnum> GetUniqueScrewColorsByLayer(int layer)
        {
            if (!screwDict.TryGetValue(layer, out var screws) || screws == null)
                return new HashSet<ColorEnum>();

            return screws
                .Where(s => s != null)
                .Select(s => s.GetColor())
                .ToHashSet();
        }
        public List<ColorEnum> GetScrewColorsByLayer(int layer)
        {
            if (layer < 0 || !screwDict.TryGetValue(layer, out var screws) || screws == null)
                return new List<ColorEnum>();

            // Lấy màu của từng screw còn tồn tại
            return screws
                .Where(s => s != null)
                .Select(s => s.GetColor())     // hoặc s.color / s.screwColor tùy class của bạn
                .ToList();
        }
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
            foreach (var part in parts)
            {
                part.Reset();
                PartPool.Instance.pool.ReturnToPool(part);
            }
        }

        internal int GetTopVisibleLayer()
        {
            if (screwDict == null || screwDict.Count == 0)
                return -1;

            int top = int.MaxValue;

            foreach (var kv in screwDict)
            {
                int layer = kv.Key;
                var list = kv.Value;

                if (list == null || list.Count == 0)
                    continue;

                bool hasValid = false;
                for (int i = 0; i < list.Count; i++)
                {
                    var s = list[i];
                    if (s == null) continue;
                    if (s.IsInHold) continue;

                    hasValid = true;
                    break;
                }

                if (!hasValid) continue;

                if (layer < top)
                    top = layer;
            }

            return top == int.MaxValue ? -1 : top ;
        }

    }
}