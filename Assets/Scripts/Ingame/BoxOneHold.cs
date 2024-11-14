using Unity.VisualScripting;
using UnityEngine;

namespace Ingame
{
    public class BoxOneHold : ScrewBox
    {
        public override void OnEnable()
        {
            base.OnEnable();
            onScrewBoxFull.AddListener(BoxFullInvoker);
            spawnStartEvent.AddListener(SpawningStar);
            Transform = transform;
        }
        public override void Start()
        {
            // SetBoxColor(UnityEngine.Color.white);
        }
        private void OnDisable()
        {
            onScrewBoxFull.RemoveListener(BoxFullInvoker);

        }
    }
}
