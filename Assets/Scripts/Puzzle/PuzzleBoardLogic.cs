using System;
using System.DataBase;
using System.Diagnostics;
using UnityEngine.LowLevel;

public class PuzzleBoardLogic
{
    private PuzzleBoardRuntime runtime;
    public int playerScrew = 100;

    public Action<int, bool> OnBlockUnlocked;
    internal Action<int> OnScrewClick;

    public PuzzleBoardLogic(PuzzleBoardRuntime runtime)
    {
        this.runtime = runtime;
    }

    public void AddScrew(int amount)
    {
        playerScrew += amount;
    }

    public void OnBlockClicked(int blockId)
    {
        if (runtime == null)
        {
            return;
        }

        bool unlocked = runtime.TryUnlockByRemovingOneCell(blockId, ref playerScrew);

        UnityEngine.Debug.Log($"Block {blockId} clicked. Unlocked: {unlocked}. Player screw left: {playerScrew}");
        if (unlocked)
        {
            OnBlockUnlocked?.Invoke(blockId, true);
            return;
        }
    }

    internal void OnCellClicked(PuzzleCellUI cell)
    {

        if (cell == null || playerScrew <= 0) return;
        int blockId = cell.BlockId;
        //playerScrew--;
        cell.SetCellOn(false);// immediate visual feedback
        DataAPIController.instance.UpdateBlockCell(cell.BlockId, cell.id, true);
        // Enough tools: forward to logic which will attempt to unlock the block
        OnBlockClicked(blockId);
        OnScrewClick?.Invoke(playerScrew);
        return;
    }
}
