using System;
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
            for(int i = 0; i < pool.list.Count; i++)
            {
                if(pool.list[i].gameObject.activeSelf)
                {
                    ReturnToPool(pool.list[i]);
                }
            }
        }

        internal void ReturnToPool(Box box)
        {
            box.OnReset();
            pool.ReturnToPool(box);
        }
    }
}

