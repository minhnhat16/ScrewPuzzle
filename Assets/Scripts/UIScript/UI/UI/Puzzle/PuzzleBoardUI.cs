using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PuzzleBoardUI : MonoBehaviour, IResetable
{
    [Header("Grid")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private RectTransform cellRoot;
    [SerializeField] private RectTransform blockRoot;

    [Header("Prefabs")]
    [SerializeField] private PuzzleCellUI cellPrefab;
    [SerializeField] private PuzzleBlock blockPrefab;


    public PuzzleBoardRuntime runtime;
    private PuzzleBoardLogic logic;

    private Dictionary<int, List<PuzzleCellUI>> blockCells
        = new Dictionary<int, List<PuzzleCellUI>>();

    private Dictionary<int, PuzzleBlock> blockViews
        = new Dictionary<int, PuzzleBlock>();
    public Action<int> grandPrize;
    // Event invoked when every block on the runtime board is unlocked/cleared
    public UnityEvent OnAllBlocksCleared = new();


    public Action<int> updatePlayerScrew;
    // Guard to prevent firing the "all cleared" event multiple times
    private bool allClearedFired = false;

    // =====================================================
    // INIT
    // =====================================================
    public void Init(int curentScrew, PuzzleBoardRuntime runtime)
    {
        this.runtime = runtime;
        logic = new PuzzleBoardLogic(runtime);
        logic.playerScrew = curentScrew;
        logic.OnBlockUnlocked += HandleBlockUnlocked;
        logic.OnScrewClick += HandleUpdatePlayerScrew;
        // Replace this line in Init:
        // OnAllBlocksCleared.AddListener(GrandPrize(5));
        OnAllBlocksCleared.AddListener(() => UpdatePuzzleProgress(5));
    }

    // =====================================================
    // LOAD PATTERN
    // =====================================================
    public void LoadPattern(
    PuzzleBoardRecord boardConfig,
    PuzzlePartenRecord pattern
)
    {
        ClearBoard();

        if (boardConfig == null || pattern == null)
        {
            Debug.LogError("[PuzzleBoardUI] Config NULL");
            return;
        }

        // ===============================
        // SETUP GRID
        // ===============================
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = boardConfig.width;

        float cellSize = cellRoot.rect.width / boardConfig.width;
        grid.cellSize = new Vector2(cellSize, cellSize);

        // ===============================
        // LOAD CELL CONFIG
        // ===============================
        var cellRecords = ConfigExtensions
            .GetAllPuzzleCellConfig(ConfigFileManager.Instance)
            .FindAll(c => c.PatternId == pattern.patternId);

        if (cellRecords.Count == 0)
        {
            Debug.LogError($"[PuzzleBoardUI] No cell for pattern {pattern.patternId}");
            return;
        }

        // sort stable for grid
        cellRecords.Sort((a, b) =>
            a.Y == b.Y ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

        string orderLog = string.Join(" | ",
            cellRecords.Select(c => $"({c.X},{c.Y})#{c.Id}")
        );


        Debug.Log("[CellOrder] " + orderLog);
        // ===============================
        // SPAWN CELLS (VIEW ONLY)
        // ===============================
        foreach (var record in cellRecords)
        {
            var cell = PuzzleCellPool.ins.Spawn();
            cell.Setup(record);
            cell.Init(this);

            Debug.Log(
                $"[Cell setup] id={record.Id}, " +
                $"pos=({record.X},{record.Y}), " +
                $"blockId={record.BlockId}," +
                $"is cell on = {cell.IsOn}"
            );
            RegisterCell(cell);
        }



        PuzzleCellPool.ins.ShortBy(cellRecords);
        // force layout
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(cellRoot);
        Canvas.ForceUpdateCanvases();

        // ===============================
        // LOAD SAVED BLOCK PARAMS
        // ===============================
        var blockParams = GetCurrentBlockParams();
        var paramMap = blockParams.ToDictionary(p => p.blockId);

        // ===============================
        // CREATE BLOCKS (RENDER FROM RUNTIME)
        // ===============================
        foreach (var pair in blockCells.OrderBy(kv => kv.Key))
        {
            int blockId = pair.Key;
            var cells = pair.Value;

            // safety
            if (cells.Any(c => c.BlockId != blockId))
            {
                Debug.LogError($"[Block ERROR] Mixed blockId in group {blockId}");
                continue;
            }

            // get save param if exists
            paramMap.TryGetValue(blockId, out var param);


            // ===============================
            // REGISTER RUNTIME (ONLY IF ABSENT)
            // ===============================
            int screwRequired = cells.Count; // total cells = total screws
            int removedCells = param.removedCells.Count(c => c.Value);

            runtime.RegisterBlock(blockId, screwRequired, removedCells);

            // ===============================
            // INIT BLOCK VIEW (APPLY SAVE)
            // ===============================
            var block = PuzzleBlockPool.ins.Spawn();

            string cellsLog = string.Join(", ",
                cells.Select(c => c.id.ToString())
            );

            var shapeResult = block.Init(blockId, cells, param);

            // visual
            var sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, "Block " + shapeResult.shape);

            block.ApplyVisual(shapeResult, sprite);
            block.SetSize();



            // position
            RectTransform blockRT = (RectTransform)block.transform;
            blockRT.anchoredPosition =
                GetBlockCenterInBlockRoot(cells, blockRoot);

            blockViews.Add(blockId, block);
            RegisterBlockListeners(block);

            Debug.Log(
                $"[Block setup] id={blockId}, cells={cells.Count}, " +
                $"saved={(param != null)}"
            );
        }

        Debug.Log(
            $"[PuzzleBoardUI] Pattern {pattern.patternId} " +
            $"cells={cellRecords.Count}, blocks={blockCells.Count}"
        );

        CheckAllBlocksCleared();
    }


    public List<BlockParam> GetCurrentBlockParams()
    {
        // lấy data cũ (từ save)
        List<BlockParam> savedParams =
            DataAPIController.instance.GetBlocksData();
        // map nhanh theo blockId
        Dictionary<int, BlockParam> paramMap =
            savedParams != null
                ? savedParams.ToDictionary(p => p.blockId)
                : new Dictionary<int, BlockParam>();

        string cells = string.Join(" | ",
             paramMap.Values.Select(p =>
                 $"Block {p.blockId}: " +
                 $"removedCells=[{string.Join(",", p.removedCells.Keys)}], " +
                 $"unlocked={p.unlocked}"
             )
         );
        return paramMap.Values.ToList();
    }


    private Vector2 GetBlockCenterInBlockRoot(
 List<PuzzleCellUI> cells,
 RectTransform blockRoot)
    {
        // Convert each cell world position to local position in blockRoot
        // Use the Canvas camera if necessary for ScreenPoint conversion
        var canvas = blockRoot.GetComponentInParent<Canvas>();
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var cell in cells)
        {
            if (cell == null) continue;
            var rt = cell.GetComponent<RectTransform>();
            // world point of the cell center
            Vector3 worldPos = rt.TransformPoint(rt.rect.center);
            // convert to screen then to local point in blockRoot
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(blockRoot, screenPoint, cam, out Vector2 localPoint);

            // cell visual half-size in blockRoot local space approximation:
            // use rt.rect.size in local units (they share same scale in typical UI)
            Vector2 half = rt.rect.size * 0.5f;

            // Adjust local bounds using localPoint +/- half (approx)
            minX = Mathf.Min(minX, localPoint.x - half.x);
            maxX = Mathf.Max(maxX, localPoint.x + half.x);
            minY = Mathf.Min(minY, localPoint.y - half.y);
            maxY = Mathf.Max(maxY, localPoint.y + half.y);
        }

        if (minX == float.MaxValue)
            return Vector2.zero;

        return new Vector2(
            (minX + maxX) * 0.5f,
            (minY + maxY) * 0.5f
        );
    }



    // =====================================================
    // CELL CLICK
    // =====================================================
    public void OnCellClicked(PuzzleCellUI cell)
    {
        logic.OnCellClicked(cell);

    }
    // =====================================================
    // BLOCK UNLOCK VISUAL
    // =====================================================
    private void HandleBlockUnlocked(int blockId, bool isUnlocked)
    {
        var block = blockViews.TryGetValue(blockId, out var view);
        view.unlocked = isUnlocked;

        Debug.Log($"[PuzzleBoardUI] Block unlocked: id={blockId}, view found={block}, isUnlocked={isUnlocked}");
        if (block)
        {
            view.Unlock(); // hide block view when unlocked
            grandPrize?.Invoke(blockId);
        }
        CheckAllBlocksCleared();
    }

    // =====================================================
    // INTERNAL
    // =====================================================
    private void RegisterCell(PuzzleCellUI cell)
    {
        if (!blockCells.TryGetValue(cell.BlockId, out var list))
        {
            list = new List<PuzzleCellUI>();
            blockCells.Add(cell.BlockId, list);
        }

        list.Add(cell);
    }

    private void ClearBoard()
    {
        // Return all block views to pool and unregister listeners
        if (blockViews != null && blockViews.Count > 0)
        {
            foreach (var kv in blockViews.ToList())
            {
                var block = kv.Value;
                if (block == null) continue;
                UnregisterBlockListeners(block);
                PuzzleBlockPool.ins.Return(block);
            }
            blockViews.Clear();
        }

        // Return cell views to pool
        if (blockCells != null && blockCells.Count > 0)
        {
            foreach (var list in blockCells.Values)
            {
                if (list == null) continue;
                foreach (var cell in list)
                {
                    if (cell == null) continue;
                    cell.SetOwner(null);
                    if (PuzzleCellPool.ins != null) PuzzleCellPool.ins.Return(cell);
                    else Destroy(cell.gameObject);
                }
            }
        }

        blockCells.Clear();

        // Clear runtime state
        runtime?.Clear();

        // reset all-cleared flag so next board can fire event
        allClearedFired = false;

        // Clear grandPrize listeners (do not destroy external subscribers)

        OnAllBlocksCleared.RemoveAllListeners();
        // Force layout rebuild so next LoadPattern has stable rects
        Canvas.ForceUpdateCanvases();
        if (cellRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(cellRoot);
    }

    // =====================================================
    // BLOCK LISTENERS
    // =====================================================

    /// <summary>
    /// Call this right after you instantiate or initialize a PuzzleBlock view.
    /// Example usage:
    ///   var block = Instantiate(blockPrefab, blockRoot);
    ///   block.Init(...);
    ///   RegisterBlockListeners(block);
    /// </summary>
    private void RegisterBlockListeners(PuzzleBlock block)
    {
        if (block == null) return;
        // register a handler to react when a cell in the block is removed
        block.RegisterCellRemoveListener(OnBlockCellRemoved);
    }

    private void UnregisterBlockListeners(PuzzleBlock block)
    {
        if (block == null) return;
        block.UnregisterCellRemoveListener(OnBlockCellRemoved);
    }

    /// <summary>
    /// Called when a PuzzleBlock reports one of its cells was removed.
    /// You can forward this to board-level logic or update UI, play FX, etc.
    /// </summary>
    private void OnBlockCellRemoved(PuzzleBlock block, PuzzleCellUI cell)
    {
        if (block == null || cell == null) return;


        Debug.Log("On block cell removed: blockId=" + block.blockId + ", cellId=" + cell.id);
        runtime.blocks.Remove(block.blockId);

        if (block.IsUnlocked())
        {
            grandPrize?.Invoke(block.blockId);
            CheckAllBlocksCleared();
        }
    }
    public void HandleUpdatePlayerScrew(int newScrew)
    {
        logic.playerScrew = newScrew;
        updatePlayerScrew?.Invoke(newScrew);
    }
    /// <summary>
    /// Check runtime state and invoke OnAllBlocksCleared once when every registered block is unlocked.
    /// </summary>
    private void CheckAllBlocksCleared()
    {
        if (allClearedFired) return;
        if (runtime == null || runtime.blocks == null || runtime.blocks.Count == 0) return;

        bool allUnlocked = runtime.blocks.Values.All(bs => bs.isUnlocked);
        if (allUnlocked)
        {
            allClearedFired = true;
            Debug.Log("[PuzzleBoardUI] All blocks cleared!");
            OnAllBlocksCleared?.Invoke();
        }
    }
    public void UpdatePuzzleProgress(int idReward)
    {
    }
    public void OnReset()
    {
        // optional: clear board when resetting
        ClearBoard();
    }


}
