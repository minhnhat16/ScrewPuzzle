using UnityEngine;

namespace Ingame.Pools
{
    public class OneHoldBoxPool : MonoBehaviour
    {
        public static OneHoldBoxPool Instance;
        public BY_Local_Pool<BoxOneHold> pool;
        public BoxOneHold prefab;
        public int total;
        private void Awake()
        {
            Instance = this;
            pool = new BY_Local_Pool<BoxOneHold>(prefab, total, transform);
        }
    }
}
