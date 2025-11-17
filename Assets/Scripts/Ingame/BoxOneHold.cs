using Unity.VisualScripting;
using UnityEngine;

namespace Ingame
{
    public class BoxOneHold : Box
    {
        public override void OnEnable()
        {
            base.OnEnable();
            onScrewBoxFull.AddListener(BoxFullInvoker);
            Transform = transform;
        }
        public override void Start()
        {
            // SetBoxColor(UnityEngine.Color.white);
        }
        public override void OnDisable()
        {
            onScrewBoxFull.RemoveListener(BoxFullInvoker);

        }
    }
}
