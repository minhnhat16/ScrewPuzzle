using ConfigFile;
using UnityEngine;

namespace Ingame
{
    public class BoxTwoHold : ScrewBox
    {
        public override void Start()
        {
            Transform = transform.GetComponent<Transform>(); 
            // SetBoxColor(UnityEngine.Color.white);
        }
        private void OnEnable()
        {
            onScrewBoxFull.AddListener(BoxFullInvoker);
        }

        private void OnDisable()
        {
            onScrewBoxFull.RemoveListener(BoxFullInvoker);

        }
    }
}
