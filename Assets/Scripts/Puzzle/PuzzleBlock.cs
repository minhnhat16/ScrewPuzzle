using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class BlockParam
{
    public int blockId;
    public int screwRequired;
    // trạng thái lưu
    public bool unlocked;
    // cell nào đã bị remove trước đó (save/load)
    public Dictionary<int, bool> removedCells = new();
}
public abstract class PuzzleBlock : MonoBehaviour, IResetable
{
    public int blockId;
    public bool unlocked = false;
    [SerializeField]
    private List<PuzzleCellUI> cells;
    protected Image image;
    private int remainingCells;

    private BlockParam param;
    private UnityEvent<PuzzleBlock, PuzzleCellUI> cellRemove = new();

    private RectTransform rectTransform;
    public List<PuzzleCellUI> Cells { get => cells; set => cells = value; }
    

    private void OnDisable()
    { 
        rectTransform.anchoredPosition = Vector3.zero;
    }
    protected virtual void Awake()
    {
        image = GetComponentInChildren<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    // =====================================================
    // INIT (UI ONLY)
    // =====================================================
    public virtual ShapeResult Init(
    int id,
    List<PuzzleCellUI> cells,
    BlockParam param
)
    {
        blockId = id;
        this.cells = cells;

        // ====== LOAD STATE FROM PARAM ======
        unlocked = param != null && param.unlocked;

        remainingCells = cells.Count;
        var cellParam = param?.removedCells;
        foreach (var c in cells)
        {
            c.SetOwner(this);
            c.IsOn = cellParam.TryGetValue(c.record.Id, out bool isRemoved) ? !isRemoved : true;

            c.SetCellOn(c.IsOn);
            // nếu cell đã bị remove trước đó
            if (param != null &&
                param.removedCells.TryGetValue(c.record.Id, out bool removed) &&
                removed)
            {
                c.SetCellOn(false);
                remainingCells--;
            }
        }

        // nếu load vào mà đã hết cell
        if (remainingCells <= 0)
        {
            unlocked = true;
            gameObject.SetActive(false);    
        }

        var shape = BlockShapeAnalyzer.Analyze(cells);
        return shape;
    }


    // =====================================================
    // VISUAL API (CALLED FROM BOARD / FACTORY)
    // =====================================================
    public void ApplyVisual(
        ShapeResult shape,
        Sprite sprite
    )
    {
        SetSprite(sprite);
        if (shape.shape == BlockShape.Rectangle2
            || shape.shape == BlockShape.Rectangle3
            || shape.shape == BlockShape.L3
            || shape.shape == BlockShape.L4)
        {
            SetSize();
            SetRotation(shape.orientation);
        }
        else
        {
            image.rectTransform.localRotation = Quaternion.identity;
        }
    }

    public void SetRotation(BlockOrientation orientation)
    {
        float z = orientation switch
        {
            BlockOrientation.Up => 0,
            BlockOrientation.Right => -90,
            BlockOrientation.Down => 180,
            BlockOrientation.Left => 90,
            _ => 0
        };

        image.rectTransform.rotation = Quaternion.Euler(0, 0, z);
    }

    public void SetSizeByCells()
    {
        if (cells == null || cells.Count == 0)
            return;

        RectTransform rt = (RectTransform)transform;
        RectTransform firstCell = (RectTransform)cells[0].transform;

        Vector2 cellSize = firstCell.rect.size;

        int minX = cells.Min(c => c.Pos.x);
        int maxX = cells.Max(c => c.Pos.x);
        int minY = cells.Min(c => c.Pos.y);
        int maxY = cells.Max(c => c.Pos.y);

        rt.sizeDelta = new Vector2(
            (maxX - minX + 1) * cellSize.x,
            (maxY - minY + 1) * cellSize.y
        );
    }
    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
        image.SetNativeSize();
    }

    public void SetSize()
    {
        if (cells == null || cells.Count == 0)
            return;

        RectTransform blockRT = (RectTransform)transform;

        // lấy rectTransform của cell đầu tiên để biết cellSize
        RectTransform firstCellRT = (RectTransform)cells[0].transform;
        Vector2 cellSize = firstCellRT.rect.size;

        // tính bounding box theo tọa độ logic (x,y)
        int minX = cells.Min(c => c.Pos.x);
        int maxX = cells.Max(c => c.Pos.x);
        int minY = cells.Min(c => c.Pos.y);
        int maxY = cells.Max(c => c.Pos.y);

        int widthInCell = maxX - minX + 1;
        int heightInCell = maxY - minY + 1;

        // set size cho block
        blockRT.sizeDelta = new Vector2(
            widthInCell * cellSize.x,
            heightInCell * cellSize.y
        );
    }

    // =====================================================
    // GAMEPLAY
    // =====================================================
    public virtual bool IsUnlocked()
    {
        Debug.Log($"Block {blockId} is unlocked: {unlocked}");
        return unlocked;
    }

    public void OnCellRemoved(PuzzleCellUI cell)
    {
        if (unlocked)
            return;

        remainingCells--;

        // cập nhật param nếu có
        if (cell.record != null && param != null)
        {
            param.removedCells[cell.record.Id] = true;
        }

        if (CanUnlock())
        {
            Unlock();
        }

        cellRemove?.Invoke(this, cell);
    }

    public bool CanUnlock()
    {
        return !unlocked && remainingCells <= 0;
    }
    public void Unlock()
    {
        this.unlocked = true;
        this.gameObject.SetActive(false);
    }
    // =====================================================
    // Event registration API so PuzzleBoardUI can listen
    // =====================================================
    public void RegisterCellRemoveListener(UnityAction<PuzzleBlock, PuzzleCellUI> listener)
    {
        if (listener == null) return;
        cellRemove.AddListener(listener);
    }

    public void UnregisterCellRemoveListener(UnityAction<PuzzleBlock, PuzzleCellUI> listener)
    {
        if (listener == null) return;
        cellRemove.RemoveListener(listener);
    }

    public void OnReset()
    {
        unlocked = false;
        blockId = -1;
        remainingCells = -1;
        param = null;
        image.sprite = null;
        cells.Clear();
    }
}
