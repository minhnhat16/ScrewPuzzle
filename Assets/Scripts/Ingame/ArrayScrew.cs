using System;
using System.Collections.Generic;
using System.Linq;
using Enum;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ingame
{
   public class ArrayScrew : MonoBehaviour
   {
      public static ArrayScrew Instance;
      [SerializeField] private int coutHoldActive;
      [SerializeField]   private SpriteRenderer spriteRenderer;
     [SerializeField] private  List<HoldScrew> holdScrews; // Mảng các HoldScrew (ô chứa screw)
     [SerializeField] private  List<Screw.Screw> screws; // Mảng các HoldScrew (ô chứa screw)

       public List<Screw.Screw> Screws => screws;

       public UnityEvent onHoldScrewsFull = new (); // Sự kiện khi holdScrews đầy

      private void OnEnable()
      {
         onHoldScrewsFull.AddListener(ScrewFullEvent) ;
      }

      public void Awake()
      {
         if (Instance == null)
         {
            Instance = this;
         }

         Instance = this;
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
         var spacing = spriteRenderer.bounds.size.x / (holdScrews.Count + 1);
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
         if (holdScrews.Count == 0) return;
    
         // Calculate the total width and spacing between screws
         var spacing = spriteRenderer.bounds.size.x / (holdScrews.Count + 1);
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
         // Try to proceed if no overlap
         var boxWithSameColor = BoxQueue.Instance.HasBoxWithSameColor(screw);
         // Reset the flag if the action didn't complete successfully
         if (boxWithSameColor != null  && !boxWithSameColor.IsBoxFull)
         {
            boxWithSameColor.AddScrew(screw);
            Debug.Log("Action completed successfully, flag remains true.");
            return true;
         }
         else
         {
            foreach (var t in holdScrews)
            {
               // Tìm ô trống trong mảng holdScrews
               if (t.Screw != null && !t.IsEmpty()) continue;
               t.AddScrew(screw); // Thêm screw vào ô trống
               screws.Add(screw);
               CheckIfHoldScrewsFull(); // Kiểm tra xem đã đầy hết chưa
               return true; // Trả về true nếu thêm thành công
            }

         }
        
         Debug.LogWarning("All holdScrews are full!");
         StartCoroutine(screw.ResetClickFlagAfterDelay(0.5f));
         return false; // Trả về false nếu không có ô trống
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
