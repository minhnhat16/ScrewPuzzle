using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Ingame
{
    public class BoxQueue : MonoBehaviour
    {
        public static BoxQueue instance;
        public ScrewBox[] screwBoxes; // Mảng các box
        public Vector3[] initialPositions; // Lưu vị trí ban đầu của các box
        public int activeBoxCount = 2; // Số box mặc định mở

        private void Awake()
        {
            if (instance != null) instance = this;
            instance = this;
        }

        private void Start()
        {
            // Khởi tạo vị trí ban đầu của các box
            initialPositions = new Vector3[screwBoxes.Length];
            for (int i = 0; i < screwBoxes.Length; i++)
            {
                initialPositions[i] = screwBoxes[i].transform.position;

                // Set trạng thái active cho 2 box đầu tiên, các box sau thì tắt
                if (i > activeBoxCount -1 )
                {
                    screwBoxes[i].gameObject.SetActive(true);
                }
                else
                {
                    screwBoxes[i].gameObject.SetActive(false);
                }
            }
        }

        // Hàm tắt box tại index và dịch chuyển hàng đợi
        internal void DeactivateAndMoveQueue(ScrewBox box)
        {
            int filledBoxIndex = Array.IndexOf(screwBoxes, box);
            if (filledBoxIndex == -1)
            {
                Debug.Log(" screw boxes not  contained "+ box.name);
                return;
            }
            Debug.Log(" screw boxes contained "+ box.name);
            // Tắt box đã đầy
            screwBoxes[filledBoxIndex].gameObject.SetActive(false);

            // Dịch chuyển các box tiếp theo vào vị trí box đã đầy
            for (int i = filledBoxIndex + 1; i < screwBoxes.Length; i++)
            {
                if (!screwBoxes[i].gameObject.activeSelf)
                {
                    // Kích hoạt box tiếp theo
                    screwBoxes[i].gameObject.SetActive(true);

                    // Di chuyển box này vào vị trí của box đã đầy
                    screwBoxes[i].transform.DOMove(initialPositions[filledBoxIndex], 0.5f)
                        .OnComplete(() => Debug.Log("Box di chuyển hoàn tất!"));

                    break;
                }
            }
        }

        // Hàm lấy box đang active
        public ScrewBox GetActiveBox()
        {
            foreach (var box in screwBoxes)
            {
                if (box.gameObject.activeSelf)
                {
                    return box; // Trả về box đang active
                }
            }

            return null; // Không có box nào active
        }
        // Hàm kiểm tra xem có box nào cùng màu với screw không
        public void HasBoxWithSameColor(Screw screw)
        {
            Debug.Log("Tìm box cungf màu với screw");
            foreach (var box in screwBoxes)
            {
                if (box.gameObject.activeSelf && box.Color == screw.Color)
                {
                    box.AddScrew(screw);
                    Debug.Log("Tìm box cungf màu với screw");
                    return;
                }
                
            }
            ArrayScrew.instance.AddScrew(screw);
        }
    }
    }

