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
        public List<ScrewBox> screwBoxes = new List<ScrewBox>(); // Initialize to avoid null reference
        public Stack<ScrewBox> boxesStack = new Stack<ScrewBox>();
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
        }

        private void InitAndShuffleColor()
        {
            configRecords = boxConfig.GetAllRecord().ToList();
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
                var box = ThreeHoldBoxPool.Instance.pool.list[i];
                box.OnInit(Vector3.left * 10, config, false);
                screwBoxes.Add(box);
            }

            var reverse = screwBoxes;
            reverse.Reverse();
            boxesStack = new Stack<ScrewBox>(reverse);
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
            for (int i = 0; i < 2; i++)
            {
                var box = SpawnBox();
                var slot = slots[i];
                var pos = CalculateInitialPosition(i);
                slot.Initialize(pos, false, box);
                StartCoroutine(MoveNewBoxToLastBox(box, slot));
            }
        }

        private Vector3 CalculateInitialPosition(int index)
        {
            if (CameraMain.instance.GetCam() != null)
            {
                float leftBoundary = CameraMain.instance.GetLeft();
                float rightBoundary = CameraMain.instance.GetRight();
                float topBoundary = CameraMain.instance.GetTop() - topAlignSpacing;
                float spacing = (rightBoundary - leftBoundary) / (spacingBox);
                Debug.Log($"Left Boundary: {leftBoundary}, Right Boundary: {rightBoundary}, Spacing: {spacing}");
                return new Vector3(-(index * spacing), topBoundary, 0);
            }
            Debug.Log("Camera null");
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
                Debug.LogError("Error: new box or slot is null" + newBox + " or " + currentSlot);
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

        private void OnLastBoxClearScrew()
        {
            Debug.LogError(onCompleteClearBoxes != null
                ? "OnCompleteClearBoxes is not null and can invoke"
                : "OnCompleteClearBoxes is null");
            if (onCompleteClearBoxes != null) onCompleteClearBoxes.Invoke(onCompleteClearBoxes != null);
        }

        private IEnumerator MoveNewBoxToLastBox(ScrewBox newBox, BoxSlot slot, Action<bool> callback = null)
        {
            var toPos = slot.initialPosition;
            yield return new WaitForSeconds(1f);
            newBox.isMoving = true;
            slot.AddBox(newBox);
            newBox.gameObject.SetActive(true);
            var t = newBox.transform.DOMove(toPos, 1f).SetEase(Ease.OutCirc);
            t.OnComplete(() =>
            {
                newBox.isMoving = false;
                callback?.Invoke(true);
            });
        }
        

    }
}
