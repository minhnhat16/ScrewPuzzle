namespace Ingame
{
    public class BoxThreeHold : ScrewBox
    {
        private void OnEnable()
        {
            onScrewBoxFull.AddListener(BoxFullInvoker);
        }

        private void OnDisable()
        {
            onScrewBoxFull.RemoveListener(BoxFullInvoker);

        }
        // Start is called before the first frame update
        void Start()
        {
            /*if (onScrewBoxFull == null)
            {
                onScrewBoxFull = new();
                onScrewBoxFull.AddListener(BoxFullInvoker);
            }*/

        }

        // Update is called once per frame
        void Update()   
        {
        
        }
    }
}
