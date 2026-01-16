using Enums;
using PoolManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private List<Screw.Screw> _screws = new();

        public Dictionary<HingeJoint2D, BasePart> hingeConnections = new Dictionary<HingeJoint2D, BasePart>();
        public event Action<Screw.Screw> OnScrewRemoved;
        public LayerMask LayerMask
        {
            get { return layerMask; }
            set { layerMask = value; }
        }
        private void Awake()
        {

        }

        private void Start()
        {
            layerMask = LayerMask.GetMask("Screw");
        }

        public void AttachScrewsToBoard(BaseWoodBoard baseWoodBoard)
        {

        }
#if UNITY_EDITOR

        public void AppendScrew(ScrewLevelMaker screw)
        {

            _screws.Add(screw);
        }
#endif
        public List<Screw.Screw> GetScrews()
        {
            Screw.Screw[] screwsInChildren = GetComponentsInChildren<Screw.Screw>();
            return screwsInChildren.ToList();
        }

        public void AddScrew(Screw.Screw screw)
        {

            _screws.Add(screw);
        }

        public void RemoveScrew(Screw.Screw screw)
        {
            // Remove all hinge connections for the given screw
            foreach (var hinge in screw.HingeController.HingeJoint2D)
            {
                RemoveHingeConnection(hinge);
            }

            // Gather distinct related parts only once
            var relatedParts = hingeConnections.Values.Distinct();

            // Process related parts and handle their state
            foreach (var part in relatedParts)
            {
                if (AreAllHingesRemoved(part))
                {
                    part.HandleNoHingesLeft();
                }
            }

            // Invoke the screw removal event after all operations
            OnScrewRemoved?.Invoke(screw);
        }

        internal void RemoveScrew(List<Screw.Screw> screwList)
        {
            if (screwList == null || screwList.Count == 0) return;

            // Normalize and dedupe input
            var distinctScrews = screwList.Where(s => s != null).Distinct().ToList();
            if (distinctScrews.Count == 0) return;

            // Collect hinges and affected parts first
            var hingesToRemove = new HashSet<HingeJoint2D>();
            var affectedParts = new HashSet<BasePart>();
            foreach (var screw in distinctScrews)
            {
                var hc = screw.HingeController;
                if (hc == null) continue;

                foreach (var hinge in hc.HingeJoint2D)
                {
                    if (hinge == null) continue;
                    hingesToRemove.Add(hinge);
                    if (hingeConnections.TryGetValue(hinge, out var part) && part != null)
                        affectedParts.Add(part);
                }
            }

            // Remove hinge connections
            foreach (var hinge in hingesToRemove)
            {
                RemoveHingeConnection(hinge);
            }

            // Remove screws from internal list and notify
            foreach (var screw in distinctScrews)
            {
                if (_screws.Contains(screw))
                    _screws.Remove(screw);

                OnScrewRemoved?.Invoke(screw);
            }

            // Final pass: ensure parts that lost all hinges are handled
            foreach (var part in affectedParts)
            {
                if (AreAllHingesRemoved(part))
                    part.HandleNoHingesLeft();
            }
        }
        public int GetScrewTotalByColor(ColorEnum color)
        {
            var screws = GetComponentsInChildren<Screw.Screw>(true);
            var listColor = screws.Where(s => s.Color == color).ToList();  
            int total = listColor.Count;
            return total;
        }
        public bool AreAllHingesRemoved(BasePart part)
        {
            // Kiểm tra xem part có còn trong hingeConnections hay không
            return !hingeConnections.Values.Contains(part);
        }
        public Screw.Screw RandomGetOneScrew()
        {
            Debug.LogWarning("Random Get One Screw");

            var tempScrews = _screws.OrderBy(screw => screw.layerMask).ToList();
            int totalScrew = tempScrews.Count - 1;
            int ramdomIndex = Random.Range(0, totalScrew);

            var rdScrew = tempScrews.ElementAt(ramdomIndex);
            rdScrew.FreeHinge();

            ScrewPool.Instance.Pool.ReturnToPool(rdScrew);
            _screws.Remove(rdScrew);
            Debug.LogWarning("Random Get One Screw" + rdScrew);

            return rdScrew;
        }

        public void ReturnAllScrewToPool()
        {
            var screwPool = ScrewPool.Instance;
            foreach (var screw in _screws)
            {
                screw.OnReset();
                screwPool.Pool.ReturnToPool(screw);
            }
        }
#if UNITY_EDITOR
        public void ResetHinge()
        {
            //Debug.LogError("reset hinge");
            var screws = GetComponentsInChildren<ScrewLevelMaker>();
            foreach (ScrewLevelMaker screw in screws)
            {
                screw.ResetHinge();
            }
        }
#endif
        public void AddHingeConnection(HingeJoint2D hinge, BasePart part)
        {
            if (!hingeConnections.ContainsKey(hinge))
            {
                hingeConnections[hinge] = part;
                //Debug.Log($"Hinge added: {hinge} connected to {part.uniqueID}");
            }
        }

        public void RemoveHingeConnection(HingeJoint2D hinge)
        {
            if (hingeConnections.ContainsKey(hinge))
            {
                var part = hingeConnections[hinge];
                hingeConnections.Remove(hinge);

                // Debug.Log($"Hinge removed: {hinge} disconnected from {part.uniqueID}");
                part.HandleNoHingesLeft();

                // Kiểm tra ngay khi xóa nếu part không còn liên kết
                if (AreAllHingesRemoved(part))
                {
                    //Debug.Log($"All hinges for part {part.uniqueID} have been removed.");
                    part.HandleNoHingesLeft();
                }
            }
        }



        public void ResetAllPartsAndHinges()
        {
            foreach (var part in hingeConnections.Values.Distinct())
            {
                if (AreAllHingesRemoved(part))
                {
                    Debug.Log($"Resetting part {part.uniqueID}, no hinge connections remain.");
                    part.Reset();
                }
            }

            hingeConnections.Clear();
            Debug.Log("All hinge connections cleared.");
        }

        public void CheckAllHinges()
        {
            foreach (var hinge in hingeConnections.Keys)
            {
                if (hinge.connectedBody == null)
                {
                    Debug.Log($"Hinge {hinge} is no longer connected.");
                    RemoveHingeConnection(hinge);
                }
            }
        }

        public void Reset()
        {
            ReturnAllScrewToPool();
        }


    }
}