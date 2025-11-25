using ConfigFile;
using DG.Tweening;
using Enums;
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

namespace Ingame
{
    public class BoxQueue : SingletonMono<BoxQueue>, IResetable
    {

        [Header("Box Settings")]
        public bool hasSpecialBox;

        [SerializeField] private bool movingBox;
        public float xRightCam;
        public float xLeftCam;
        public int activeBoxCount = 5;
        [SerializeField] private float spacingBox = 10;
        [SerializeField] private float topAlignSpacing;
        public float leftBound;

        [Header("Config / Data")]
        public Stack<BoxConfigRecord> ConfigStack = new Stack<BoxConfigRecord>();
        public BoxConfig boxConfig;
        public List<BoxConfigRecord> configRecords = new List<BoxConfigRecord>();
        public List<Box> screwBoxes = new List<Box>();
        public Stack<Box> boxesStack = new Stack<Box>();

        [Header("Runtime")]
        public List<Screw.Screw> hidingScrews;
        [SerializeField] private List<BoxSlot> boxSlots;
        [SerializeField] private List<Box> rainbowBoxes;
        private Coroutine alignCoroutine;

        [Header("Events")]
        public UnityEvent<bool> onCompleteClearBoxes = new();
        public UnityEvent<Screw.Screw> onDeletOneScrew = new();
        public UnityEvent<Box> boxFullEvent = new();

        public bool MovingBox
        {
            get => movingBox;
            set => movingBox = value;
        }

        #region Unity
        private void OnEnable()
        {
            if (onDeletOneScrew != null)
                onDeletOneScrew.AddListener(RecalculatingBox);
        }

        private void OnDisable()
        {
            if (onDeletOneScrew != null)
                onDeletOneScrew.RemoveListener(RecalculatingBox);
        }

        private void Start()
        {
            hidingScrews = new List<Screw.Screw>();

            var currentScene = SceneManager.GetActiveScene();
            if (!currentScene.name.Equals("LevelMaker"))
            {
                onCompleteClearBoxes = IngameController.ins.onCompleteLevel;
            }
        }

        #endregion

        #region Init

        public void Init()
        {
            StartCoroutine(InitCoroutine());
            Debug.Log("top align spacing " + topAlignSpacing);
        }

        private IEnumerator InitCoroutine()
        {
            yield return new WaitUntil(() => CameraMain.instance.GetCam() != null);
            yield return new WaitUntil(() => configRecords.Count != 0);

            InitAndShuffleColor();
            InitBoxes();
            InitBoxSlots(boxSlots);


            yield return new WaitForSeconds(0.1f);
        }

        public void InitRainbowBoxes(int count = 1)
        {
            rainbowBoxes.Clear();

            for (int i = 0; i < count; i++)
            {
                var box = ThreeHoldBoxPool.Instance.pool.SpawnNonGravity();
                box.SetActive(true);
                box.SetIsLocked(false);
                box.SetBoxColor(ColorEnum.Rainbow);

                // Vị trí khác nhau (gom ở phía trên)
                var pos = CameraMain.instance.GetTopRight();
                pos.x -= 2f + (i * 2.5f);
                pos.y -= 5f;
                box.transform.position = pos;

                box.isMoving = false;
                rainbowBoxes.Add(box);
            }
        }


        public void LoadBoxConfigRecord(BoxConfig boxConfig)
        {
            this.boxConfig = boxConfig;
            Debug.Log("Load box config record, null? " + (boxConfig == null));

            var allRecord = boxConfig.GetAllRecord();
            configRecords.AddRange(allRecord);
        }

        public void ClearConfigRecords()
        {
            configRecords.Clear();
            ConfigStack.Clear();
        }

        public void ClearCurrentBoxes()
        {
            foreach (var box in screwBoxes.ToList())
            {
                if (box != null)
                    box.SetActive(false);
            }

            screwBoxes.Clear();
            boxesStack.Clear();
        }

        private void InitAndShuffleColor()
        {
            var threeHoldList = configRecords.Where(r => r.NumberOfScrewHoles == 3).ToList();
            var twoHoldList = configRecords.Where(r => r.NumberOfScrewHoles == 2).ToList();
            var oneHoldList = configRecords.Where(r => r.NumberOfScrewHoles == 1).ToList();

            // tránh 2 box 3-hole liền kề cùng màu
            for (int i = 1; i < threeHoldList.Count; i++)
            {
                if (threeHoldList[i].BoxColor == threeHoldList[i - 1].BoxColor)
                {
                    int swapIndex = i + 1;
                    while (swapIndex < threeHoldList.Count &&
                           threeHoldList[swapIndex].BoxColor == threeHoldList[i].BoxColor)
                    {
                        swapIndex++;
                    }

                    if (swapIndex < threeHoldList.Count)
                    {
                        (threeHoldList[i], threeHoldList[swapIndex]) =
                            (threeHoldList[swapIndex], threeHoldList[i]);
                    }
                }
            }

            var finalList = new List<BoxConfigRecord>();
            finalList.AddRange(threeHoldList);
            finalList.AddRange(twoHoldList);
            finalList.AddRange(oneHoldList);

            int totalStar = threeHoldList.Count * 3 +
                            twoHoldList.Count * 2 +
                            oneHoldList.Count;

            IngameController.ins.TotalStarInLevel = totalStar;

            configRecords = finalList;
            ConfigStack = new Stack<BoxConfigRecord>(configRecords);
        }

        private void InitBoxes()
        {
            screwBoxes.Clear();
            boxesStack.Clear();

            while (ConfigStack.Count > 0)
            {
                var config = ConfigStack.Pop();
                var color = config.BoxColor;
                Box box = null;
                float leftCamPos = CameraMain.instance.GetLeft();

                switch (config.NumberOfScrewHoles)
                {
                    case 1:
                        box = OneHoldBoxPool.Instance.pool.SpawnNonGravity();
                        box.OnInit(Vector2.one * leftCamPos, color, false, config.NumberOfScrewHoles);
                        break;

                    case 2:
                        box = TwoHoldBoxPool.Instance.pool.SpawnNonGravity();
                        box.OnInit(Vector3.left * leftCamPos, color, false, config.NumberOfScrewHoles);
                        break;

                    case 3:
                    default:
                        box = ThreeHoldBoxPool.Instance.pool.SpawnNonGravity();
                        box.OnInit(Vector3.left * leftCamPos, color, false, config.NumberOfScrewHoles);
                        break;
                }

                box.SetActive(false);
                screwBoxes.Add(box);
            }

            IngameController.ins.ShuffleList(screwBoxes);
            var order = screwBoxes.OrderBy(b => b.holdScrews.Count);
            boxesStack = new Stack<Box>(order);
        }

        #endregion

        #region Box Stack helpers

        private void RecalculatingBox(Screw.Screw clearedScrew)
        {
            if (clearedScrew == null || boxesStack.Count == 0)
                return;

            var listBox = boxesStack.ToList();
            var boxSameColor = listBox.FirstOrDefault(b => b.Color == clearedScrew.Color);
            if (boxSameColor == null) return;

            int index = listBox.IndexOf(boxSameColor);
            var color = boxSameColor.Color;
            int totalHold = boxSameColor.TotalHold;

            switch (totalHold)
            {
                case 1:
                    listBox.RemoveAt(index);
                    break;

                case 2:
                    var boxOne = OneHoldBoxPool.Instance.pool.SpawnNonGravity();
                    boxOne.OnInit(Vector3.left * 5, color, false, 1);
                    boxOne.gameObject.SetActive(false);
                    listBox[index] = boxOne;
                    break;

                case 3:
                    var boxTwo = TwoHoldBoxPool.Instance.pool.SpawnNonGravity();
                    boxTwo.OnInit(Vector3.left * 5, color, false, 2);
                    boxTwo.gameObject.SetActive(false);
                    listBox[index] = boxTwo;
                    break;
            }

            IngameController.ins.TotalStarInLevel--;

            var ordered = listBox.OrderBy(b => b.holdScrews.Count).ToList();
            boxesStack = new Stack<Box>(ordered);
            screwBoxes = ordered;
        }

        private Box SpawnBox()
        {
            if (boxesStack.Count == 0) return null;
            return boxesStack.Pop();
        }

        private Box PopBoxByPredicate(Func<Box, bool> predicate)
        {
            var list = boxesStack.ToList();
            list.Reverse();

            var ordered = list.OrderByDescending(b => b.holdScrews.Count).ToList();
            var box = ordered.FirstOrDefault(predicate);
            if (box == null) return null;

            ordered.Remove(box);
            boxesStack = new Stack<Box>(ordered);
            return box;
        }

        #endregion

        #region BoxSlots & Align

        private void InitBoxSlots(List<BoxSlot> slots)
        {
            if (slots == null || slots.Count == 0) return;

            for (int i = 0; i < 4 && i < slots.Count; i++)
            {
                var slot = slots[i];
                var pos = CalculateCenteredPosition(i, 4);

                if (i < activeBoxCount)
                {
                    var box = SpawnBox();
                    if (box == null) continue;

                    slot.Initialize(pos, false, box);
                    box.SetActive(true);
                    box.SetIsLocked(false);

                    StartCoroutine(MoveToSlot(box, slot));
                }
                else
                {
                    var lockBox = ThreeHoldBoxPool.Instance.Spawn();
                    lockBox.SetIsLocked(true);
                    lockBox.SetActive(true);

                    slot.Initialize(pos, true, lockBox);
                    StartCoroutine(MoveToSlot(lockBox, slot));
                }
            }

            StartAligningSlots(slots);
        }

        private Vector3 CalculateCenteredPosition(int index, int totalActiveSlots)
        {
            if (CameraMain.instance.GetCam() == null)
            {
                Debug.Log("Camera is null");
                return Vector3.zero;
            }

            float leftBoundary = CameraMain.instance.GetLeft() - leftBound;
            float rightBoundary = CameraMain.instance.GetRight();
            float topBoundary = CameraMain.instance.GetTop() - topAlignSpacing;

            float width = rightBoundary - leftBoundary;
            float minSpacing = spacingBox;
            float spacing = Mathf.Max(minSpacing, width / (totalActiveSlots + 1));

            float x = leftBoundary + spacing * (index + 1);
            return new Vector3(x, topBoundary, 0);
        }

        public void StartAligningSlots(List<BoxSlot> slots)
        {
            if (slots == null || slots.Count == 0) return;

            if (alignCoroutine != null)
                StopCoroutine(alignCoroutine);

            alignCoroutine = StartCoroutine(AlignSlots(slots));
        }

        private IEnumerator AlignSlots(List<BoxSlot> slots)
        {
            var activeSlots = slots.Where(s => s.gameObject.activeSelf).ToList();
            if (activeSlots.Count == 0) yield break;

            bool done = false;

            while (!done)
            {
                done = true;

                for (int i = 0; i < activeSlots.Count; i++)
                {
                    var slot = activeSlots[i];
                    var target = CalculateCenteredPosition(i, activeSlots.Count);
                    var current = slot.transform.position;

                    var newPos = Vector3.Lerp(current, target, 0.1f);
                    slot.transform.position = newPos;

                    if (slot.screwBox != null && !slot.screwBox.isMoving && !slot.screwBox.IsBoxFull)
                        slot.screwBox.Position = newPos;

                    if (Vector3.Distance(newPos, target) > 0.01f)
                        done = false;
                }

                yield return null;
            }

            alignCoroutine = null;
        }

        #endregion

        #region Box lifecycle

        private void CloseAndRemoveBox(Box screwBox, float time = 0.5f, Action onComplete = null)
        {
            if (screwBox == null)
            {
                onComplete?.Invoke();
                return;
            }

            movingBox = true;

            screwBox.CloseBox(time, _ =>
            {
                Debug.Log("Closed box");
                screwBoxes.Remove(screwBox);

                if (screwBoxes.Count == 0 && rainbowBoxes.Count <0)
                {
                    OnLastBoxClearScrew();
                }

                onComplete?.Invoke();
            });
        }

        private Box TrySpawnNewBox(BoxSlot currentSlot, Func<Box, bool> predicate = null)
        {
            if (currentSlot == null)
            {
                Debug.LogError("[TrySpawnNewBox] Slot is null");
                return null;
            }

            Box box;
            Vector3 spawnPos;

            if (predicate == null)
            {
                spawnPos = new Vector3(CameraMain.instance.GetLeft() - 10,
                                       currentSlot.transform.position.y,
                                       0);
                box = SpawnBox();
                if (box == null) return null;

                box.gameObject.SetActive(false);
            }
            else
            {
                spawnPos = new Vector3(CameraMain.instance.GetLeft() - 10,
                                       currentSlot.transform.position.y,
                                       0);
                box = PopBoxByPredicate(predicate);
                if (box == null) return null;

                box.gameObject.SetActive(true);
            }

            box.transform.position = spawnPos;
            box.Position = spawnPos;
            box.ClearScrewOnHold();

            return box;
        }

        public void DeactivateAndMoveQueue(Box screwBox)
        {
            if (screwBox == null) return;

            var currentSlot = boxSlots.Find(slot => slot.CheckIsContainingThisBox(screwBox));

            CloseAndRemoveBox(screwBox, 0.5f, () =>
            {
                Debug.Log("Close and remove box " + currentSlot);

                MissionManager.ins.OnRainbowBoxClosed(screwBox);
                var newBox = TrySpawnNewBox(currentSlot);
                if (newBox != null)
                {
                    AddHidingScrewToBox(newBox);
                    StartCoroutine(MoveAndHandleBox(newBox, currentSlot));
                }
            });
        }

        private IEnumerator MoveAndHandleBox(Box newBox, BoxSlot currentSlot, float time = 0.5f)
        {
            if (newBox == null || currentSlot == null) yield break;

            bool isDone = false;
            MovingBox = true;

            yield return StartCoroutine(MoveToSlot(newBox, currentSlot, time, ok =>
            {
                isDone = ok;
            }));

            yield return new WaitUntil(() => isDone);
        }

        private IEnumerator MoveToSlot(Box newBox, BoxSlot slot, float time = 0.5f, Action<bool> callback = null)
        {
            yield return new WaitForSeconds(time);

            if (newBox == null)
            {
                Debug.LogWarning("MoveToSlot called with null newBox");
                callback?.Invoke(false);
                yield break;
            }

            var toPos = slot.transform.position;

            newBox.gameObject.SetActive(true);
            newBox.Position = toPos + new Vector3(CameraMain.instance.GetLeft() - 1, 0);
            newBox.isMoving = true;

            slot.AddBox(newBox);

            var t = newBox.transform.DOMove(toPos, time).SetEase(Ease.OutCirc);
            t.OnStart(() =>
            {
                AddHidingScrewToBox(newBox);
            });
            t.OnComplete(() =>
            {
                newBox.isMoving = MovingBox = false;
                newBox.FindScrew();
                callback?.Invoke(true);
            });
        }

        public void ReturnBoxToPool(Box box)
        {
            if (box == null) return;

            int totalHold = box.holdScrews.Count;
            switch (totalHold)
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

        #endregion

        #region Add screw & hiding

        public void AddScrewToBox(Screw.Screw screw, Box box, out bool canAdd)
        {
            canAdd = false;
            if (screw == null || box == null) return;

            if (box.IsAddingScrew)
            {
                Debug.LogWarning($"Box {box.name} is currently adding a screw.");
                return;
            }

            box.AddScrew(screw, out canAdd);

            if (canAdd)
            {
                var screwMng = LevelManager.ins.ScrewManager;
                screwMng.RemoveScrew(screw);
            }
        }

        public void AddMultipleScrew(List<Screw.Screw> screwList, Box box, bool isTele)
        {
            if (box == null || screwList == null || screwList.Count == 0) return;
            if (box.IsAddingScrew) return;

            box.AddScrew(screwList, isTele);
        }

        public void AddHidingScrewToBox(Box newBox)
        {
            if (newBox == null || hidingScrews == null || hidingScrews.Count == 0)
                return;

            var hinding = hidingScrews
                .Where(h => h != null && h.Color == newBox.Color)
                .Take(3)
                .ToList();

            if (hinding.Count > 0)
            {
                AddMultipleScrew(hinding, newBox, true);
            }

            foreach (var h in hinding)
            {
                hidingScrews.Remove(h);
            }
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

            int total = hidingScrews.Count;

            var breakdown = hidingScrews
                .Where(s => s != null)
                .GroupBy(s => s.Color)
                .Select(g => $"{g.Key}:{g.Count()}")
                .ToList();

            var indexed = hidingScrews
                .Select((s, i) => $"{i}:{(s == null ? "null" : s.Color.ToString())}")
                .ToList();

            Debug.Log($"[HidingScrews] Total={total}; Breakdown=[{string.Join(", ", breakdown)}]; Items=[{string.Join(", ", indexed)}]");
        }

        public void RemoveToHidingList(List<Screw.Screw> screws)
        {
            if (screws == null) return;

            foreach (var screwItem in screws)
            {
                hidingScrews.Remove(screwItem);
            }
        }

        #endregion

        #region Find box / move grouped

        public Box FindSuitableBox(Screw.Screw screw, bool allowFallbackToInactive = true)
        {
            if (screw == null) return null;

            if (screw.Color == ColorEnum.Rainbow)
            {
                return rainbowBoxes
                    .Where(box => box != null && !box.IsBoxFull)
                    .OrderByDescending(box => box.NextEmptyIndex)
                    .FirstOrDefault();
            }

            bool BasePredicate(Box box) =>
                box != null &&
                box.Color == screw.Color &&
                !box.isMoving &&
                !box.IsBoxFull;

            var activeCandidate = screwBoxes
                .Where(box => BasePredicate(box) && box.gameObject != null && box.gameObject.activeInHierarchy)
                .OrderByDescending(box => box.isActiveAndEnabled)
                .ThenByDescending(box => box.NextEmptyIndex)
                .FirstOrDefault();

            if (activeCandidate != null)
                return activeCandidate;

            if (!allowFallbackToInactive)
                return null;

            var inactiveCandidate = screwBoxes
                .Where(box => BasePredicate(box) && (box.gameObject == null || !box.gameObject.activeInHierarchy))
                .OrderByDescending(box => box.NextEmptyIndex)
                .ThenByDescending(box => box.NextEmptyIndex)
                .FirstOrDefault();

            return inactiveCandidate;
        }

        public bool TryMoveScrewsGroupedByColor(List<Screw.Screw> screws, bool includeInactive = false)
        {
            if (screws == null || screws.Count == 0) return false;

            var distinct = screws.Where(s => s != null).Distinct().ToList();
            if (distinct.Count == 0) return false;

            var groups = distinct.GroupBy(s => s.Color);
            int totalMoved = 0;

            foreach (var group in groups)
            {
                var color = group.Key;
                var groupList = group.ToList();

                var box = FindSuitableBox(groupList[0], includeInactive);
                if (box == null)
                {
                    AddToHidingList(groupList);
                    Debug.Log($"BoxQueue: No suitable box found for color {color}. Skipping {groupList.Count} screws.");
                    continue;
                }

                var freeHold = box.holdScrews.Where(h => h.IsEmpty()).ToList();
                int freeSlots = freeHold.Count;

                List<Screw.Screw> toMove = groupList;
                if (freeSlots > 0 && groupList.Count > freeSlots)
                {
                    toMove = groupList.Take(freeSlots).ToList();
                }
                else if (freeSlots <= 0)
                {
                    GameUtils.LogAndSelect($"Box {box.name} is full for color {color}. Skipping.", box.gameObject);
                    continue;
                }

                AddMultipleScrew(toMove, box, false);
                totalMoved += toMove.Count;

                if (toMove.Count < groupList.Count)
                {
                    Debug.Log($"Moved {toMove.Count} of {groupList.Count} screws into box {box.name} for color {color} (box filled).");
                }
            }

            Debug.Log($"TryMoveScrewsGroupedByColor: moved {totalMoved} screws grouped by color.");
            return totalMoved > 0;
        }

        #endregion

        #region Unlock / new slot / reset

        private void OnLastBoxClearScrew()
        {
            Debug.Log("on last box clear screw");
            bool isComplete = onCompleteClearBoxes != null && IngameController.ins.IsGameOver;
            onCompleteClearBoxes?.Invoke(isComplete);
        }

        //public bool CanAddRainbowScrew(Box box)
        //{
        //    return box == rainbowBox;
        //}

        public int IdexLockedBox()
        {
            return screwBoxes.FindIndex(b => b.IsLocked);
        }

        public void UnlockedBox()
        {
            var slot = boxSlots.FirstOrDefault(s => s.screwBox != null && s.screwBox.IsLocked);
            Debug.Log("unlock box");

            if (slot == null)
            {
                Debug.LogWarning("[UnlockedBox] No locked slot found!");
                return;
            }

            var boxUnlock = slot.screwBox;
            if (boxUnlock == null)
            {
                Debug.LogWarning("[UnlockedBox] Slot found but screwBox is NULL");
                return;
            }

            var color = ArrayScrew.Instance.GetMostestColorInArray();
            boxUnlock.SetActive(false);

            CloseAndRemoveBox(boxUnlock, 0.01f, () =>
            {
                Box newBox;
                if (color == ColorEnum.Clear)
                    newBox = TrySpawnNewBox(slot);
                else
                    newBox = TrySpawnNewBox(slot, b => b.Color == color);

                Debug.Log("try spawn new box " + newBox);
                AddHidingScrewToBox(newBox);

                if (newBox != null)
                    StartCoroutine(MoveAndHandleBox(newBox, slot, 0));
            });
        }

        public void AddNewBoxSlot()
        {
            if (boxSlots.All(slot => slot.gameObject.activeSelf)) return;

            var newBoxSlot = boxSlots.First(slot => !slot.gameObject.activeSelf);
            newBoxSlot.gameObject.SetActive(true);

            ColorEnum colorActiveInArray = ArrayScrew.Instance.GetMostestColorInArray();
            Box newBox = null;

            if (colorActiveInArray != ColorEnum.Clear)
            {
                newBox = TrySpawnNewBox(newBoxSlot, b => b.Color == colorActiveInArray);
            }

            if (newBox == null)
            {
                newBox = TrySpawnNewBox(newBoxSlot);
            }

            if (newBox != null)
            {
                StartCoroutine(MoveAndHandleBox(newBox, newBoxSlot));
                StartAligningSlots(boxSlots);
            }
            else
            {
                Debug.LogWarning("[AddNewBoxSlot] No box available to spawn.");
            }
        }

        public void OnReset()
        {
            ThreeHoldBoxPool.Instance.pool.DeSpawnAll();
            TwoHoldBoxPool.Instance.pool.DeSpawnAll();
            OneHoldBoxPool.Instance.pool.DeSpawnAll();

            foreach (var box in screwBoxes)
            {
                if (box != null)
                    box.Reset();
            }

            screwBoxes.Clear();
            boxesStack.Clear();
            hidingScrews?.Clear();
            foreach (var rb in rainbowBoxes)
            {
                if (rb != null) rb.Reset();
            }

            rainbowBoxes.Clear();
        }

        internal void RemoveBoxByColor(ColorEnum color, int requiredCount)
        {
            var listedBox = screwBoxes
                .Where(b => b != null && b.Color == color)
                .Take(requiredCount)
                .ToList();
            screwBoxes.RemoveAll(b => listedBox.Contains(b));
        }

        #endregion
    }
}
