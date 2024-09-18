using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConfigFile;
using DG.Tweening;
using Enum;
using Ingame.Pools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Ingame
{
    public class BoxQueue : MonoBehaviour
    {
        public static BoxQueue Instance;
         public List<BoxConfigRecord> configRecords = new List<BoxConfigRecord>();
        public Stack ConfigStack = new Stack();
        public ScrewBox[] screwBoxes; // Mảng các box
        public Vector3[] initialPositions; // Lưu vị trí ban đầu của các box
        public float xRightCam;
        public float xLeftCam;
        public int activeBoxCount = 2; // Số box mặc định mở
        [SerializeField] private int spacingBox;
        [SerializeField] private float topAlignSpacing;
        [SerializeField] private List<ScrewBox> boxSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Hủy game object nếu instance khác tồn tại
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Initialize and position the boxes via BoxSlot
            InitAndShuffleColor();
            InitBoxes();
        }

        private void InitAndShuffleColor()
        {
            if (configRecords == null) return;

            // Separate items based on the number of screw holes
            List<BoxConfigRecord> threeHoldList = configRecords.Where(boxConfigRecord => boxConfigRecord.numberOfScrewHoles == 3).ToList();
            List<BoxConfigRecord> twoHoldList = configRecords.Where(boxConfigRecord => boxConfigRecord.numberOfScrewHoles == 2).ToList();
            List<BoxConfigRecord> oneHoldList = configRecords.Where(boxConfigRecord => boxConfigRecord.numberOfScrewHoles == 1).ToList();

            // Shuffle the list of items with 3 screw holes
            threeHoldList = threeHoldList.OrderBy(x => Guid.NewGuid()).ToList();

            // Ensure no adjacent items in threeHoldList have the same color
            for (int i = 1; i < threeHoldList.Count; i++)
            {
                if (threeHoldList[i].boxColor == threeHoldList[i - 1].boxColor)
                {
                    // If two adjacent items have the same color, find a different color to swap
                    int swapIndex = i + 1;
                    while (swapIndex < threeHoldList.Count && threeHoldList[swapIndex].boxColor == threeHoldList[i].boxColor)
                    {
                        swapIndex++;
                    }

                    if (swapIndex < threeHoldList.Count)
                    {
                        // Swap the items
                        (threeHoldList[i], threeHoldList[swapIndex]) = (threeHoldList[swapIndex], threeHoldList[i]);
                    }
                }
            }

            // Combine lists with twoHoldList and oneHoldList always at the end
            List<BoxConfigRecord> finalList = new List<BoxConfigRecord>();
            finalList.AddRange(threeHoldList);
            finalList.AddRange(twoHoldList);
            finalList.AddRange(oneHoldList);

            // Set the final combined list back to ConfigRecords or use it as needed
            configRecords = finalList;
            ConfigStack = new Stack(configRecords);
        }
        private void InitBoxes()
        {
            for (int i = 0; i < boxSlots.Count; i++)
            {
                var config = (BoxConfigRecord)ConfigStack.Pop();
                var isLocked = i < activeBoxCount;

                // Initialize each box slot with its position and locked status
                Vector3 initialPosition = CalculateInitialPosition(i); // Replace with your logic to calculate position
                boxSlots[i].Initialize(initialPosition, isLocked);
            }
        }

        private Vector3 CalculateInitialPosition(int index)
        {
            // Logic to calculate initial position
            float leftBoundary = CameraMain.instance.GetLeft();
            float rightBoundary = CameraMain.instance.GetRight();
            float topBoundary = CameraMain.instance.GetTop() - topAlignSpacing;
            float spacing = (rightBoundary - leftBoundary) / (spacingBox + 1);
            return new Vector3(leftBoundary + (index + 1) * spacing, topBoundary, 0);
        }
       
        public void DeactivateAndMoveQueue(ScrewBox boxSlot)
        {
            Vector3 moveToPosition = boxSlot.initialPosition;
        }
        // Function to update the positions of all active boxes
        private void UpdateBoxPositions()
        {
            // Update the position of all active box slots
            var activeSlots = boxSlots.Where(slot => slot.gameObject.activeSelf).ToArray();
            for (int i = 0; i < activeSlots.Length; i++)
            {
                Vector3 newPosition = CalculateInitialPosition(i);
                // activeSlots[i].MoveToPosition(newPosition);
            }
        }

        private IEnumerator MoveNewBoxToLastBox(ScrewBox newBox, Vector3 toPos, Action<bool> callback = null)
        {
            yield return new WaitForSeconds(1f);
            // Dịch chuyển box mới tới vị trí của box cuối cùng
            newBox.gameObject.SetActive(true);
            var t = newBox.transform.DOMove(toPos, 1f).SetEase(Ease.OutCirc);
            t.OnComplete(() => callback?.Invoke(true));
        }

        // Hàm lấy box đang active cuối cùng
        private ScrewBox GetLastActiveBox()
        {
            for (int i = screwBoxes.Length - 1; i >= 0; i--)
            {
                if (screwBoxes[i].gameObject.activeSelf)
                {
                    return screwBoxes[i]; // Trả về box cuối cùng đang active
                }
            }
            return null; // Không có box nào active
        }

        // Hàm kiểm tra xem có box nào cùng màu với screw không
        public void HasBoxWithSameColor(Screw.Screw screw)
        {
            Debug.Log("Tìm box cùng màu với screw");
            foreach (var box in screwBoxes)
            {
                if (box.gameObject.activeSelf && box.Color == screw.Color)
                {
                    box.AddScrew(screw);
                    Debug.Log("Đã tìm thấy box cùng màu với screw");
                    return;
                }
            }

            // Nếu không tìm thấy box nào cùng màu, thêm screw vào ArrayScrew
            ArrayScrew.instance.AddScrew(screw);
        }

        // Hàm lấy box đang active
        public ScrewBox GetActiveBox()
        {
            return screwBoxes.FirstOrDefault(box => box.gameObject.activeSelf); // Trả về box đang active
        }
    }
}
