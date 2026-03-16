using Ingame;
using System.Collections.Generic;
using UnityEngine;

namespace PoolManager
{
    public class PartPool : MonoBehaviour
    {
        public static PartPool Instance;
        public BY_Local_Pool<BasePart> pool;
        public BasePart prefab;
        public int total;
        private void Awake()
        {
            Instance = this;
            pool = new BY_Local_Pool<BasePart>(prefab, total, transform);
        }


        private void ReturnToPool(BasePart part)
        {
            part.Reset();

            pool.ReturnToPool(part);
        }
        public void ReturnAll(List<BasePart> parts)
        {
            foreach (var part in parts)
            {
                ReturnToPool(part);
            }
        }
    }
}
