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

            var hinge = screw.hingeController?.HingeJoint2D;
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
                var h = s.hingeController?.HingeJoint2D;
                if (h != null) RemoveHingeConnection(h);
                screws.Remove(s);
                OnScrewRemoved?.Invoke(s);
            }
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
            if (hinge == null || part == null) return;

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

            if (!_hingeToPartMap.TryGetValue(hinge, out var part))
            {
                Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge '{hinge.name}' không có trong map.");
                return;
            }

            // Xóa hinge → part
            _hingeToPartMap.Remove(hinge);

            // Xóa hinge khỏi set của part
            if (_partToHingesMap.TryGetValue(part, out var hingeSet))
            {
                hingeSet.Remove(hinge);

                int remaining = hingeSet.Count;
                Debug.Log($"[ScrewManager] RemoveHingeConnection: hinge={hinge.name}, part={part.name}, " +
                          $"remaining hinges={remaining}");

                if (remaining == 0)
                {
                    _partToHingesMap.Remove(part);
                    part.HandleNoHingesLeft();
                }
            }
            else
            {
                // Map không đồng bộ — vẫn gọi HandleNoHingesLeft để an toàn
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

        public void Reset() => ReturnAllScrewToPool();

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

        internal void AddHiddenScrews(List<ScrewController> copy)
        {
            var hidden = copy.GroupBy(s => s.GetColor())
                             .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in hidden)
            {
                if (!hiddenByColor.ContainsKey(kvp.Key))
                    hiddenByColor[kvp.Key] = new List<ScrewController>();
                hiddenByColor[kvp.Key].AddRange(kvp.Value);

            }
            Debug.Log($"[ScrewManager] Added {copy.Count} hidden screws. Total hidden by color: " +
                      $"{string.Join(", ", hiddenByColor.Select(kvp => $"{kvp.Key}: {kvp.Value.Count}"))}");
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
    }
}