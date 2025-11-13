using ConfigFile;
using DG.Tweening;
using Ingame.Pools;
using Ingame.Screw;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Ingame
{
    public class BoxQueue : MonoBehaviour,IResetable
    {
        public static BoxQueue Instance;


        [SerializeField] private bool movingBox; public float xRightCam;
        public float xLeftCam;
        public int activeBoxCount = 5; // Số box mặc định mở
        [SerializeField] private float spacingBox = 10;
        [SerializeField] private float topAlignSpacing;

        public Stack ConfigStack = new Stack();
        public BoxConfig boxConfig;
        public List<BoxConfigRecord> configRecords = new List<BoxConfigRecord>();
        public List<ScrewBox> screwBoxes = new List<ScrewBox>(); // Initialize to avoid null reference
        public Stack<ScrewBox> boxesStack = new Stack<ScrewBox>();


        public List<Screw.Screw> hidingScrews;
        [SerializeField] private List<BoxSlot> boxSlots;
        public UnityEvent<bool> onCompleteClearBoxes = new();
        public UnityEvent<Screw.Screw> onDeletOneScrew = new();
        private Coroutine alignCoroutine;

        public bool MovingBox { get => movingBox; set => movingBox = value; }
        private void OnEnable()
        {
            onDeletOneScrew.AddListener(RecalculatingBox);
        }
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
            hidingScrews = new List<Screw.Screw>();
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.name.CompareTo("LevelMaker") != 0)
            {
                onCompleteClearBoxes = IngameController.Instance.onCompleteLevel;
                topAlignSpacing = CameraMain.instance.GetTop() + 7.2f;
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
                box.SetActive(false);
                screwBoxes.Remove(box);
            }
            boxesStack.Clear();
        }
        private void InitAndShuffleColor()
        {
            //if (configRecords == null) return;

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

            int totalStar = threeHoldList.Count * 3 + twoHoldList.Count * 2 + oneHoldList.Count;
            IngameController.Instance.TotalStarInLevel = totalStar;
            configRecords = finalList;
            ConfigStack = new Stack(configRecords);
        }

        private void InitBoxes()
        {
            for (int i = 0; i < configRecords.Count; i++)
            {
                var config = (BoxConfigRecord)ConfigStack.Pop();
                var isLocked = i < activeBoxCount;
                var color = config.BoxColor;
                ScrewBox box;
                var leftCamPos = CameraMain.instance.GetLeft();
                switch (config.NumberOfScrewHoles)
                {
                    case 1:
                        box = OneHoldBoxPool.Instance.pool.SpawnNonGravity();
                        box.OnInit(Vector2.one * leftCamPos, color, false, config.NumberOfScrewHoles);
                        box.SetActive(false);
                        screwBoxes.Add(box);
                        break;
                    case 2:
                        box = TwoHoldBoxPool.Instance.pool.SpawnNonGravity();
                        box.OnInit(Vector3.left * leftCamPos, color, false, config.NumberOfScrewHoles);
                        box.SetActive(false);
                        screwBoxes.Add(box);
                        break;
                    case 3:
                        box = ThreeHoldBoxPool.Instance.pool.SpawnNonGravity();
                        box.OnInit(Vector3.left * leftCamPos, color, false, config.NumberOfScrewHoles);
                        box.SetActive(false);
                        screwBoxes.Add(box);
                        break;


                }

            }

            IngameController.Instance.ShuffleList(screwBoxes);
            var oderByHoldNumber = screwBoxes.OrderBy(box => box.holdScrews.Count);
            boxesStack = new Stack<ScrewBox>(oderByHoldNumber);
        }
        private void RecalculatingBox(Screw.Screw clearedScrew)
        {
            var listBox = boxesStack.ToList();
            var boxSameColor = listBox.FirstOrDefault((box) => box.Color == clearedScrew.Color);
            if (boxSameColor == null) return;
            var idCrBox = listBox.IndexOf(boxSameColor);
            var color = boxSameColor.Color;
            var totalHold = boxSameColor.TotalHold;
            switch (totalHold)
            {
                case 1:
                    listBox.RemoveAt(idCrBox);
                    break;
                case 2:
                    var boxOneHold = OneHoldBoxPool.Instance.pool.SpawnNonGravity();
                    boxOneHold.OnInit(Vector3.left * 5, color, false, --totalHold);
                    boxOneHold.gameObject.SetActive(false);
                    listBox.RemoveAt(idCrBox);
                    listBox[idCrBox] = boxOneHold;
                    break;
                case 3:
                    var boxTwoHold = TwoHoldBoxPool.Instance.pool.SpawnNonGravity();
                    boxTwoHold.OnInit(Vector3.left * 5, color, false, --totalHold);
                    boxTwoHold.gameObject.SetActive(false);
                    listBox.RemoveAt(idCrBox);
                    listBox[idCrBox] = boxTwoHold;
                    break;
            }
            IngameController.Instance.TotalStarInLevel--;
            IngameController.Instance.ShuffleList(screwBoxes);
            var oderByHoldNumber = listBox.OrderBy(box => box.holdScrews.Count);
            boxesStack = new Stack<ScrewBox>(oderByHoldNumber);
            screwBoxes = oderByHoldNumber.ToList();
        }
        private ScrewBox SpawnBox()
        {
            if (boxesStack.Count == 0) return null;
            //Debug.Log("SpawnBox");
            var box = boxesStack.Pop();
            return box;
        }

        private void InitBoxSlots(List<BoxSlot> slots)
        {
            for (int i = 0; i < 4; i++)
            {
                var slot = slots[i];
                var pos = CalculateCenteredPosition(i, 4); // Tính vị trí ban đầu dựa trên số slot

                if (i < activeBoxCount)
                {
                    var box = SpawnBox();
                    slot.Initialize(pos, false, box);
                    box.SetActive(true);
                    StartCoroutine(MoveToSlot(box, slot));
                }
                else
                {
                    var box = ThreeHoldBoxPool.Instance.Spawn();
                    box.SetIsLocked(false);
                    box.SetActive(true);
                    slot.Initialize(pos, true, box);
                    StartCoroutine(MoveToSlot(box, slot));

                }
            }

            StartAligningSlots(slots);
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
            movingBox = true;
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
            newBox.gameObject.SetActive(true);
             newBox.Position = currentSlot.transform.position + new Vector3(CameraMain.instance.GetLeft() - 1, 0);

            newBox.ClearScrewOnHold();

            var freeSlots = 3;
            Debug.Log("free slot " + freeSlots);
            if (freeSlots <= 0) return newBox;

            var pendingForColor = hidingScrews
                .Where(s => s != null && s.Color == newBox.Color)
                .Take(freeSlots)
                .ToList();
            Debug.Log("Pending for colors " + pendingForColor.Count);
            if (pendingForColor.Count == 0) return newBox;

            int countScrew = newBox.holdScrews.Where(s => s.Screw != null).Count();

            return newBox;
        }
        public void DeactivateAndMoveQueue(ScrewBox screwBox)
        {
            var currentSlot = boxSlots.Find((boxSlot) => boxSlot.CheckIsContainingThisBox(screwBox)) as BoxSlot;

            CloseAndRemoveBox(screwBox, () =>
            {
                var newBox = TrySpawnNewBox(currentSlot);
                AddHidingScrewToBox(newBox);
                if (newBox != null)
                {
                    StartCoroutine(MoveAndHandleBox(newBox, currentSlot));
                }
            });
        }

        private IEnumerator MoveAndHandleBox(ScrewBox newBox, BoxSlot currentSlot)
        {
            var isMoveBoxDone = false;
            MovingBox = true;
            yield return StartCoroutine(MoveToSlot(newBox, currentSlot, (boxDone) =>
            {
                isMoveBoxDone = boxDone;
            }));
            yield return new WaitUntil(() => isMoveBoxDone == true);

        }
        public void AddScrewToBox(Screw.Screw screw, ScrewBox box, out bool canAdd)
        {
            canAdd = false;
            if (!box.IsAddingScrew)
            {

                Debug.Log("Adding screw to box");
                box.AddScrew(screw, out canAdd);
                var screwMng = LevelManager.Instance.ScrewManager;
                screwMng.RemoveScrew(screw); // Xóa screw khỏi danh sách quản lý
            }
            else
            {
                Debug.LogWarning($"Box {box.name} is currently adding a screw.");
            }
        }
        public void AddMultipleScrew(List<Screw.Screw> screwList, ScrewBox box, bool isTele)
        {
            if (box.IsAddingScrew) return;
            box.AddScrew(screwList, isTele);
        }
        public ScrewBox FindSuitableBox(Screw.Screw screw, bool allowFallbackToInactive = true)
        {
            if (screw == null) return null;

            // Common base predicate
            bool BasePredicate(ScrewBox box) =>
                box != null &&
                box.Color == screw.Color &&
                !box.isMoving &&
                !box.IsBoxFull;

            // 1) Try active boxes first
            var activeCandidate = screwBoxes
                .Where(box => BasePredicate(box) && box.gameObject != null && box.gameObject.activeInHierarchy)
                .OrderByDescending(box => box.isActiveAndEnabled)
                .FirstOrDefault();

            if (activeCandidate != null)
                return activeCandidate;

            // 2) Fallback to inactive boxes if allowed
            if (!allowFallbackToInactive)
                return null;

            var inactiveCandidate = screwBoxes
                .Where(box => BasePredicate(box) && (box.gameObject == null || !box.gameObject.activeInHierarchy))
                .OrderByDescending(box => box.NextEmptyIndex)
                .FirstOrDefault();

            return inactiveCandidate;
        }

        public bool TryMoveScrewsGroupedByColor(List<Screw.Screw> screws, bool includeInactive = false)
        {
            if (screws == null || screws.Count == 0) return false;

            // Normalize and dedupe input
            var distinct = screws.Where(s => s != null).Distinct().ToList();
            if (distinct.Count == 0) return false;

            // Group screws by color
            var groups = distinct.GroupBy(s => s.Color);

            int totalMoved = 0;

            foreach (var group in groups)
            {
                var color = group.Key;
                var groupList = group.ToList();

                // Find a suitable box for this color
                var box = FindSuitableBox(groupList[0], includeInactive);
                if (box == null)
                {
                    AddToHidingList(groupList);
                    Debug.Log($"BoxQueue: No suitable box found for color {color}. Skipping {groupList.Count} screws.");
                    continue;
                }

                var freeHold = box.holdScrews.FindAll(h => h.IsEmpty());
                int freeSlots = freeHold.Count;
                var toMove = groupList;
                if (freeSlots > 0 && groupList.Count > freeSlots)
                {
                    toMove = groupList.Take(freeSlots).ToList();
                }
                else if (freeSlots <= 0)
                {
                    GameUtils.LogAndSelect($"Box {box.name} is full for color {color}. Skipping.", box.gameObject);
                    continue;
                }

                // Move the screws into the box
                AddMultipleScrew(toMove, box, false);
                totalMoved += toMove.Count;

                // If some screws in the group were not moved (box filled), they remain in 'distinct' — caller can handle fallback
                if (toMove.Count < groupList.Count)
                {
                    Debug.Log($"Moved {toMove.Count} of {groupList.Count} screws into box {box.name} for color {color} (box filled).");
                }
            }

            Debug.Log($"TryMoveScrewsGroupedByColor: moved {totalMoved} screws grouped by color.");
            return totalMoved > 0;
        }
        private void OnLastBoxClearScrew()
        {

            bool isComplete = onCompleteClearBoxes != null && IngameController.Instance.IsGameOver;
            if (onCompleteClearBoxes != null) onCompleteClearBoxes.Invoke(isComplete);
        }

        private IEnumerator MoveToSlot(ScrewBox newBox, BoxSlot slot, Action<bool> callback = null)
        {
            yield return new WaitForSeconds(1f);
            var toPos = slot.transform.position;

            // Safety
            if (newBox == null)
            {
                Debug.LogWarning("MoveToSlot called with null newBox");
                callback?.Invoke(false);
                yield break;
            }
            newBox.Position = toPos + new Vector3(CameraMain.instance.GetLeft() - 1, 0);
            newBox.isMoving = true;
            slot.AddBox(newBox);

            // Candidates for this box color

            var t = newBox.transform.DOMove(toPos, 1f).SetEase(Ease.OutCirc);
            t.OnStart(() =>
            {
                AddHidingScrewToBox(newBox);
            });
            t.OnUpdate(() => { });
            t.OnComplete(() =>
            {
                newBox.isMoving = MovingBox = false;
                newBox.FindScrew();
                callback?.Invoke(true);
            });
        }
        public void AddHidingScrewToBox(ScrewBox newBox)
        {
            List<Screw.Screw> hinding = hidingScrews
                .Where(h => h != null && h.Color == newBox.Color)
                .Take(3)
                .ToList();

            if (hinding.Count > 0)
            {
                AddMultipleScrew(hinding, newBox, true);
            }
            hinding.ForEach(h => { hidingScrews.Remove(h); });
        }

        public void StartAligningSlots(List<BoxSlot> slots)
        {
            if (alignCoroutine != null) StopCoroutine(alignCoroutine);
            alignCoroutine = StartCoroutine(AlignSlots(slots));
        }

        private IEnumerator AlignSlots(List<BoxSlot> slots)
        {
            // Filter active slots only once before alignment
            var activeSlots = slots.Where(slot => slot.gameObject.activeSelf).ToList();
            if (activeSlots.Count == 0) yield break; // Exit if no active slots

            bool isAlignmentComplete = false;

            while (!isAlignmentComplete)
            {
                isAlignmentComplete = true; // Assume alignment will complete this frame

                // Update positions for active slots
                for (int i = 0; i < activeSlots.Count; i++)
                {
                    var slot = activeSlots[i];
                    var targetPosition = CalculateCenteredPosition(i, activeSlots.Count);
                    var currentPosition = slot.transform.position;

                    // Smoothly move the slot to the target position
                    var newPosition = Vector3.Lerp(currentPosition, targetPosition, 0.1f);
                    slot.transform.position = newPosition;

                    // Update screwBox position if applicable
                    if (slot.screwBox != null && !slot.screwBox.isMoving && !slot.screwBox.IsBoxFull)
                    {
                        slot.screwBox.Position = newPosition;
                    }

                    // Check if the slot is close enough to its target position
                    if (Vector3.Distance(newPosition, targetPosition) > 0.01f)
                    {
                        isAlignmentComplete = false; // Continue aligning if not yet completed
                    }
                }

                // Wait for the next frame
                yield return null;
            }

            // Reset coroutine reference when finished
            alignCoroutine = null;
        }
        public void AddToHidingList(List<Screw.Screw> screws)
        {
            if (screws == null || screws.Count == 0) return;

            foreach (var screwItem in screws)
            {
                hidingScrews.Add(screwItem);
                if (screwItem != null && screwItem.gameObject != null)
                    screwItem.gameObject.SetActive(false);
            }

            // Summary: total count
            int total = hidingScrews.Count;

            // Breakdown by color (safely handle null entries)
            var breakdown = hidingScrews
                .Where(s => s != null)
                .GroupBy(s => s.Color)
                .Select(g => $"{g.Key}:{g.Count()}")
                .ToList();

            // Full list with index -> color (or "null")
            var indexed = hidingScrews
                .Select((s, i) => $"{i}:{(s == null ? "null" : s.Color.ToString())}")
                .ToList();

            Debug.Log($"[HidingScrews] Total={total}; Breakdown=[{string.Join(", ", breakdown)}]; Items=[{string.Join(", ", indexed)}]");
        }
        public void RemoveToHidingList(List<Screw.Screw> screws)
        {
            foreach (var screwItem in screws)
            {
                hidingScrews.Remove(screwItem);
            }
        }
        public void AddNewBoxSlot()
        {
            if (boxSlots.All(slot => slot.gameObject.activeSelf)) return;
            var newBoxSlot = boxSlots.First(slot => !slot.gameObject.activeSelf);
            newBoxSlot.gameObject.SetActive(true);
            var newBox = TrySpawnNewBox(newBoxSlot);
            if (newBox != null)
            {
                StartCoroutine(MoveAndHandleBox(newBox, newBoxSlot));
                StartAligningSlots(boxSlots);
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
        public void OnReset()
        {
            ThreeHoldBoxPool.Instance.pool.DeSpawnAll();
            TwoHoldBoxPool.Instance.pool.DeSpawnAll();
            OneHoldBoxPool.Instance.pool.DeSpawnAll();
            foreach (var box in screwBoxes)
            {
                box.Reset();
            }
        }
    }
}
