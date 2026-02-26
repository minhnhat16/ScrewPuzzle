using UnityEngine;

namespace Ingame.Pools
{
    public class BoxPool : MonoBehaviour
    {
        public static BoxPool Instance;
        public BY_Local_Pool<Box> pool;
        public Box prefab;
        public int total;
        private void Awake()
        {
            Instance = this;
            pool = new BY_Local_Pool<Box>(prefab, total, transform);
        }

        public Box Spawn()
        {
            return pool.SpawnNonGravity();
        }
        public void ReturnAll()
        {
            pool.DeSpawnAll();
        }

    }
}

