using Ingame.Screw;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace PoolManager
{
    public class ScrewPool : MonoBehaviour
    {
        public static ScrewPool Instance;
        public BY_Local_Pool<ScrewController> Pool;
        public ScrewController prefab;
        public int total;
        private void Awake()
        {
            Instance = this;
            Pool = new BY_Local_Pool<ScrewController>(prefab, total, transform);
        }

        public void ReturnScrewToPool(ScrewController screw)
        {
            screw.OnReset();
            Pool.ReturnToPool(screw);
        }
        public void ReturnScrewToPool(List<ScrewController> screws)
        {
            foreach (var screw in screws)
            {
                screw.OnReset();
                Pool.ReturnToPool(screw);
            }
        }
    }
}
