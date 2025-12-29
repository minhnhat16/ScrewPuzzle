using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleCellUI : MonoBehaviour, IPointerClickHandler,IResetable
{
    public PuzzleCellRecord record;
    public PuzzleBlock owner;

    // ===============================
    // PROPERTIES
    // ===============================
    public int id => record.Id;
    private bool isOn;
    public int BlockId => record.BlockId;
    public Vector2Int Pos => new Vector2Int(record.X, record.Y);

    public bool IsOn { get => isOn; set => isOn = value; }

    private PuzzleBoardUI board;

    private Image img;

    private void OnDisable()
    {
        OnReset();
    }


    private void Awake()
    {
        img = GetComponentInChildren<Image>();
    }
    // ===============================
    // INIT
    // ===============================
    public void Setup(PuzzleCellRecord record)
    {
        this.record = record;
    }

    public void Init(PuzzleBoardUI board)
    {
        this.board = board;

        Debug.Log("cell position " + ((RectTransform)this.transform).position);
    }

    public void SetOwner(PuzzleBlock block)
    {
        owner = block;
    }

    // ===============================
    // INPUT
    // ===============================
    public void OnPointerClick(PointerEventData eventData)      
    {
        if (board == null)
        {
            Debug.LogError("PuzzleCellUI: board is null");
            return;
        }
        board.OnCellClicked(this); // Fix: Use OnCellClicked instead of OnCellClick
    }
    public void SetCellOn(bool isOn)
    {
        this.IsOn = isOn;
        img.gameObject.SetActive(isOn);
    }
    // ===============================
    // VISUAL
    // ===============================
    public void PlayUnlock()
    {
        // prototype – sau này thay animation
        gameObject.SetActive(false);
    }

    public void OnReset()
    {
        IsOn = true;
        record = null;
        owner = null;
    }

}
