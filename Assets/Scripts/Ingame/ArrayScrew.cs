using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
   public class ArrayScrew : MonoBehaviour
   {
      public static ArrayScrew instance;
      [SerializeField]   private SpriteRenderer spriteRenderer;
      public HoldScrew[] holdScrews; // Mảng các HoldScrew (ô chứa screw)
      public UnityEvent onHoldScrewsFull; // Sự kiện khi holdScrews đầy
      
      public void Awake()
      {
         if (instance == null)
         {
            instance = this;
         }

         instance = this;
      }

      private void Start()
      {
         if (onHoldScrewsFull == null)
            onHoldScrewsFull = new UnityEvent();  // Khởi tạo sự kiện nếu chưa có
         spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            HoldAlignment();
      }

      private void HoldAlignment()
      {
         if (holdScrews.Length == 0) return;
    
         // Calculate the total width and spacing between screws
         float spacing = spriteRenderer.bounds.size.x / (holdScrews.Length + 1);
         float startX = spriteRenderer.bounds.min.x;

         for (int i = 0; i < holdScrews.Length; i++)
         {
            // Each screw is placed at (startX + spacing * (i + 1)) along the x-axis
            Vector3 newPosition = new Vector3(startX + spacing * (i + 1), holdScrews[i].transform.localPosition.y, holdScrews[i].transform.localPosition.z);
            holdScrews[i].transform.localPosition = newPosition;
         }
      }
      // Hàm thêm Screw vào một ô trống trong holdScrew
      public bool AddScrew(Screw.Screw screw)
      {
         for (int i = 0; i < holdScrews.Length; i++)
         {
            // Tìm ô trống trong mảng holdScrews
            if (holdScrews[i] == null || holdScrews[i].IsEmpty())
            {
               holdScrews[i].AddScrew(screw); // Thêm screw vào ô trống
               CheckIfHoldScrewsFull(); // Kiểm tra xem đã đầy hết chưa
               return true; // Trả về true nếu thêm thành công
            }
         }

         Debug.LogWarning("All holdScrews are full!");
         return false; // Trả về false nếu không có ô trống
      }

      // Hàm kiểm tra xem tất cả các ô trong holdScrews đã đầy chưa
      private void CheckIfHoldScrewsFull()
      {
         foreach (var holdScrew in holdScrews)
         {
            if (holdScrew == null || holdScrew.IsEmpty())
            {
               // Có ít nhất một ô trống, không cần xử lý thêm
               return;
            }
         }

         // Nếu tất cả các ô đều đầy, thực hiện invoke
         Debug.Log("All holdScrews are full!");
         onHoldScrewsFull?.Invoke(); // Gọi sự kiện thua hoặc một hành động khác
      }
   }
}
