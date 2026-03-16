using Enums;
using Ingame.Board;
using Ingame.Screw;
using PoolManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        private Dictionary<ColorEnum, List<ScrewController>> hiddenByColor = new();

        private readonly List<ScrewController> screws = new();

        // ─── Hinge Connections (bidirectional) ─────────────────────
        // Tìm part qua hinge: O(1)
        private readonly Dictionary<HingeJoint2D, BasePart> _hingeToPartMap = new();
        // Tìm tất cả hinge của 1 part: O(1) — dùng để check còn hinge nào không
        private readonly Dictionary<BasePart, HashSet<HingeJoint2D>> _partToHingesMap = new();

        public event Action<ScrewController> OnScrewRemoved;

        public LayerMask LayerMask => layerMask;
        public List<ScrewController> Screws => screws;

        private void Start()
        {
            layerMask = LayerMask.GetMask("Screw");
        }

        //======================================================================//
        // PUBLIC API
        //======================================================================//

        public void AddScrew(ScrewController screw)
        {
            if (screw == null) return;
            if (!screws.Contains(screw))
                screws.Add(screw);
        }

        public void RemoveScrew(ScrewController screw)
        {
            if (screw == null) return;

            // Remove from LayerManager's dictionary
            var lm = LevelManager.ins.layerManager;
            lm.RemoveScrewOnDict(screw, screw.GetSortingOrder());

            var istrue = lm.screwDict.TryGetValue(screw.GetSortingOrder(), out var list) && list.Contains(screw);
            Debug.Log($"[ScrewManager] RemoveScrew: {screw.GetColor()}, with sorting {screw.GetSortingLayerName()} from LayerManager.screwDict, screw has in dict {istrue}");

            var hinge = screw.hingeController.HingeJoint2D;
            if (hinge != null)
                RemoveHingeConnection(hinge);

            screws.Remove(screw);
            OnScrewRemoved?.Invoke(screw);
        }

        internal void RemoveScrew(List<ScrewController> screwList)
        {
            if (screwList == null || screwList.Count == 0) return;

            var distinct = new HashSet<ScrewController>(screwList);

            foreach (var s in distinct)
            {
                if (s == null) continue;
                var h = s.hingeController.HingeJoint2D;
                if (h != null) RemoveHingeConnection(h);
                screws.Remove(s);
                OnScrewRemoved?.Invoke(s);
            }
        }

        internal void AddHiddenScrews(List<ScrewController> copy)
        {
            if (copy == null || copy.Count == 0) return;

            var hidden = copy.GroupBy(s => s.GetColor())
                             .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in hidden)
            {
                if (!hiddenByColor.ContainsKey(kvp.Key))
                    hiddenByColor[kvp.Key] = new List<ScrewController>();
                hiddenByColor[kvp.Key].AddRange(kvp.Value);
            }

            // Remove from LayerManager.screwDict so LayerVisibilityController won't re-activate these screws later
            try
            {
                var lm = LevelManager.ins.layerManager;
                if (lm != null)
                {
                    lm.RemoveScrewsOnDict(copy);
                    Debug.Log($"[ScrewManager] AddHiddenScrews: removed {copy.Count} screws from LayerManager.screwDict");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ScrewManager] AddHiddenScrews -> RemoveScrewsOnDict failed: {ex.Message}");
            }

            // Remove khỏi active screws list — screw đang vào hidden không còn trên board
            foreach (var s in copy)
            {
                if (s == null) continue;
                var hinge = s.hingeController?.HingeJoint2D;
                if (hinge != null)
                    RemoveHingeConnection(hinge);
                screws.Remove(s);
                // Không fire OnScrewRemoved — screw chưa bị xóa hoàn toàn, chỉ tạm ẩn
            }

            Debug.Log($"[ScrewManager] Added {copy.Count} hidden screws. Total hidden by color: " +
                      $"{string.Join(", ", hiddenByColor.Select(kvp => $"{kvp.Key}: {kvp.Value.Count}"))}");
        }

        //======================================================================//
        // HINGE CONNECTIONS — Bidirectional
        //======================================================================//

        /// <summary>
        /// Đăng ký hinge ↔ part.
        /// 1 part có thể có nhiều hinge (ví dụ part bị giữ bởi 2 screw).
        /// </summary>
        public void AddHingeConnection(HingeJoint2D hinge, BasePart part)
        {
            if (hinge == null || part == null)
            {
                Debug.LogWarning($"[ScrewManager] AddHingeConnection: hinge or part is null (hinge={hinge?.name}, part={part?.name}). Skipping.");
                return;
            }
            // hinge → part
            _hingeToPartMap[hinge] = part;

            // part → set of hinges
            if (!_partToHingesMap.ContainsKey(part))
                _partToHingesMap[part] = new HashSet<HingeJoint2D>();
            _partToHingesMap[part].Add(hinge);

            Debug.Log($"[ScrewManager] AddHingeConnection: hinge={hinge.name} → part={part.name} " +
                      $"(total hinges on part: {_partToHingesMap[part].Count})");
        }

        /// <summary>
        /// Xóa hinge khỏi map.
        /// Nếu part không còn hinge nào → gọi HandleNoHingesLeft().
        /// </summary>
        public void RemoveHingeConnection(HingeJoint2D hinge)
        {
            if (hinge == null) return;

            // Defensive: ensure _hingeToPartMap is up-to-date
            if (!_hingeToPartMap.TryGetValue(hinge, out var part))
            {
                // Try to recover: check if hinge is attached to a part via connectedBody
                var body = hinge.connectedBody;
                if (body != null)
                {
                    part = body.GetComponent<BasePart>();
                    if (part != null)
                    {
                        Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge '{hinge.name}' not in map, but found part via connectedBody: {part.name}. Recovering.");
                        // Register missing mapping for cleanup
                        _hingeToPartMap[hinge] = part;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge '{hinge.name}' không có trong map và không tìm được part.");
                    return;
                }
            }

            // Remove hinge → part
            _hingeToPartMap.Remove(hinge);

            // Remove hinge from set of part
            if (_partToHingesMap.TryGetValue(part, out var hingeSet))
            {
                hingeSet.Remove(hinge);

                int remaining = hingeSet.Count;
                Debug.Log($"[ScrewManager] RemoveHingeConnection: hinge={hinge.name}, part={part.name}, remaining hinges={remaining}");

                if (remaining == 0)
                {
                    _partToHingesMap.Remove(part);
                    part.HandleNoHingesLeft();
                }
            }
            else
            {
                Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: part={part.name} không có trong _partToHingesMap.");
                part.HandleNoHingesLeft();
            }
        }

        /// <summary>
        /// Query: part này còn bao nhiêu hinge đang active?
        /// </summary>
        public int GetHingeCountForPart(BasePart part)
        {
            if (part == null) return 0;
            return _partToHingesMap.TryGetValue(part, out var set) ? set.Count : 0;
        }

        /// <summary>
        /// Query: tìm part qua hinge — O(1).
        /// </summary>
        public BasePart GetPartByHinge(HingeJoint2D hinge)
        {
            if (hinge == null) return null;
            _hingeToPartMap.TryGetValue(hinge, out var part);
            return part;
        }

        //======================================================================//
        // UTILS
        //======================================================================//

        public int GetScrewTotalByColor(ColorEnum color)
        {
            if (screws == null || screws.Count == 0) return 0;
            return screws.Count(s => s != null && s.GetColor() == color);
        }

        public ScrewController RandomGetOneScrew()
        {
            if (screws.Count == 0) return null;

            int idx = UnityEngine.Random.Range(0, screws.Count);
            var screw = screws[idx];

            screw.screwPhysics.FreeHinge();
            ScrewPool.Instance.Pool.ReturnToPool(screw);

            screws.RemoveAt(idx);
            return screw; 
        }
        
        public void ReturnAllScrewToPool()
        {
            var pool = ScrewPool.Instance;
            foreach (var s in screws)
            {
                if (s == null) continue;
                s.OnReset();
                pool.Pool.ReturnToPool(s);
            }
            screws.Clear();
            _hingeToPartMap.Clear();
            _partToHingesMap.Clear();
        }
        public void Reset()
        {
            // Return active screws
            ReturnAllScrewToPool();
            // Return any hidden screws (from Breaker) to pool as well
            ReturnHiddenScrewsToPool();
            // Ensure hiddenByColor cleared
            hiddenByColor.Clear();
        }

        private void ReturnHiddenScrewsToPool()
        {
            if (hiddenByColor == null || hiddenByColor.Count == 0) return;

            var pool = ScrewPool.Instance;
            foreach (var kvp in hiddenByColor)
            {
                var list = kvp.Value;
                if (list == null) continue;
                foreach (var s in list)
                {
                    if (s == null) continue;
                    try
                    {
                        s.OnReset();
                        pool.Pool.ReturnToPool(s);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ScrewManager] ReturnHiddenScrewsToPool failed for screw: {ex.Message}");
                    }
                }
            }
        }

        internal List<ScrewController> PopHiddenScrew(ColorEnum color, int max)
        {
            if (!hiddenByColor.TryGetValue(color, out var list) || list.Count == 0)
                return new List<ScrewController>();

            int takeCount = Mathf.Min(max, list.Count);
            var popped = list.GetRange(0, takeCount);
            list.RemoveRange(0, takeCount);

            if (list.Count == 0)
                hiddenByColor.Remove(color);

            return popped;
        }

        internal void RemoveHidden(ScrewController screw)
        {
            if (screw == null) return;
            var color = screw.GetColor();
            if (!hiddenByColor.TryGetValue(color, out var list)) return;
            list.Remove(screw);
            if (list.Count == 0) hiddenByColor.Remove(color);
        }

        internal void RemoveHiddens(IEnumerable<ScrewController> screws)
        {
            if (screws == null) return;
            foreach (var group in screws.Where(s => s != null).GroupBy(s => s.GetColor()))
            {
                if (!hiddenByColor.TryGetValue(group.Key, out var list)) continue;
                foreach (var s in group) list.Remove(s);
                if (list.Count == 0) hiddenByColor.Remove(group.Key);
            }
        }

        public void RemoveFromColor(ColorEnum color, IEnumerable<ScrewController> screws)
        {
            if (!hiddenByColor.TryGetValue(color, out var list)) return;
            foreach (var s in screws) list.Remove(s);
            if (list.Count == 0) hiddenByColor.Remove(color);
        }

        public void OnValidate()
        {
            Debug.Log("[ScrewManager] OnValidate: checking for duplicate screws in inspector..." +
                $"+{(screws == null ? 0 : screws.Count)}, hinge to part map {_hingeToPartMap.Count}");
        }
    }
}