using Ingame;
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
    }
}
