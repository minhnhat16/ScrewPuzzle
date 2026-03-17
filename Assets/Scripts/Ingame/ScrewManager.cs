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
        private readonly Dictionary<HingeJoint2D, BasePart> _hingeToPartMap = new();
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
            // [FIX 4] Guard duplicate — pool reuse có thể AddScrew() lại screw đã có
            if (!screws.Contains(screw))
                screws.Add(screw);
        }

        public void RemoveScrew(ScrewController screw)
        {
            if (screw == null) return;

            var lm = LevelManager.ins.layerManager;
            lm.RemoveScrewOnDict(screw, screw.GetSortingOrder());

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

            foreach (var s in copy)
            {
                if (s == null) continue;
                var hinge = s.hingeController.HingeJoint2D;
                if (hinge != null)
                    RemoveHingeConnection(hinge);
                screws.Remove(s);
            }

            Debug.Log($"[ScrewManager] Added {copy.Count} hidden screws.");
        }

        //======================================================================//
        // HINGE CONNECTIONS — Bidirectional
        //======================================================================//

        public void AddHingeConnection(HingeJoint2D hinge, BasePart part)
        {
            if (hinge == null || part == null)
            {
                Debug.LogWarning($"[ScrewManager] AddHingeConnection: hinge or part is null. Skipping.");
                return;
            }

            if (_hingeToPartMap.TryGetValue(hinge, out var existingPart)
                && existingPart != null
                && existingPart != part)
            {
                Debug.LogWarning($"[ScrewManager] AddHingeConnection: hinge {hinge.name} already mapped to {existingPart.name}, remapping to {part.name}. Cleaning up stale entry.");
                // Xóa hinge khỏi set của part cũ
                if (_partToHingesMap.TryGetValue(existingPart, out var oldSet))
                {
                    oldSet.Remove(hinge);
                    if (oldSet.Count == 0)
                        _partToHingesMap.Remove(existingPart);
                }
            }

            _hingeToPartMap[hinge] = part;

            if (!_partToHingesMap.ContainsKey(part))
                _partToHingesMap[part] = new HashSet<HingeJoint2D>();
            _partToHingesMap[part].Add(hinge);

            Debug.Log($"[ScrewManager] AddHingeConnection: {hinge.name} → {part.name} " +
                      $"(hinges on part: {_partToHingesMap[part].Count})");
        }

        public void RemoveHingeConnection(HingeJoint2D hinge)
        {
            if (hinge == null) return;

            if (!_hingeToPartMap.TryGetValue(hinge, out var part))
            {
                // [FIX 2] Hinge không có trong map — có thể do reload rồi pool reuse
                // Thử recover qua connectedBody như cũ, nhưng log rõ hơn
                var body = hinge.connectedBody;
                if (body != null)
                {
                    part = body.GetComponent<BasePart>();
                    if (part != null)
                    {
                        Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge {hinge.name} not in map, recovered via connectedBody ({part.name}).");
                        _hingeToPartMap[hinge] = part;
                    }
                    else
                    {
                        Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge {hinge.name} — connectedBody has no BasePart. Skipping.");
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge {hinge.name} not in map and no connectedBody. Skipping.");
                    return;
                }
            }

            // [FIX 2] Unity destroyed object check
            if (part == null)
            {
                // Part đã bị destroy (pool/scene cleanup) — chỉ xóa hinge key, không gọi callback
                _hingeToPartMap.Remove(hinge);
                Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: part was destroyed, removing hinge key only.");
                return;
            }

            _hingeToPartMap.Remove(hinge);

            if (_partToHingesMap.TryGetValue(part, out var hingeSet))
            {
                hingeSet.Remove(hinge);
                int remaining = hingeSet.Count;
                Debug.Log($"[ScrewManager] RemoveHingeConnection: remaining hinges on {part.name} = {remaining}");

                if (remaining == 0)
                {
                    _partToHingesMap.Remove(part);
                    part.HandleNoHingesLeft();
                }
            }
            else
            {
                Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: part {part.name} not in _partToHingesMap. Calling HandleNoHingesLeft.");
                part.HandleNoHingesLeft();
            }
        }

        public int GetHingeCountForPart(BasePart part)
        {
            if (part == null) return 0;
            return _partToHingesMap.TryGetValue(part, out var set) ? set.Count : 0;
        }

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

            // [FIX 1] Clear cả 2 map sau khi return pool
            // Không để stale HingeJoint2D/BasePart reference tồn tại sang game tiếp theo
            _hingeToPartMap.Clear();
            _partToHingesMap.Clear();
        }

        public void Reset()
        {
            // Return active screws + clear hinge maps (gọi ReturnAllScrewToPool đã xử lý)
            ReturnAllScrewToPool();

            // [FIX 3] ReturnHiddenScrewsToPool xong thì clear map — tránh stale color entries
            ReturnHiddenScrewsToPool();
            hiddenByColor.Clear(); // đảm bảo clear dù ReturnHiddenScrewsToPool có exception
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
                        Debug.LogWarning($"[ScrewManager] ReturnHiddenScrewsToPool failed: {ex.Message}");
                    }
                }
                list.Clear(); // [FIX 3] Clear list ngay sau khi return
            }
            // hiddenByColor.Clear() được gọi ở Reset() sau hàm này
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

        public void SetAllScrewsInteractable(bool enable)
        {
            if (Screws == null) return;

            foreach (var s in Screws)
            {
                if (s == null || !s.isActiveAndEnabled) continue;
                try
                {
                    s.EnableColliderAndRig(enable);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ScrewManager] SetAllScrewsInteractable failed for {s?.name}: {ex.Message}");
                }
            }
        }

        public void OnValidate()
        {
            Debug.Log($"[ScrewManager] OnValidate: screws={screws?.Count ?? 0}, hingeMap={_hingeToPartMap.Count}, partMap={_partToHingesMap.Count}");
        }

        //======================================================================//
        // DEBUG HELPERS
        //======================================================================//

        /// <summary>
        /// Kiểm tra stale entries trong map — gọi sau mỗi reload để detect leak sớm.
        /// </summary>
        public void ValidateMaps()
        {
            int staleHinge = 0, stalePart = 0;

            foreach (var kvp in _hingeToPartMap.ToList())
            {
                // Unity destroyed object == null khi dùng == operator
                if (kvp.Key == null || kvp.Value == null)
                {
                    _hingeToPartMap.Remove(kvp.Key);
                    staleHinge++;
                }
            }

            foreach (var kvp in _partToHingesMap.ToList())
            {
                if (kvp.Key == null)
                {
                    _partToHingesMap.Remove(kvp.Key);
                    stalePart++;
                    continue;
                }
                kvp.Value.RemoveWhere(h => h == null);
                if (kvp.Value.Count == 0)
                    _partToHingesMap.Remove(kvp.Key);
            }

            if (staleHinge > 0 || stalePart > 0)
                Debug.LogWarning($"[ScrewManager] ValidateMaps: removed {staleHinge} stale hinge entries, {stalePart} stale part entries.");
            else
                Debug.Log("[ScrewManager] ValidateMaps: maps are clean.");
        }
    }
}
