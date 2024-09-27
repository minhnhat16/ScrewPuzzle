using System;
using System.Collections.Generic;
using System.Linq;
using Enum;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
   public class ArrayScrew : MonoBehaviour
   {
      public static ArrayScrew instance;
      [SerializeField] private int coutHoldActive;
      [SerializeField]   private SpriteRenderer spriteRenderer;
      public HoldScrew[] holdScrews; // Mảng các HoldScrew (ô chứa screw)
      public UnityEvent onHoldScrewsFull = new (); // Sự kiện khi holdScrews đầy

      private void OnEnable()
      {
         onHoldScrewsFull.AddListener(ScrewFullEvent) ;
      }

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
     
         spriteRenderer = GetComponentInChildren<SpriteRenderer>();
         coutHoldActive = 5;
         HoldAlignment();
      }

      public void SpawnNewHold()
      {
         coutHoldActive++;
         holdScrews[coutHoldActive - 1].gameObject.SetActive(true);
         var spacing = spriteRenderer.bounds.size.x / (holdScrews.Length + 1);
         var startX = spriteRenderer.bounds.min.x;
         for (var i = 0; i < coutHoldActive; i++)
         {
            // Each screw is placed at (startX + spacing * (i + 1)) along the x-axis
            var newPosition = new Vector3(startX + spacing * (i + 1), holdScrews[i].transform.localPosition.y, holdScrews[i].transform.localPosition.z);
            holdScrews[i].transform.localPosition = newPosition;
         }
      }
      private void HoldAlignment()
      {
         if (holdScrews.Length == 0) return;
    
         // Calculate the total width and spacing between screws
         var spacing = spriteRenderer.bounds.size.x / (holdScrews.Length + 1);
         var startX = spriteRenderer.bounds.min.x;
         for (var i = 0; i < coutHoldActive; i++)
         {
            // Each screw is placed at (startX + spacing * (i + 1)) along the x-axis
            var newPosition = new Vector3(startX + spacing * (i + 1), holdScrews[i].transform.localPosition.y, holdScrews[i].transform.localPosition.z);
            holdScrews[i].transform.localPosition = newPosition;
            holdScrews[i].gameObject.SetActive(true);
         }
     
      }
      // Hàm thêm Screw vào một ô trống trong holdScrew
      public bool AddScrew(Screw.Screw screw)
      {
         foreach (var t in holdScrews)
         {
            // Tìm ô trống trong mảng holdScrews
            if (t.Screw == null || t.IsEmpty())
            {
               t.AddScrew(screw); // Thêm screw vào ô trống
               CheckIfHoldScrewsFull(); // Kiểm tra xem đã đầy hết chưa
               return true; // Trả về true nếu thêm thành công
            }
         }

         Debug.LogWarning("All holdScrews are full!");
         return false; // Trả về false nếu không có ô trống
      }

      public List<Screw.Screw> ListScrewSameColor(ColorEnum color, int numberScrewCanTake)
      {
         List<Screw.Screw> newList = new();
         var count = numberScrewCanTake;
         foreach (var hold in holdScrews)
         {
            if (hold.Screw == null || hold.Screw.Color != color) continue;
            newList.Add(hold.Screw);
            count--;
            hold.ClearScrewOnHold();
            if (count > 2) break;
         }
         return newList;
      }

      private void ScrewFullEvent()
      {
         IngameController.Instance.Reset();
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
