using System.Collections.Generic;
using System.Linq;
using PoolManager;
using UnityEngine;
using Ingame.Screw;
namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
       [SerializeField] private LayerMask layerMask;
       [SerializeField] private List<Screw.Screw> _screws = new();
        public Dictionary<HingeJoint2D, BasePart> hingeConnections = new Dictionary<HingeJoint2D, BasePart>();

        public LayerMask LayerMask
        {
            get { return layerMask;}
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
        public void AppendScrew(ScrewLevelMaker screw)
        {
            _screws.Add(screw); 
        }
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
            var hingeJoints = screw.HingeController.HingeJoint2D;
            foreach(var hinge in hingeJoints)
            {
                RemoveHingeConnection(hinge);
            }
            _screws.Remove(screw);
        }
        public bool AreAllHingesRemoved(BasePart part)
        {
            // Check if any hinge in the dictionary is associated with the given part
            foreach (var kvp in hingeConnections)
            {
                if (kvp.Value == part)
                {
                    return false; // Found a hinge connected to the part
                }
            }
            return true; // No hinges associated with the part
        }

        public void ReturnAllScrewToPool()
        {
            var screwPool = ScrewPool.Instance;
            foreach (var screw in _screws)
            {
                screw.Reset();
                screwPool.Pool.ReturnToPool(screw);
            }
        }
        public void ResetHinge()
        {
            Debug.LogError("reset hinge");
            var screws = GetComponentsInChildren<ScrewLevelMaker>();
            foreach (ScrewLevelMaker screw in screws)
            {
                screw.ResetHinge();
            }
        }
        public void AddHingeConnection(HingeJoint2D hinge, BasePart part)
        {
            if (!hingeConnections.ContainsKey(hinge))
            {
                hingeConnections[hinge] = part;
                Debug.Log($"Hinge added: {hinge} connected to {part.uniqueID}");
            }
        }

        public void RemoveHingeConnection(HingeJoint2D hinge)
        {
            if (hingeConnections.ContainsKey(hinge))
            {
                var part = hingeConnections[hinge];
                hingeConnections.Remove(hinge);
                Debug.Log($"Hinge removed: {hinge} disconnected from {part.uniqueID}");
            }
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