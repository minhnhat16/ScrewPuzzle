using System.Security.Cryptography;
using UnityEngine;

namespace Ingame
{
    public class ApplyParentLayer : MonoBehaviour
    {
         
           // Hàm apply layer của cha cho tất cả object con
           protected internal void ApplyLayerToChildren(GameObject layerObj)
           {
               int parentLayer = layerObj.layer;
       
               // Duyệt qua tất cả các object con
               foreach (Transform child in layerObj.transform)
               {
                   SetLayerRecursively(child.gameObject, parentLayer);
               }
           }
       
           // Hàm áp dụng layer cho đối tượng và các object con của nó (đệ quy)
           private void SetLayerRecursively(GameObject obj, int newLayer)
           {
               // Kiểm tra nếu đối tượng có tên là "Screw" hoặc có gắn script Screw
               if (obj.name == "Screw" || obj.GetComponent<Screw.Screw>() != null)
               {
                   return; // Bỏ qua đối tượng này
               }

               // Đặt layer mới cho đối tượng hiện tại
               obj.layer = newLayer;

               // Đệ quy để áp dụng layer cho tất cả object con của nó
               foreach (Transform child in obj.transform)
               {
                   SetLayerRecursively(child.gameObject, newLayer);
               }
           }
    }
}
