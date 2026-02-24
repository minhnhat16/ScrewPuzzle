using UnityEngine;

namespace Ingame.Screw
{
    public class ScrewPhysics : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D circleCollider;
        [SerializeField] private HingeController hingeController;

        public bool IsBlocked()
        {
            int connectedBodyLayer = hingeController.GetIntBodyLayer(0);
            LayerMask mask = 0;

            for (int i = 10; i < connectedBodyLayer; i++)
                mask |= (1 << i);

            var hits = Physics2D.OverlapCircleAll(circleCollider.transform.position, circleCollider.radius - 0.1f, mask);
            return hits.Length > 0;
        }

        public void FreeHinge() => hingeController.FreeHinges();

        public void DisableCollider() => circleCollider.enabled = false;

        public void EnableCollider() => circleCollider.enabled = true;

        public void ResetPhysics()
        {
            circleCollider.enabled = true;
            circleCollider.isTrigger = false;
        }
    }
}
