using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIScript
{
    public abstract class ItemButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        public Button Button
        {
            get => button;
            set => button = value;
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

        [SerializeField] private Text text_lb;
        [SerializeField] private ItemType type;
        [HideInInspector]
        public UnityEvent<int> Event = new UnityEvent<int>();

        public virtual void OnEnable()
        {
        }

        public virtual void OnClick()
        {
            Debug.Log("on button click");
            IngameController.Instance.onItemInvoke?.Invoke(type);
        }
    }
}
