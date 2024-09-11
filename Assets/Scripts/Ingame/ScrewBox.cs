using ConfigFile;
using Enum;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class ScrewBox : FSMSystem   
    {
        public BoxConfig config; // ScriptableObject chứa cấu hình cho CrewBox
        [SerializeField] private SpriteRenderer render;
        [SerializeField] private bool isBoxFull;
        [SerializeField] private int nextEmptyIndex = -1;
        [SerializeField] public HoldScrew[] holdScrews; // Mảng các lỗ Screw
        [SerializeField] public UnityEvent<bool> onScrewBoxFull;
        [SerializeField] private ColorEnum color;
        public bool IsBoxFull
        {
            get => isBoxFull;
            set => isBoxFull = value;
        }
        public ColorEnum Color {get => color;
            set => color=value;
        }
        public SpriteRenderer Render {get => render;
            set => render = value;
        }
        private void OnEnable()
        {
        }

      

        void Start()
        {
            // Khởi tạo số lượng lỗ dựa trên config
            // holdScrews = new Screw[config.numberOfScrewHoles];
            var renderObj = transform.GetChild(0);
            render = renderObj.GetComponent<SpriteRenderer>();
            SetBoxColor(UnityEngine.Color.white);
            // Debug.Log("Initialized CrewBox with " + config.numberOfScrewHoles + " screw holes.");
        }

        // Hàm kiểm tra xem các lỗ trong CrewBox có đầy đủ Screw không
        public bool AreAllHolesFilled()
        {
            foreach (var screw in holdScrews)
            {   
                if (screw.IsEmpty())
                {
                    return false; // Nếu có lỗ trống thì trả về false
                }
            }
            return true; // Nếu tất cả lỗ đều đầy
        }
        // khi box đầy thuc hien ham sau 
        protected virtual void BoxFullInvoker(bool isFull)
        {
            Debug.Log("Box full invoker " + gameObject.name  + "\t" + isBoxFull);

            if (isFull)
            {
                //Do Star anim at screw holder
                // closing box 
                // set box active fasle
                Debug.Log("Box full invoker " + gameObject.name );
                BoxQueue.instance.DeactivateAndMoveQueue(this);
            }
        }
        // Hàm di chuyển Screw vào một lỗ trống trong CrewBox
        public void AddScrew(Screw screw)
        {
            // Nếu màu screw không khớp, kết thúc ngay
            if (screw.Color != color)
            {
                Debug.LogWarning("Screw color mismatch!");
                return;
            }

            // Nếu đã biết vị trí trống
            if (nextEmptyIndex >= 0 && nextEmptyIndex < holdScrews.Length)
            {
                if (holdScrews[nextEmptyIndex].IsEmpty())
                {
                    holdScrews[nextEmptyIndex].AddScrew(screw);
                    UpdateNextEmptyIndex(); // Tìm vị trí trống mới
                    return;
                }
            }

            // Tìm lỗ trống lần đầu hoặc khi trạng thái thay đổi
            for (int i = 0; i < holdScrews.Length; i++)
            {
                if (holdScrews[i].IsEmpty())
                {
                    holdScrews[i].AddScrew(screw);
                    nextEmptyIndex = i;
                    UpdateNextEmptyIndex(); // Tìm lỗ trống tiếp theo
                    return;
                }
            }

            // Nếu không có lỗ trống nào
            Debug.LogWarning("All screw holes are filled!" + gameObject.name + " at hold ");
        }

        private void UpdateNextEmptyIndex()
        {
            nextEmptyIndex = -1; // Đặt mặc định không có lỗ trống
            for (int i = 0; i < holdScrews.Length; i++)
            {
                if (holdScrews[i].IsEmpty())
                {
                    nextEmptyIndex = i;
                    break;
                }
            }
            if (nextEmptyIndex == -1)
            {
                Debug.Log("All screw holes are now filled!");
                onScrewBoxFull.Invoke(true); // Gọi sự kiện khi tất cả lỗ đã đầy
            }
        }
        // Hàm để thay đổi màu của CrewBox
        private void SetBoxColor(Color newColor)
        {
            
            // Đặt màu cho box (có thể thêm logic cập nhật màu)
            render.material.color = newColor;
        }
    }
}
