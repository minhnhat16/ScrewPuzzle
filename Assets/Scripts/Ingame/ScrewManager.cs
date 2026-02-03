using Enums;
using PoolManager;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;

        // Danh sách screw hiện có
        private readonly List<Screw.Screw> screws = new();

        // Map hinge → part để kiểm tra nối kết
        private readonly Dictionary<HingeJoint2D, BasePart> hingeConnections = new();

        public event Action<Screw.Screw> OnScrewRemoved;

        public LayerMask LayerMask => layerMask;

        public List<Screw.Screw> Screws => screws;

        private void Start()
        {
            layerMask = LayerMask.GetMask("Screw");
        }

        //======================================================================//
        //  PUBLIC API
        //======================================================================//

        public void AddScrew(Screw.Screw screw)
        {
            if (screw == null) return;
            if (!screws.Contains(screw))
                screws.Add(screw);
        }

        /// <summary>
        /// Remove a SINGLE screw
        /// </summary>
        public void RemoveScrew(Screw.Screw screw)
        {
            if (screw == null) return;

            // Remove hinges
            var hinge = screw.HingeController?.HingeJoint2D;
            if (hinge != null)
            {
                RemoveHingeConnection(hinge);
            }

            // Remove from list
            screws.Remove(screw);

            // Notify listener
            OnScrewRemoved?.Invoke(screw);
        }


        /// <summary>
        /// Remove MULTIPLE screws at once (optimized)
        /// </summary>
        internal void RemoveScrew(List<Screw.Screw> screwList)
        {
            if (screwList == null || screwList.Count == 0) return;

            // dedup
            var distinct = new HashSet<Screw.Screw>(screwList);

            // Collect hinges to remove first
            var hingesToRemove = new HashSet<HingeJoint2D>();
            foreach (var s in distinct)
            {
                var h = s?.HingeController?.HingeJoint2D;
                if (h == null) continue;
                hingesToRemove.Add(h);
            }

            // Remove hinges first
            foreach (var hinge in hingesToRemove)
                RemoveHingeConnection(hinge);

            // Remove screws from list
            foreach (var screw in distinct)
            {
                if (screw != null)
                {
                    screws.Remove(screw);
                    OnScrewRemoved?.Invoke(screw);
                }
            }
        }

        //======================================================================//
        //  HINGE CONNECTIONS
        //======================================================================//

        /// <summary>
        /// Đăng ký kết nối hinge → part
        /// </summary>
        public void AddHingeConnection(HingeJoint2D hinge, BasePart part)
        {
            if (hinge == null || part == null) return;

            if (!hingeConnections.ContainsKey(hinge))
                hingeConnections[hinge] = part;
        }

        /// <summary>
        /// Remove a hinge → kiểm tra part có còn hinge khác không
        /// </summary>
        public void RemoveHingeConnection(HingeJoint2D hinge)
        {
            if (hinge == null) return;

            if (!hingeConnections.TryGetValue(hinge, out var part))
                return;

            hingeConnections.Remove(hinge);
            // Nếu không còn hinge nào thuộc về part này → gọi HandleNoHingesLeft()
            bool hasAnyHingeConnected = HasAnyHingeConnected(part);
            part.Body.gravityScale = 1.0f;
            //Debug.Log("Has any hinge connect " + hasAnyHingeConnected + " partId" + part.uniqueID);
            if (!hasAnyHingeConnected)
                part.HandleNoHingesLeft();
        }

        /// <summary>
        /// Kiểm tra part còn hinge khác hay không
        /// </summary>
        private bool HasAnyHingeConnected(BasePart part)
        {
            foreach (var p in hingeConnections.Values)
                if (p == part)
                    return true;
            return false;
        }

        //======================================================================//
        //  UTILS
        //======================================================================//

        /// <summary>
        /// Trả về tổng số screw theo màu
        /// </summary>
        public int GetScrewTotalByColor(ColorEnum color)
        {
            int count = 0;
            foreach (var s in screws)
                if (s != null && s.Color == color)
                    count++;
            return count;
        }

        /// <summary>
        /// Random một screw hợp lệ
        /// </summary>
        public Screw.Screw RandomGetOneScrew()
        {
            if (screws.Count == 0) return null;

            int idx = UnityEngine.Random.Range(0, screws.Count); // FIX off-by-one
            var sc = screws[idx];

            sc?.FreeHinge();
            ScrewPool.Instance.Pool.ReturnToPool(sc);

            screws.RemoveAt(idx);
            return sc;
        }

        /// <summary>
        /// Trả tất cả screw về pool
        /// </summary>
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

        //======================================================================//
        //  EDITOR ONLY
        //======================================================================//
#if UNITY_EDITOR
        public void AppendScrew(ScrewLevelMaker screw)
        {
            if (screw != null)
                screws.Add(screw);
        }

        public void ResetHinge()
        {
            var makerScrews = GetComponentsInChildren<ScrewLevelMaker>();
            foreach (var s in makerScrews)
                s.ResetHinge();
        }
#endif
    }
}
