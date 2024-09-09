using System;
using System.Security.Cryptography;
using ConfigFile;
using Enum;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ingame
{
    public class ScrewBox : FSMSystem   
    {
        public BoxConfig config; // ScriptableObject chứa cấu hình cho CrewBox
        [SerializeField] private SpriteRenderer render;
        [SerializeField] public HoldScrew[] holdScrews; // Mảng các lỗ Screw
        [SerializeField] public UnityEvent<bool> onScrewBoxFull = new UnityEvent<bool>();
        [SerializeField] private ColorEnum color;
        public ColorEnum Color {get{return color;}set{color=value;}}
        public SpriteRenderer Render {get{return render;}set{render = value;}}
        private void OnEnable()
        {
            onScrewBoxFull.AddListener(BoxFullInvoker);
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
        private void BoxFullInvoker(bool isFull)
        {
            if (isFull)
            {
                //Do Star anim at screw holder
                // closing box 
                // set box active fasle
            }
        }
        // Hàm di chuyển Screw vào một lỗ trống trong CrewBox
        public bool AddScrew(Screw screw)
        {
            Debug.Log("Adding Screww" );
            for (int i = 0; i < holdScrews.Length; i++)
            {
                if (holdScrews[i].IsEmpty() && screw.Color == ColorEnum.White)
                {
                    holdScrews[i].AddScrew(screw); // Đặt Screw vào lỗ trống
                    return true; // Trả về true nếu thành công
                }
            }

            Debug.LogWarning("All screw holes are filled!");
            return false; // Trả về false nếu không có lỗ trống
        }

        // Hàm để thay đổi màu của CrewBox
        public void SetBoxColor(Color color)
        {
            
            // Đặt màu cho box (có thể thêm logic cập nhật màu)
            render.material.color = color;
        }
    }
}
