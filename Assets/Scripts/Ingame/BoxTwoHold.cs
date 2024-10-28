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
        private void OnEnable()
        {
            Transform = transform; 
            onScrewBoxFull.AddListener(BoxFullInvoker);
        }

        private void OnDisable()
        {
            onScrewBoxFull.RemoveListener(BoxFullInvoker);

        }
    }
}
