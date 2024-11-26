using System.Collections.Generic;
using System.Linq;
using PoolManager;
using UnityEngine;
using Ingame.Screw;
using System;
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
            var hingeJoints = screw.HingeController.HingeJoint2D;

            foreach (var hinge in hingeJoints)
            {
                // Xóa liên kết giữa HingeJoint2D và BasePart
                RemoveHingeConnection(hinge);
            }

            // Kiểm tra trạng thái của các BasePart liên quan
            var relatedParts = hingeConnections.Values.Distinct().ToList();
            foreach (var part in relatedParts)
            {
                if (AreAllHingesRemoved(part))
                {
                   // Debug.Log($"All hinges for part {part.uniqueID} have been removed.");
                    // Thực hiện logic bổ sung, ví dụ:
                    part.HandleNoHingesLeft();
                }
            }

            OnScrewRemoved?.Invoke(screw);
            //_screws.Remove(screw);
        }


        public bool AreAllHingesRemoved(BasePart part)
        {
            // Kiểm tra xem part có còn trong hingeConnections hay không
            return !hingeConnections.Values.Contains(part);
        }
        public Screw.Screw RandomGetOneScrew()
        {
            Debug.LogWarning("Random Get One Screw");

            var tempScrews = _screws.OrderBy(screw => screw.LayerMask).ToList();
            int totalScrew = tempScrews.Count - 1;
            int ramdomIndex = Random.Range(0, totalScrew);

            var rdScrew = tempScrews.ElementAt(ramdomIndex);
            rdScrew.FreeHinge();

            ScrewPool.Instance.Pool.ReturnToPool(rdScrew);
            _screws.Remove(rdScrew);
            Debug.LogWarning("Random Get One Screw"+  rdScrew);

            return rdScrew;
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