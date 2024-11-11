using System;
using System.Collections.Generic;
using System.Linq;
using ConfigFile.ConfigFile;
using Level;
using PoolManager;
using Unity.VisualScripting;
using UnityEngine;

namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
       [SerializeField] private LayerMask layerMask;
       [SerializeField] private List<Screw.Screw> _screws = new();
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
        public void Reset()
        {
            ReturnAllScrewToPool();
        }
    }
}