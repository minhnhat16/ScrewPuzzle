using System;
using System.Collections;
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
      [SerializeField] private  List<Screw.Screw> screws; // Mảng các HoldScrew (ô chứa screw)

      public List<Screw.Screw> Screws
      {
         get => screws;
         set => screws = value;
      }


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
      private IEnumerator CheckHoldCoroutine()
      {
          while (true)
          {
              // Kiểm tra nếu tất cả các ô đều đầy
              bool allFull = holdScrews.All(holdScrew => holdScrew != null && !holdScrew.IsEmpty());
      
              if (allFull)
              {
                  Debug.Log("All holdScrews are full!");
                  yield return new WaitForSeconds(2f); // Chờ 2 giây để chắc chắn
                 
                  // Kiểm tra lại sau 2 giây xem có ô nào trống hay không
                  allFull = holdScrews.All(holdScrew => holdScrew != null && !holdScrew.IsEmpty());
                  
                  if (allFull)
                  {
                      // Nếu vẫn đầy, gọi sự kiện
                      onHoldScrewsFull?.Invoke();
                      yield break; // Kết thúc coroutine sau khi gọi sự kiện
                  }
                  else
                  {
                      Debug.Log("HoldScrews cleared during waiting period.");
                  }
              }
              
              // Đợi 0.5 giây trước khi kiểm tra lại
              yield return new WaitForSeconds(0.5f);
          }
      }


      private void CheckIfHoldScrewsFull()
      {
         StartCoroutine(CheckHoldCoroutine());
      }
      public void RemoveScrewOutHold(Screw.Screw screw)
      {
         var hold = holdScrews.Find(h => h.IsContain(screw));
         if (hold == null) return;
         hold.ClearScrewOnHold();
         screws.Remove(screw);
      }

      public void RemoveListScrewOutHold(List<Screw.Screw> screws)
      {
         foreach (var screw in screws)
         {
            RemoveScrewOutHold(screw);
         }
      }
      public List<Screw.Screw> GetAllScrewInHold()
      {
         return (from holdScrew in holdScrews where !holdScrew.IsEmpty() select holdScrew.Screw).ToList();
      } 
      public void AddScrew(Screw.Screw screw)
      {

        if( screw.OnScrewClicked()) return;
         // Tìm các box đang active có cùng màu với screw
         var boxActive = BoxQueue.Instance.screwBoxes
            .Where(b => b.isActiveAndEnabled && b.Color == screw.Color && !b.isMoving && !b.IsBoxFull)
            .ToList();
    
         // Sắp xếp các box theo NextEmptyIndex để tìm box thích hợp
         var boxSameColor = boxActive
            .OrderByDescending(b => b.NextEmptyIndex)
            .FirstOrDefault();
    
         // Nếu tìm thấy box thích hợp và nó đang active
         if (boxSameColor != null && !boxSameColor.IsAddingScrew)
         {
            boxSameColor.AddScrew(screw); // Thêm screw vào box
         }
         else
         {
            // Kiểm tra và tìm holdScrew trống
            var holdScrew = holdScrews.FirstOrDefault(hold => hold.IsEmpty());
    
            // Kiểm tra nếu không còn holdScrew trống
            if (holdScrew == null)
            {
               Debug.LogWarning("No empty holdScrew available to hold the screw.");
               screw.ResetClickedFlag();
               return; // Ngừng thực thi nếu không tìm thấy holdScrew trống
            }

            // Nếu không có box nào phù hợp, thêm vào holdScrew và danh sách screws
            holdScrew.AddScrew(screw, (onMoved) =>
            {
               CheckIfHoldScrewsFull();
            });
            screws.Add(screw); // Lưu screw vào danh sách tạm thời
         }
      }


   }
}
