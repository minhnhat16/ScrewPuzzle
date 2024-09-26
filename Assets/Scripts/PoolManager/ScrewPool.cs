using Ingame.Screw;
using UnityEngine;

namespace PoolManager
{
    public class ScrewPool : MonoBehaviour
    {
        public static ScrewPool Instance;
        public BY_Local_Pool<Screw> Pool;
        public Screw prefab;
        public int total;
        private void Awake()
        {
            Instance = this;
            Pool = new BY_Local_Pool<Screw>(prefab, total, transform);
        }
    }
}
