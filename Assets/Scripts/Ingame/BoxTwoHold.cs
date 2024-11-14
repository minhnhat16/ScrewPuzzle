using ConfigFile;
using UnityEngine;

namespace Ingame
{
    public class BoxTwoHold : ScrewBox
    {
        public override void Start()
        {
            // SetBoxColor(UnityEngine.Color.white);
        }
        public override void OnEnable()
        {
            Transform = transform; 
            onScrewBoxFull.AddListener(BoxFullInvoker);
            spawnStartEvent.AddListener(SpawningStar);

        }

        private void OnDisable()
        {
            onScrewBoxFull.RemoveListener(BoxFullInvoker);

        }
    }
}
