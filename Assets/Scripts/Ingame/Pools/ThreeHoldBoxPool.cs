using UnityEngine;

namespace Ingame.Pools
{
    public class ThreeHoldBoxPool : MonoBehaviour
    {
        public static ThreeHoldBoxPool Instance;
        public BY_Local_Pool<BoxThreeHold> pool;
        public BoxThreeHold prefab;
        public int total;
        private void Awake()
        {
            Instance = this;
            pool = new BY_Local_Pool<BoxThreeHold>(prefab, total, transform);
        }
    }
}

