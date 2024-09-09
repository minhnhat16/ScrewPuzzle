using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
   public class ArrayScrew : MonoBehaviour
   {
      public static ArrayScrew instance;
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
      }

      // Hàm thêm Screw vào một ô trống trong holdScrew
      public bool AddScrew(Screw screw)
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
         onHoldScrewsFull.Invoke(); // Gọi sự kiện thua hoặc một hành động khác
      }
   }
}
