using Ingame.Screw;
using PoolManager;
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
            visibilityController.ShowNextLayer();
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
        public void RemoveScrewOnDict(Screw.Screw screw, int layer)
        {
            layer -= 9;
            screwDict.TryGetValue(layer, out List<Screw.Screw> listScrews);
            Debug.Log("Remove screw on layer " + layer + " total screws " + (listScrews != null ? listScrews.Count.ToString() : "null"));
            if (listScrews == null) return;
            listScrews.Remove(screw);
        }
        public List<Screw.Screw> GetScrewByPart(BasePart part)
        {
            Debug.Log("part null: " +part.name);
            if (part == null) return new List<Screw.Screw>();

            int layer = part.PartLayer() - 10;

            if (!screwDict.TryGetValue(layer, out var screwsInLayer) || screwsInLayer == null || screwsInLayer.Count == 0)
            {
                Debug.Log("Screw in layer ");
                return new List<Screw.Screw>();

            }

            // Match any hinge connected body to the part's Rigidbody2D (safe null checks)
            var result = screwsInLayer
                .Where(s => s?.HingeController?.BodyConnect != null && s.HingeController.BodyConnect.Any(b => b == part.Body))
                .ToList();

            return result;
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
