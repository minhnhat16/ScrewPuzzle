using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIScript
{
    public abstract class ItemButton :MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Button addQuantityBtn;
        [SerializeField] private int quantity = 1;
        [SerializeField] private Text text_lb;
        [SerializeField] private ItemType type;
        public Button Button
        {
            get => button;
            set => button = value;
        }
        public Button AddQuantityBtn
        {
            get => addQuantityBtn;
            set => addQuantityBtn = value;
        }
        public Text TextLB
        {
            get => text_lb;
            set => text_lb = value;
        }

        public ItemType Type
        {
            get => type;
            set => type = value;
        }

        public UnityEvent<int> Event1
        {
            get => Event;
            set => Event = value;
        }
        public int Quantity { get => quantity; set => quantity = value; }

        [HideInInspector]
        public UnityEvent<int> Event = new UnityEvent<int>();


        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public virtual void OnEnable()
        {
        }
        public virtual void OnDisable()
        {

        }
        public virtual void OnClick()
        {
            Debug.Log("on button click");
            IngameController.ins.OnItemInvoke?.Invoke(type,Vector3.one);
        }

        public virtual void OnAddQuantity()
        {
            
        }
    }
}
