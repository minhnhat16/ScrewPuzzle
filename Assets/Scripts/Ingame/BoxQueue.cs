using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConfigFile;
using DG.Tweening;
using Ingame.Pools;
using Managers;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Ingame
{
    public class BoxQueue : MonoBehaviour
    {
        public static BoxQueue Instance;
        public float xRightCam;
        public float xLeftCam;
        public int activeBoxCount = 2; // Số box mặc định mở
        [SerializeField] private float spacingBox = 10;
        [SerializeField] private float topAlignSpacing;

        public Stack ConfigStack = new Stack();
        public BoxConfig boxConfig;
        public List<BoxConfigRecord> configRecords = new List<BoxConfigRecord>();
        public List<ScrewBox> screwBoxes = new List<ScrewBox>(); // Initialize to avoid null reference
        public Stack<ScrewBox> boxesStack = new Stack<ScrewBox>();
        [SerializeField] private List<BoxSlot> boxSlots;
        public UnityEvent<bool> onCompleteClearBoxes = new();
        private Coroutine alignCoroutine;
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
            var currentScene = SceneManager.GetActiveScene();
          if (currentScene.name.CompareTo("LevelMaker") != 0)
            {
                onCompleteClearBoxes = IngameController.Instance.onCompleteLevel;
            }
            
        }

        public void Init()
        {
            StartCoroutine(InitCoroutine());
        }
    
        private IEnumerator InitCoroutine()
        {
            yield return new WaitUntil(() => CameraMain.instance.GetCam() != null);
            yield return new WaitUntil(() => configRecords.Count != 0);
            InitAndShuffleColor();
            InitBoxes();
            InitBoxSlots(this.boxSlots);
            yield return new WaitForSeconds(0.1f);
        }

        public void LoadBoxConfigRecord(BoxConfig boxConfig)
        {
            this.boxConfig = boxConfig;
            Debug.Log("Load box config record" + boxConfig == null);
            var allRecord = boxConfig.GetAllRecord();
            configRecords.AddRange(allRecord);
        }

        public void ClearConfigRecords()
        {
            configRecords.Clear();
        }

        public void ClearCurrentBoxes()
        {
            foreach (var box in screwBoxes.ToList())
            {
                box.SetBoxActive(false);
                screwBoxes.Remove(box);
            }
            boxesStack.Clear();
        }
        private void InitAndShuffleColor()
        {   
            if (configRecords == null) return;

            List<BoxConfigRecord> threeHoldList = configRecords.Where(record => record.NumberOfScrewHoles == 3).ToList();
            List<BoxConfigRecord> twoHoldList = configRecords.Where(boxConfigRecord => boxConfigRecord.NumberOfScrewHoles == 2).ToList();
            List<BoxConfigRecord> oneHoldList = configRecords.Where(boxConfigRecord => boxConfigRecord.NumberOfScrewHoles == 1).ToList();

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

            configRecords = finalList;
            ConfigStack = new Stack(configRecords);
        }

        private void InitBoxes()
        {
            for (int i = 0; i < configRecords.Count; i++)
            {
                var config = (BoxConfigRecord)ConfigStack.Pop();
                var isLocked = i < activeBoxCount;
                switch (config.NumberOfScrewHoles)
                {
                    case 1:
                        var boxOneHold = OneHoldBoxPool.Instance.pool.SpawnNonGravity();
                        boxOneHold.OnInit(Vector3.left * 5, config, false);
                        boxOneHold.gameObject.SetActive(false);
                        screwBoxes.Add(boxOneHold);
                        break;
                    case 2:
                        var  boxTwoHold = TwoHoldBoxPool.Instance.pool.SpawnNonGravity();
                        boxTwoHold.OnInit(Vector3.left * 5, config, false);
                        boxTwoHold.gameObject.SetActive(false);

                         screwBoxes.Add(boxTwoHold);
                        break;
                    case 3:
                        var boxThreeHold = ThreeHoldBoxPool.Instance.pool.SpawnNonGravity();
                        boxThreeHold.OnInit(Vector3.left * 5, config, false);
                        boxThreeHold.gameObject.SetActive(false);
                        screwBoxes.Add(boxThreeHold);
                        break;
                }   
            }

            var oderByHoldNumber = screwBoxes.OrderBy(box=> box.holdScrews.Count);
            boxesStack = new Stack<ScrewBox>(oderByHoldNumber);
        }

        private ScrewBox SpawnBox()
        {
            if (boxesStack.Count == 0) return null;
            Debug.Log("SpawnBox");
            var box = boxesStack.Pop();
            return box;
        }

        private void InitBoxSlots(List<BoxSlot> slots)
        {
            for (int i = 0; i < 4; i++)
            {
                var slot = slots[i];
                var pos = CalculateCenteredPosition(i, 4); // Tính vị trí ban đầu dựa trên số slot

                if (i < 2)
                {
                    var box = SpawnBox();
                    slot.Initialize(pos, false, box);
                    StartCoroutine(MoveNewBoxToLastBox(box, slot));
                }
                else
                {
                    slot.Initialize(pos, false, null);
                    slot.gameObject.SetActive(false);
                }
            }

            // Bắt đầu coroutine để căn chỉnh slot
            if (alignCoroutine == null)
            {
                alignCoroutine = StartCoroutine(AlignSlots(slots));
            }
        }


        private Vector3 CalculateCenteredPosition(int index, int totalActiveSlots)
        {
            if (CameraMain.instance.GetCam() != null)
            {
                float leftBoundary = CameraMain.instance.GetLeft();
                float rightBoundary = CameraMain.instance.GetRight();
                float topBoundary = CameraMain.instance.GetTop() - topAlignSpacing;

                // Calculate the total width available between the left and right boundaries
                float width = rightBoundary - leftBoundary;

                // Define a minimum spacing between the slots
                float minSpacing = 1.5f; // Adjust this value to control how far apart boxes should be

                // Calculate the maximum possible spacing based on the number of slots
                float spacing = Mathf.Max(minSpacing, width / (totalActiveSlots + 1)); // Ensure spacing is never less than minSpacing

                // Calculate the X position of the current slot
                float xPosition = leftBoundary + (spacing * (index + 1)); // Index starts at 0, add 1 to offset the first slot

                return new Vector3(xPosition, topBoundary, 0);
            }

            Debug.Log("Camera is null");
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
                //Debug.LogError("Error: new box or slot is null" + newBox + " or " + currentSlot);
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
            var isMoveBoxDone = false;
            yield return StartCoroutine(MoveNewBoxToLastBox(newBox, currentSlot, (boxDone) =>
            {
                isMoveBoxDone = boxDone;
            }));
            yield return new WaitUntil(() => isMoveBoxDone == true);
            
        }
        public void AddScrewToBox(Screw.Screw screw, ScrewBox box)
        {
            if (!box.IsAddingScrew)
            {
                box.AddScrew(screw);
                var screwMng = LevelManager.Instance.ScrewManager;
                screwMng.RemoveScrew(screw); // Xóa screw khỏi danh sách quản lý
            }
            else
            {
                Debug.LogWarning($"Box {box.name} is currently adding a screw.");
            }
        }

        public ScrewBox FindSuitableBox(Screw.Screw screw)
        {
            return BoxQueue.Instance.screwBoxes
                .Where(box => box.isActiveAndEnabled &&
                              box.Color == screw.Color &&
                              !box.isMoving &&
                              !box.IsBoxFull)
                .OrderByDescending(box => box.NextEmptyIndex) // Ưu tiên box có NextEmptyIndex cao nhất
                .FirstOrDefault();
        }

        private void OnLastBoxClearScrew()
        {
            Debug.LogError(onCompleteClearBoxes != null
                ? "OnCompleteClearBoxes is not null and can invoke"
                : "OnCompleteClearBoxes is null");
            bool isComplete = onCompleteClearBoxes != null && IngameController.Instance.IsGameOver;
            if (onCompleteClearBoxes != null) onCompleteClearBoxes.Invoke(isComplete);
        }

        private IEnumerator MoveNewBoxToLastBox(ScrewBox newBox, BoxSlot slot, Action<bool> callback = null)
        {
            yield return new WaitForSeconds(1f);
            var toPos = slot.transform.position;
            newBox.Position = toPos + new Vector3(CameraMain.instance.GetLeft() - 1, 0);
            newBox.isMoving = true;
            slot.AddBox(newBox);
            newBox.gameObject.SetActive(true);
            var t = newBox.transform.DOMove(toPos, 1f).SetEase(Ease.OutCirc);
            t.OnComplete(() =>
            {
                newBox.isMoving = false;
                newBox.FindScrew();
                callback?.Invoke(true);
            });
        }
        private IEnumerator AlignSlots(List<BoxSlot> slots)
        {
            while (true)
            {
                // Lọc các slot đang active
                var activeSlots = slots.Where(slot => slot.gameObject.activeSelf).ToList();

                if (activeSlots.Count > 0)
                {
                    // Cập nhật lại vị trí cho tất cả các slot đang active
                    for (int i = 0; i < activeSlots.Count; i++)
                    {
                        var slot = activeSlots[i];
                        var newPosition = CalculateCenteredPosition(i, activeSlots.Count);
                        var pos = slot.transform.position = Vector3.Lerp(slot.transform.position, newPosition, 0.1f); // Smoothly transition to the new position
                        if (slot.screwBox != null && !slot.screwBox.isMoving&& !slot.screwBox.IsBoxFull) slot.screwBox.Position = pos;
                    }
                }
                yield return null; // Chờ một khung hình trước khi kiểm tra lại
            }
        }
        public void AddNewBoxSlot()
        {
            var newBoxSlot = boxSlots.First(slot => !slot.gameObject.activeSelf); 
            newBoxSlot.gameObject.SetActive(true);
            var newBox = TrySpawnNewBox(newBoxSlot);
            if (newBox != null)
            {
                StartCoroutine(MoveAndHandleBox(newBox, newBoxSlot));
            }
        }
        public void ReturnBoxToPool(ScrewBox box)
        {
            int totalhold = box.holdScrews.Count;
            switch (totalhold)
            {
                case 1:
                    OneHoldBoxPool.Instance.pool.ReturnToPool(box as BoxOneHold);
                    break;
                case 2:
                    TwoHoldBoxPool.Instance.pool.ReturnToPool(box as BoxTwoHold);
                    break;
                case 3:
                    ThreeHoldBoxPool.Instance.pool.ReturnToPool(box as BoxThreeHold);
                    break;
            }
        }
        public void Reset()
        {
            foreach (var box in screwBoxes)
            {
                box.Reset();
            }
        }
    }
}
