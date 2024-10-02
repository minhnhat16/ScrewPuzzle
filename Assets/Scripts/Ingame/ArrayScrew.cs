using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
namespace Ingame
{
   public class ArrayScrew : MonoBehaviour
   {
      public static ArrayScrew Instance;
      [SerializeField] private int coutHoldActive;
      [SerializeField]   private SpriteRenderer spriteRenderer;
      [SerializeField] private  List<HoldScrew> holdScrews; // Mảng các HoldScrew (ô chứa screw)


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

      public List<Screw.Screw> GetAllScrewInHold()
      {
         return (from holdScrew in holdScrews where !holdScrew.IsEmpty() select holdScrew.Screw).ToList();
      } 
      public void AddScrew(Screw.Screw screw)
      {
         var holdScrew = holdScrews.First(hold => hold.IsEmpty());
         holdScrew.AddScrew(screw);
         MatchScrewsToBoxes();  
      }
      private void MatchScrewsToBoxes()
      {
         var listBoxActive = BoxQueue.Instance.screwBoxes;
         // Iterate over each box
         foreach (var box in listBoxActive)
         {
            // Create a list to store screws that match the box color

            var holdsHadScrew = holdScrews.Where(h => h.GetScrew() != null).ToList();
            
            // Iterate over each screw
            var matchedScrews = holdsHadScrew.Select(hold => hold.GetScrew()).Where(screw => screw.Color == box.Color).ToList();

            // Assign the matched screws to the box
            box.AddScrew(matchedScrews);

            // Log the result
           // Debug.Log($"Box with color {box.color} has {box.screws.Count} screws.");
         }
      }
   }
}
