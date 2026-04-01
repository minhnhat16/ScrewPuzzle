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
        private readonly Dictionary<ColorEnum, List<ScrewController>> hiddenByColor = new();

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

        // [FIX A] — Clear dict ngay khi object được enable lại (pool reuse / scene reload)
        // Đảm bảo không còn stale reference từ lần chơi trước dù Reset() chưa kịp gọi
        private void OnEnable()
        {
            Clear();
        }

        public void Clear()
        {
            _hingeToPartMap.Clear();
            _partToHingesMap.Clear();
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

            var lm = LevelManager.ins.layerManager;
            lm.RemoveScrew(screw);

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
                    //Debug.Log($"[ScrewManager] AddHiddenScrews: removed {copy.Count} screws from LayerManager.screwDict");
                }
            }
            catch (Exception ex)
            {
                //Debug.LogWarning($"[ScrewManager] AddHiddenScrews -> RemoveScrewsOnDict failed: {ex.Message}");
            }

            foreach (var s in copy)
            {
                if (s == null) continue;
                s.MarkDetachedFromBoard();
                var hinge = s.hingeController.HingeJoint2D;
                if (hinge != null)
                    RemoveHingeConnection(hinge);
                screws.Remove(s);
            }

            //Debug.Log($"[ScrewManager] Added {copy.Count} hidden screws.");
        }

        //======================================================================//
        // HINGE CONNECTIONS — Bidirectional
        //======================================================================//

        public void AddHingeConnection(HingeJoint2D hinge, BasePart part)
        {
            if (hinge == null || part == null)
            {
                //Debug.LogWarning("[ScrewManager] AddHingeConnection: hinge or part is snull. Skipping.");
                return;
            }

            // Nếu hinge đã map sang part khác, cleanup đúng chiều
            if (_hingeToPartMap.TryGetValue(hinge, out var existingPart) && existingPart != part)
            {
                if (_partToHingesMap.TryGetValue(existingPart, out var oldSet))
                {
                    oldSet.Remove(hinge);
                    if (oldSet.Count == 0)
                    {
                        _partToHingesMap.Remove(existingPart);
                        existingPart.HandleNoHingesLeft();
                    }
                }
            }

            // Map mới
            _hingeToPartMap[hinge] = part;

            if (!_partToHingesMap.TryGetValue(part, out var hingeSet))
            {
                hingeSet = new HashSet<HingeJoint2D>();
                _partToHingesMap[part] = hingeSet;


                string set = string.Join(", ", hingeSet.Select(h => h.name));
                //Debug.Log($"[ScrewManager] AddHingeConnection: created new hinge set for part {part.name}: set {set == null}");
            }
            hingeSet.Add(hinge);

            //Debug.Log($"[ScrewManager] AddHingeConnection: {hinge.name} → {part.name} (total: {hingeSet.Count})");
        }
        public void RemoveHingeConnection(HingeJoint2D hinge)
        {
            if (hinge == null) return;

            if (!_hingeToPartMap.TryGetValue(hinge, out var part) || part == null)
            {
                // Recovery: tìm part qua connectedBody nếu có
                var body = hinge.connectedBody;
                if (body != null)
                {
                    part = body.GetComponent<BasePart>();
                    if (part != null)
                    {
                        //Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge {hinge.name} not in map, recovered via connectedBody ({part.name}).");
                        _hingeToPartMap[hinge] = part;
                    }
                    else
                    {
                        //Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge {hinge.name} — connectedBody has no BasePart. Skipping.");
                        return;
                    }
                }
                else
                {
                    //Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: hinge {hinge.name} not in map and no connectedBody. Skipping.");
                    return;
                }
            }

            _hingeToPartMap.Remove(hinge);

            if (_partToHingesMap.TryGetValue(part, out var hingeSet))
            {
                hingeSet.Remove(hinge);
                //Debug.Log($"[ScrewManager] RemoveHingeConnection: removed {hinge.name} from {part.name}, remaining: {hingeSet.Count}");

                if (hingeSet.Count == 0)
                {
                    _partToHingesMap.Remove(part);
                    part.HandleNoHingesLeft();
                }
            }
            else
            {
                //Debug.LogWarning($"[ScrewManager] RemoveHingeConnection: part {part.name} not in _partToHingesMap. Calling HandleNoHingesLeft.");
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
            ScrewPool.Instance.ReturnScrewToPool(screw);

            screws.RemoveAt(idx);
            return screw;
        }

        public void ReturnAllScrewToPool()
        {
            var pool = ScrewPool.Instance;

            // [FIX C] — Gọi RemoveHingeConnection từng cái thay vì Clear() thô
            // Đảm bảo HandleNoHingesLeft() được gọi đúng cho mọi BasePart còn sót
            // Tránh BasePart giữ state sai sang lần reload tiếp theo
            foreach (var s in screws)
            {
                if (s == null) continue;

                var hinge = s.hingeController?.HingeJoint2D;
                if (hinge != null)
                    RemoveHingeConnection(hinge);

                s.OnReset();
                pool.Pool.ReturnToPool(s);
            }
            screws.Clear();

            // Clear phần còn sót (hinges không có screw tương ứng hoặc bị miss)
            _hingeToPartMap.Clear();
            _partToHingesMap.Clear();
        }

        public void Reset()
        {


            //Debug.Log("[ScrewManager] Resetting screws and hinge connections...");  
            ReturnAllScrewToPool();

            ReturnHiddenScrewsToPool();
            hiddenByColor.Clear();

            // [FIX D] — Validate sau Reset để detect stale leak sớm, log ra console
            ValidateMaps();
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
                        //Debug.LogWarning($"[ScrewManager] ReturnHiddenScrewsToPool failed: {ex.Message}");
                    }
                }
                list.Clear();
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

        public void SetAllScrewsInteractable(bool enable)
        {
            if (Screws == null) return;

            foreach (var s in Screws)
            {
                if (s == null || !s.isActiveAndEnabled) continue;
                s.EnableColliderAndRig(enable);

            }
        }

        public void ApplyTutorialTargetInput(string targetKey)
        {
            if (screws == null) return;

            foreach (var screw in screws)
            {
                if (screw == null) continue;
                screw.SetTutorialInputEnabled(!string.IsNullOrEmpty(targetKey) && screw.tutorialKey == targetKey);
            }
        }

        public void ClearTutorialInputFilter()
        {
            if (screws == null) return;

            foreach (var screw in screws)
            {
                if (screw == null) continue;
                screw.SetTutorialInputEnabled(true);
            }
        }

        public void OnValidate()
        {
            ////Debug.Log($"[ScrewManager] OnValidate: screws={screws?.Count ?? 0}, hingeMap={_hingeToPartMap.Count}, partMap={_partToHingesMap.Count}");
        }

        //======================================================================//
        // //Debug HELPERS
        //======================================================================//

        /// <summary>
        /// Kiểm tra stale entries trong map.
        /// Tự động gọi sau mỗi Reset() — không cần gọi thủ công.
        /// </summary>
        public void ValidateMaps()
        {
            int staleHinge = 0, stalePart = 0;

            foreach (var kvp in _hingeToPartMap.ToList())
            {
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

            //if (staleHinge > 0 || stalePart > 0)
            //    //Debug.LogWarning($"[ScrewManager] ValidateMaps: removed {staleHinge} stale hinge entries, {stalePart} stale part entries.");
            //else
            //    //Debug.Log("[ScrewManager] ValidateMaps: maps are clean.");
        }

        internal int GetHiddenScrew(ColorEnum color)
        {
            if (!hiddenByColor.TryGetValue(color, out var list) || list.Count == 0)
                return 0;
            return list.Count;
        }

        internal int GetHiddenCountByColor(ColorEnum color)
        {
            if (!hiddenByColor.TryGetValue(color, out var list) || list.Count == 0)
                return 0;
            return list.Count;
        }
    }
}
