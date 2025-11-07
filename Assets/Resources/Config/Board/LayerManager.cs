using PoolManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Ingame.Board
{
    public class LayerManager : MonoBehaviour
    {
        [SerializeField] List<BaseLayer> layers = new List<BaseLayer>();
        [SerializeField] private Dictionary<string, BasePart> partDict = new();
        [SerializeField] List<BasePart> parts = new List<BasePart>();
        public Dictionary<int, List<Screw.Screw>> screwDict = new();
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
        [SerializeField] private const int MaxVisibleLayers = 3;



        public void OnEnable()
        {
          
        }

    

        private void Awake()
        {
            visibilityController = GetComponent<LayerVisibilityController>();
        }

        private void Start()
        {
            GetChildrenInRange();
            // Thêm tất cả layer vào hàng đợi
            foreach (var layer in layers)
            {
                layerQueue.Enqueue(layer);
            }
            //ApplyLayerVisibility(); // Thiết lập hiển thị layer ban đầu
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
            // Đảm bảo layer đã clear không còn trong danh sách
            if (layers.Contains(clearedLayer))
            {
                layers.Remove(clearedLayer);
            }
            // Kiểm tra và cập nhật hiển thị các layer còn lại
            visibilityController.ApplyLayerVisibility();
        }

        public IEnumerator ChangePartState()
        {
            foreach (var part in parts)
            {
                part.Body.bodyType = RigidbodyType2D.Dynamic;
                yield return null;
            }
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
            if (idLayer >= layers.Count || idLayer <0)
            {
                ActiveAllLayers();
                return;
            }
            ;
            for(int i = 0; i < layers.Count; i++)
            {
                layers[i].gameObject.SetActive(idLayer == i);
                LayerUtils.ActiveObjectInLayer(idLayer == i,i, this);

            }
            int idScrewLayer = idLayer--;
        }
        public void ActiveAllLayers()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].gameObject.SetActive(true);
                LayerUtils.ActiveObjectInLayer(true,i, this);
            }
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
    }
}
