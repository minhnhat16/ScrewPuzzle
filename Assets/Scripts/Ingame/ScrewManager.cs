using System;
using UnityEngine;

namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
       [SerializeField] private LayerMask layerMask;
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

        public void AddScrew(Screw.Screw screw)
        {
        }
    }
}