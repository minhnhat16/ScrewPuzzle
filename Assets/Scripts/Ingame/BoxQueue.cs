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
using System.Threading;
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

            InitAndShuffleColorSmart();
            InitBoxes();

            yield return new WaitForSeconds(0.1f);
        }

        public void InitRainbowBoxes(int count = 1, ColorEnum targetColorID = 0)
        {
            var list = boxesStack.ToList();
            var oldColor = list.Where(b => b.Color == targetColorID).Take(count).ToList();
            foreach (var oc in oldColor)
            {
                list.Remove(oc);
            }

            screwBoxes.Clear();
            screwBoxes = list.OrderByDescending(b => b.TotalHold).ToList();
            boxesStack = new Stack<Box>(list);
            boxesStack.OrderByDescending(b => b.TotalHold);
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
        private Dictionary<ColorEnum, int> GetDesignWeight()
        {
            return configRecords
                .GroupBy(r => r.BoxColor)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.NumberOfScrewHoles)
                );
        }
        private Dictionary<ColorEnum, int> GetActiveDemand()
        {
            var dict = new Dictionary<ColorEnum, int>();

            foreach (var kv in LevelManager.ins.layerManager.screwDict)
            {
                foreach (var screw in kv.Value)
                {
                    if (screw == null) continue;

                    if (!dict.ContainsKey(screw.Color))
                        dict[screw.Color] = 0;

                    dict[screw.Color]++;
                }
            }

            return dict;
        }
        public void InitAndShuffleColorSmart()
        {
            var activeDemand = GetActiveDemand();
            var designWeight = GetDesignWeight();

            // GOM bucket theo màu
            var buckets = configRecords
                .GroupBy(r => r.BoxColor)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<BoxConfigRecord>();
            ColorEnum? last = null;

            while (true)
            {
                bool allEmpty = buckets.Values.All(l => l.Count == 0);
                if (allEmpty) break;

                var color = ChooseNextColor(activeDemand, buckets, last);

                if (!buckets.ContainsKey(color) || buckets[color].Count == 0)
                    continue;

                // LẤY BOX QUAN TRỌNG NHẤT TRONG MÀU
                var next = buckets[color]
                    .OrderByDescending(r => r.NumberOfScrewHoles)
                    .First();

                result.Add(next);
                buckets[color].Remove(next);
                last = color;
            }

            configRecords = result;
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

            var ordered = listBox.OrderByDescending(b => b.TotalHold).ToList();
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
        private ColorEnum ChooseNextColor(
        Dictionary<ColorEnum, int> activeDemand,
        Dictionary<ColorEnum, List<BoxConfigRecord>> buckets,
        ColorEnum? lastColor)
        {
            float alpha = 2f;   // mức ưu tiên theo screw thật
            float beta = 1f;   // mức ưu tiên theo thiết kế

            ColorEnum bestColor = default;
            float bestScore = float.NegativeInfinity;

            foreach (var kv in buckets)
            {
                var color = kv.Key;
                var list = kv.Value;

                if (list.Count == 0)
                    continue;

                int demand = activeDemand.ContainsKey(color) ? activeDemand[color] : 0;
                int design = list.Sum(r => r.NumberOfScrewHoles);

                float penalty = (lastColor == color) ? 5f : 0f;

                float score = demand * alpha + design * beta - penalty;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColor = color;
                }
            }

            return bestColor;
        }
        #endregion

        #region BoxSlots & Align
        public void InitBoxToSlot()
        {
            InitBoxSlots(boxSlots);
        }
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



                Debug.Log("Screw boxes count after remove: " + screwBoxes.Count);

                int specialScrew = SideMissionManager.ins.currentMission.requiredCount;
                int currentScrew = SideMissionManager.ins.currentMission.currentCount;
                bool missionComplete = currentScrew >= specialScrew;



                if (screwBoxes.Count <= 0 && missionComplete)
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
            //box.ClearScrewOnHold();
            box.SetIsLocked(false);

            return box;
        }

        public void DeactivateAndMoveQueue(Box screwBox, float timeClose = 0.5f)
        {
            if (screwBox == null) return;

            var currentSlot = boxSlots.Find(slot => slot.CheckIsContainingThisBox(screwBox));

            CloseAndRemoveBox(screwBox, timeClose, () =>
            {
                Debug.Log("Close and remove box " + currentSlot);

                MissionManager.ins.OnRainbowBoxClosed(screwBox);
                var newBox = TrySpawnNewBox(currentSlot);
                if (newBox != null)
                {
                    var fpos = currentSlot.transform.position;
                    fpos.x -= 10;
                    newBox.transform.position = fpos;
                    newBox.Render.enabled = true;
                    StartCoroutine(MoveAndHandleBox(newBox, currentSlot, fpos));
                }
            });
        }

        private IEnumerator MoveAndHandleBox(Box newBox, BoxSlot currentSlot, Vector3 fPos = default, float time = 0.5f)
        {
            if (newBox == null || currentSlot == null) yield break;

            bool isDone = false;
            MovingBox = true;

            yield return StartCoroutine(MoveToSlot(newBox, currentSlot, time, fPos, ok =>
            {
                isDone = ok;
            }));

            yield return new WaitUntil(() => isDone);
        }

        private IEnumerator MoveToSlot(Box newBox, BoxSlot slot, float time = 0.5f, Vector3 fromPos = default, Action<bool> callback = null)
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

            if (fromPos == default)
                fromPos = newBox.transform.position;
            newBox.transform.position = fromPos;

            slot.AddBox(newBox);

            var t = newBox.transform.DOMove(toPos, time).SetEase(Ease.InOutElastic);
            t.OnStart(() =>
            {
                AddHidingScrewToBox(newBox);
                newBox.isMoving = true;

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
                int boxLayer = box.Render.sortingLayerID;
                screw.SetSortingOrderAndLayer(boxLayer + 2, box.Render.sortingLayerName);
                screwMng.RemoveScrew(screw);
                MissionManager.OnScrewCollected.Invoke(screw.Color,1);

            }
            else
            {
                screw.IsClicked = false;
                Debug.Log($"Cannot add screw {screw.name} to box {box.name}.");
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


            Debug.Log($"new box is moving " + newBox.isMoving);
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
            if (screw == null)
                return null;

            // -------------------------------
            // Predicate chung cho Normal Box
            // -------------------------------
            bool Match(Box box) =>
                box != null &&
                box.Color == screw.Color &&
                !box.isMoving &&
                !box.IsBoxFull;

            // -------------------------------
            // CASE 2: Ưu tiên Active Boxes
            // -------------------------------
            var activeBox = screwBoxes
                .Where(b => Match(b) && b.gameObject.activeInHierarchy)
                .OrderByDescending(b => b.NextEmptyIndex)   // ưu tiên box gần full trước
                .FirstOrDefault();

            if (activeBox != null)
                return activeBox;

            // -------------------------------
            // CASE 3: Allow fallback vào inactive boxes
            // -------------------------------
            if (!allowFallbackToInactive)
                return null;

            var inactiveBox = screwBoxes
                .Where(b => Match(b) && !b.gameObject.activeInHierarchy)
                .OrderByDescending(b => b.NextEmptyIndex)
                .FirstOrDefault();

            return inactiveBox;
        }


        public bool TryMoveScrewsGroupedByColor(List<Screw.Screw> screws, bool includeInactive = false)
        {
            if (screws == null || screws.Count == 0)
                return false;

            var validScrews = screws.Distinct().Where(s => s != null).ToList();
            if (validScrews.Count == 0)
                return false;

            // -------------------------------
            // NEW: Nếu level dùng Special Box → đưa hết screw vào SpecialBoxManager,
            // không dùng box thường / rainbow nữa.
            // -------------------------------

            int totalMoved = 0;

            var groups = validScrews.GroupBy(s => s.Color);

            foreach (var group in groups)
            {
                var color = group.Key;
                var groupList = group.ToList();
                var screw = groupList[0];
                var box = FindSuitableBox(screw, false);

                if (box == null && group.Key != ColorEnum.Rainbow)
                {
                    AddToHidingList(groupList);
                    Debug.Log($"❌ Không tìm thấy box {color}, đưa {groupList.Count} screw vào hiding.");
                    continue;
                }

                if (hasSpecialBox && group.Key == ColorEnum.Rainbow)
                {
                    if (SpecialBoxManager.ins == null)
                    {
                        Debug.LogWarning("[BoxQueue] hasSpecialBox = true nhưng SpecialBoxManager.Instance = null");
                        return false;
                    }

                    SpecialBoxManager.ins.AddScrews(groupList);
                    Debug.Log($"[BoxQueue] SpecialBox: moved {validScrews.Count} screws to special box.");
                    return true;
                }


                // Lấy hold trống
                var freeHold = box.holdScrews.Where(h => h.IsEmpty()).ToList();
                int freeSlots = freeHold.Count;

                if (freeSlots <= 0)
                {
                    Debug.Log($"⚠ Box {box.Color} đã full → skip màu {color}");
                    continue;
                }

                // Cắt số lượng screw cần move
                var toMove = groupList.Take(freeSlots).ToList();

                // move
                AddMultipleScrew(toMove, box, false);

                totalMoved += toMove.Count;

                Debug.Log($"✔ Moved {toMove.Count}/{groupList.Count} screw vào box {box.Color}");

                // Nếu còn dư → cho hiding (nhưng KHÔNG hiding Rainbow)
                var remain = groupList.Skip(toMove.Count).ToList();
                if (remain.Count > 0)
                {
                    if (box.Color != ColorEnum.Rainbow)
                        AddToHidingList(remain);
                }
            }



            Debug.Log($"[TryMove] Tổng move = {totalMoved}");
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

            activeBoxCount++;
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

                var fpos = Vector3.down * 2;
                newBox.transform.position = fpos;
                if (newBox != null)
                    StartCoroutine(MoveWithFade(newBox, slot, fpos, 1f, 0.25f));

                MissionManager.OnBoxClosed?.Invoke();
            });
        }

        public IEnumerator MoveWithFade(Box newBox, BoxSlot slot, Vector3 startPos, float moveTime = 0.5f, float fadeTime = 0.25f)
        {
            if (newBox == null) yield break;

            // đưa box về vị trí spawn
            newBox.transform.position = startPos;
            newBox.SetActive(true);

            // Fade in trước
            yield return StartCoroutine(FadeInBox(newBox, fadeTime));

            // Fade xong thì move box vào slot
            yield return StartCoroutine(MoveAndHandleBox(newBox, slot, startPos, moveTime));
        }
        public IEnumerator FadeInBox(Box box, float duration = 0.25f)
        {
            if (box == null) yield break;

            var renderer = box.GetComponentInChildren<SpriteRenderer>();
            if (renderer == null) yield break;

            // set alpha = 0 trước
            var c = renderer.color;
            c.a = 0;
            renderer.color = c;

            renderer.DOFade(1f, duration).SetEase(Ease.OutSine);
            yield return new WaitForSeconds(duration);
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
                Vector3 fPos = Vector3.left * -6;
                StartCoroutine(MoveAndHandleBox(newBox, newBoxSlot, fPos, 4));
                StartAligningSlots(boxSlots);
            }
            else
            {
                Debug.LogWarning("[AddNewBoxSlot] No box available to spawn.");
            }
        }

        public void OnReset()
        {
            activeBoxCount = 2;
            ThreeHoldBoxPool.Instance.ReturnAll();
            TwoHoldBoxPool.Instance.pool.DeSpawnAll();
            OneHoldBoxPool.Instance.pool.DeSpawnAll();
            screwBoxes.Clear();
            boxesStack.Clear();
            hidingScrews?.Clear();
            StopAllCoroutines();
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
