using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConfigFile;
using DG.Tweening;
using Ingame.Pools;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class BoxQueue : MonoBehaviour
    {
        public static BoxQueue Instance;
        public float xRightCam;
        public float xLeftCam;
        public int activeBoxCount = 2; // Số box mặc định mở
        public Stack ConfigStack = new Stack();
        public BoxConfig boxConfig;
        public List<BoxConfigRecord> configRecords = new List<BoxConfigRecord>();
        public List<ScrewBox> screwBoxes;
        public Stack<ScrewBox> boxesStack;
        [SerializeField] private List<BoxSlot> boxSlots;
        [SerializeField] private float spacingBox = 10;
        [SerializeField] private float topAlignSpacing;

        public UnityEvent<bool> onCompleteClearBoxes = new();


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
            onCompleteClearBoxes = IngameController.Instance.onCompleteLevel;
            Init();
            
        }

        private void Init()
        {
            StartCoroutine(InitCoroutine());
        }

        private IEnumerator InitCoroutine()
        {
            yield return new WaitUntil(() => CameraMain.instance.GetCam() != null);
            InitAndShuffleColor();
            InitBoxes();
            InitBoxSlots(this.boxSlots);
            yield return new WaitForSeconds(0.1f);

            var firstBox = screwBoxes[0];
            var firstSlotPos = boxSlots[0].initialPosition;
        }

        private void InitAndShuffleColor()
        {
            configRecords = boxConfig.GetAllRecord().ToList();

            if (configRecords == null) return;

            // Separate items based on the number of screw holes
            List<BoxConfigRecord> threeHoldList = configRecords
                .Where(record => record.NumberOfScrewHoles == 3).ToList();
            List<BoxConfigRecord> twoHoldList = configRecords.Where(boxConfigRecord => boxConfigRecord.NumberOfScrewHoles == 2).ToList();
            List<BoxConfigRecord> oneHoldList = configRecords.Where(boxConfigRecord => boxConfigRecord.NumberOfScrewHoles== 1).ToList();

            // Shuffle the list of items with 3 screw holes
            //threeHoldList = threeHoldList.OrderBy(x => Guid.NewGuid()).ToList();

            // Ensure no adjacent items in threeHoldList have the same color
            for (int i = 1; i < threeHoldList.Count; i++)
            {
                if (threeHoldList[i].BoxColor == threeHoldList[i - 1].BoxColor)
                {
                    // If two adjacent items have the same color, find a different color to swap
                    int swapIndex = i + 1;
                    while (swapIndex < threeHoldList.Count && threeHoldList[swapIndex].BoxColor == threeHoldList[i].BoxColor)
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
            for (int i = 0; i < configRecords.Count; i++)
            {
                var config = (BoxConfigRecord)ConfigStack.Pop();
                var isLocked = i < activeBoxCount;
                var box = ThreeHoldBoxPool.Instance.pool.list[i];
                box.OnInit(Vector3.left * 10, config, false);
                screwBoxes.Add(box);
                // switch (config.numberOfScrewHoles)
                // {
                //     case 1:
                //         
                //         case 2:
                //             
                //         case 3:
                //         
                // }
              
            
            }

            var reverse = screwBoxes;
            reverse.Reverse();
            boxesStack = new Stack<ScrewBox>(reverse);
        }

        private ScrewBox SpawnBox()
        {
            if (boxesStack.Count == 0) return null;
            Debug.Log("SpawBox");
            var box = boxesStack.Pop();
            return  box;
        }
        private void InitBoxSlots(List<BoxSlot> slots)
        {
            for (int i = 0; i < 2; i++)
            {
                var box = SpawnBox();
                var slot = slots[i];
                var pos = CalculateInitialPosition(i);
                slot.Initialize(pos,false,box);
                StartCoroutine(MoveNewBoxToLastBox(box, slot, (complete) =>
                {
                    FindBoxActiveHaveSameColorWithArray(box);
                }));
            }
        }
        private Vector3 CalculateInitialPosition(int index)
        {
            // Logic to calculate initial position
            if (CameraMain.instance.GetCam() != null)
            {
                float leftBoundary  = CameraMain.instance.GetLeft();
                float rightBoundary = CameraMain.instance.GetRight();
                float topBoundary = CameraMain.instance.GetTop() - topAlignSpacing;
                float spacing = (rightBoundary - leftBoundary ) / (spacingBox);
                Debug.Log($"Left Boundary: {leftBoundary}, Right Boundary: {rightBoundary}, Spacing: {spacing}");
                return new Vector3(-( index * spacing), topBoundary, 0);
            }
            Debug.Log("Camera null  ");
            return Vector3.zero;
        }
        private void CloseAndRemoveBox(ScrewBox screwBox, Action onComplete)
        {
            screwBox.CloseBox((complete) =>
            {
                screwBoxes.Remove(screwBox);
                if (screwBoxes.Count == 0)
                {
                    OnLastBoxClearScrew();
                }
                onComplete?.Invoke();
            });
        }
        private ScrewBox TrySpawnNewBox(BoxSlot currentSlot)
        {
            var newBox = SpawnBox();
            if (newBox == null || currentSlot == null)
            {
                Debug.LogError("Error: new box or slot is null" + newBox + " or "+currentSlot);
                return null;
            }
            return newBox;
        }
        public void DeactivateAndMoveQueue(ScrewBox screwBox)
        {
            var currentSlot = boxSlots.Find((boxSlot) => boxSlot.CheckIsContainingThisBox(screwBox)) as BoxSlot;

            CloseAndRemoveBox(screwBox, () =>
            {
                var newBox = TrySpawnNewBox(currentSlot);
                if (newBox != null)
                {
                    StartCoroutine(MoveAndHandleBox(newBox, currentSlot));
                }
            });
        }

        private IEnumerator MoveAndHandleBox(ScrewBox newBox, BoxSlot currentSlot)
        {
            yield return StartCoroutine(MoveNewBoxToLastBox(newBox, currentSlot, (onCompleteClearBoxes) =>
            {
                List<ScrewBox> listActive = screwBoxes.Where(box => box.isActiveAndEnabled).ToList();
                listActive.Reverse();

                var targetBox = listActive.Count > 0 ? listActive.First() : newBox;
                FindBoxActiveHaveSameColorWithArray(targetBox);
            }));
        }

        private void FindBoxActiveHaveSameColorWithArray(ScrewBox box)
        {
            // Tính số đinh vít có thể thêm vào box
            var totalScrewCanTake = box.holdScrews.Count(hold => hold.IsEmpty());
            // Lấy danh sách các đinh vít có cùng màu với box
            var screwSameColor = ArrayScrew.instance.ListScrewSameColor(box.Color, totalScrewCanTake);
                
            // Kiểm tra danh sách null hoặc rỗng
            if (screwSameColor == null || !screwSameColor.Any()) return;
            // Thêm các đinh vít vào box
            foreach (var screw in screwSameColor)
            {
                box.AddScrew(screw);
                
            }
        }

     
        private void OnLastBoxClearScrew()
        {
            Debug.LogError(onCompleteClearBoxes != null
                ? "OnCompleteClearBoxes is not null and can ivoke"
                : "OnCompleteClearBoxes is null");
            if (onCompleteClearBoxes != null) onCompleteClearBoxes.Invoke(onCompleteClearBoxes != null);
        }
        // ReSharper restore Unity.ExpensiveCode
        private IEnumerator MoveNewBoxToLastBox(ScrewBox newBox, BoxSlot slot, Action<bool> callback = null)
        {
            var toPos = slot.initialPosition;
            yield return new WaitForSeconds(1f);
            newBox.isMoving = true;
            slot.AddBox(newBox); 
            // Dịch chuyển box mới tới vị trí của box cuối cùng
            newBox.gameObject.SetActive(true);
            var t = newBox.transform.DOMove(toPos, 1f).SetEase(Ease.OutCirc);
            t.OnComplete(() =>
            {
                newBox.isMoving = false;
                callback?.Invoke(true);
            });
        }
        // Hàm kiểm tra xem có box nào cùng màu với screw không
        public ScrewBox HasBoxWithSameColor(Screw.Screw screw)
        {
            Debug.Log("Tìm box cùng màu với screw");
            var boxArray =
                screwBoxes.Where(box => box.gameObject.activeSelf && box.Color == screw.Color && !box.isMoving)
                .ToList();
            return boxArray.Count() != 0 ? boxArray.First() : null;
        }

    }
}
