using ConfigFile;
using UnityEngine;

namespace Ingame
{
    public class BoxThreeHold : Box
    {

        public override void Start()
        {
            // SetBoxColor(UnityEngine.Color.white);
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Transform = transform; 
            onScrewBoxFull.AddListener(BoxFullInvoker);

        }

        public override void OnDisable()
        {
            onScrewBoxFull.RemoveListener(BoxFullInvoker);

        }
       
    }
}
