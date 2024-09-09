using System;
using UnityEngine;

namespace Ingame
{
    public class ScrewManager : MonoBehaviour
    {
        public static ScrewManager instance;
       [SerializeField] private LayerMask layerMask;
        public LayerMask LayerMask
        {
            get { return layerMask;}
            set { layerMask = value; }
        }
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            };
        }

        private void Start()
        {
            layerMask = LayerMask.GetMask("Screw");
            
        }

        public void AttachScrewsToBoard(BaseWoodBoard baseWoodBoard)
        {
            
        }

        public void AddScrew(Screw screw)
        {
        }
    }
}