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

        // Danh sách screw hiện có (dạng ScrewController)
        private readonly List<ScrewController> screws = new();

        // Map hinge → part để kiểm tra nối kết
        private readonly Dictionary<HingeJoint2D, BasePart> hingeConnections = new();

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

        /// <summary> Remove a SINGLE screw </summary>
        public void RemoveScrew(ScrewController screw)
        {
            if (screw == null) return;

            // Remove hinge mapping
            var hinge = screw.GetComponent<HingeController>()?.HingeJoint2D;
            if (hinge != null)
                RemoveHingeConnection(hinge);

            // Remove from list
            screws.Remove(screw);

            // Notify listeners
            OnScrewRemoved?.Invoke(screw);
        }

        /// <summary> Remove MULTIPLE screws (optimized) </summary>
        internal void RemoveScrew(List<ScrewController> screwList)
        {
            if (screwList == null || screwList.Count == 0) return;

            var distinct = new HashSet<ScrewController>(screwList);
            var hingesToRemove = new HashSet<HingeJoint2D>();

            foreach (var s in distinct)
            {
                var h = s?.GetComponent<HingeController>()?.HingeJoint2D;
                if (h != null)
                    hingesToRemove.Add(h);
            }

            // Remove hinge connections first
            foreach (var hinge in hingesToRemove)
                RemoveHingeConnection(hinge);

            // Remove screws and dispatch event
            foreach (var screw in distinct)
            {
                if (screw == null) continue;
                screws.Remove(screw);
                OnScrewRemoved?.Invoke(screw);
            }
        }

        //======================================================================//
        // HINGE CONNECTIONS
        //======================================================================//

        public void AddHingeConnection(HingeJoint2D hinge, BasePart part)
        {
            if (hinge == null || part == null) return;

            if (!hingeConnections.ContainsKey(hinge))
                hingeConnections[hinge] = part;
        }

        public void RemoveHingeConnection(HingeJoint2D hinge)
        {
            if (hinge == null) return;

            if (!hingeConnections.TryGetValue(hinge, out var part))
                return;

            hingeConnections.Remove(hinge);

            bool hasAnyHinge = HasAnyHingeConnected(part);
            part.Body.gravityScale = 1.0f;

            if (!hasAnyHinge)
                part.HandleNoHingesLeft();
        }

        private bool HasAnyHingeConnected(BasePart part)
        {
            foreach (var p in hingeConnections.Values)
                if (p == part)
                    return true;
            return false;
        }

        //======================================================================//
        // UTILS
        //======================================================================//

        public int GetScrewTotalByColor(ColorEnum color)
        {
            if (screws == null || screws.Count == 0) return 0;

            int count = 0;
            foreach (var s in screws)
            {
                if (s == null) continue;
                if (s.GetColor() == color)
                    count++;
            }
            return count;
        }

        /// <summary> Random một screw hợp lệ </summary>
        public ScrewController RandomGetOneScrew()
        {
            if (screws.Count == 0) return null;

            int idx = UnityEngine.Random.Range(0, screws.Count);
            var screw = screws[idx];

            screw?.GetComponent<ScrewPhysics>()?.FreeHinge();
            ScrewPool.Instance.Pool.ReturnToPool(screw);

            screws.RemoveAt(idx);
            return screw;
        }

        /// <summary> Trả tất cả screw về pool </summary>
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
            hingeConnections.Clear();
        }

        public void Reset()
        {
            ReturnAllScrewToPool();
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
        internal void AddHiddenScrews(List<ScrewController> copy)
        {
            var hidden = copy.GroupBy(s => s.GetColor()).ToDictionary(g => g.Key, g => g.ToList()); 

            foreach (var kvp in hidden)
            {
                var color = kvp.Key;
                var screwList = kvp.Value;
                if (!hiddenByColor.ContainsKey(color))
                    hiddenByColor[color] = new List<ScrewController>();
                hiddenByColor[color].AddRange(screwList);
            }
        }

        internal void RemoveHidden(ScrewController screw)
        {
            if (screw == null)
                return;

            var color = screw.GetColor();

            if (!hiddenByColor.TryGetValue(color, out var list))
                return;

            list.Remove(screw);

            if (list.Count == 0)
                hiddenByColor.Remove(color);
        }

        internal void RemoveHiddens(IEnumerable<ScrewController> screws)
        {
            if (screws == null)
                return;

            var grouped = screws
                .Where(s => s != null)
                .GroupBy(s => s.GetColor());

            foreach (var group in grouped)
            {
                if (!hiddenByColor.TryGetValue(group.Key, out var list))
                    continue;

                foreach (var screw in group)
                {
                    list.Remove(screw);
                }

                if (list.Count == 0)
                    hiddenByColor.Remove(group.Key);
            }
        }

        public void RemoveFromColor(ColorEnum color, IEnumerable<ScrewController> screws)
        {
            if (!hiddenByColor.TryGetValue(color, out var list))
                return;

            foreach (var screw in screws)
                list.Remove(screw);

            if (list.Count == 0)
                hiddenByColor.Remove(color);
        }
    }
}
