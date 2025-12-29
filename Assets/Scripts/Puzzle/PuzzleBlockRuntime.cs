using System;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;

public class PuzzleBoardRuntime
{
    // key = blockId
    public Dictionary<int, BlockState> blocks
        = new Dictionary<int, BlockState>();

    // ===============================
    // STATE
    // ===============================
    public class BlockState
    {
        public int screwRequired;
        public bool isUnlocked;
        internal int removedCells;
    }

    // ===============================
    // API
    // ===============================
    public void RegisterBlock(int blockId, int screwRequired,int removedCells)
    {
        if (blocks.ContainsKey(blockId))
            return;

        blocks.Add(blockId, new BlockState
        {
            screwRequired = screwRequired,
            removedCells = removedCells,
            isUnlocked = false
        });
    }

    public bool IsUnlocked(int blockId)
    {
        return blocks.TryGetValue(blockId, out var b) && b.isUnlocked;
    }

    public bool TryUnlockByRemovingOneCell(int blockId, ref int playerScrew)
    {

        blocks.TryGetValue(blockId, out var block);
     
        if (block == null)
        {
            
            return false;

        }

        if (block.isUnlocked || playerScrew < 1)
            return false;

        playerScrew--;
        block.removedCells++;

        Debug.Log($"Remove cell {block.removedCells}, screw require  {block.screwRequired}");
        if (block.removedCells >= block.screwRequired)
        {
            block.isUnlocked = true;
            return true;
        }

        return false;
    }


    public void Clear()
    {
        blocks.Clear();
    }
    internal void OnCellClick(PuzzleCellUI cell, PuzzleBlock block, int currentTool)
    {
        // runtime is now data-agnostic: caller supplies currentTool.
        if (currentTool <= 0) return;

        if (cell == null) return;

        bool isOn = cell.IsOn;
        Debug.Log("Cell clicked " + cell.id + " isOn " + isOn);
        if (!isOn) return;

        cell.SetCellOn(false);
        if (block != null)
            block.OnCellRemoved(cell);
    }

    public bool TryUnlock(int blockId, ref int playerScrew)
    {
        if (!blocks.TryGetValue(blockId, out var block))
            return false;

        if (block.isUnlocked)
            return false;

        if (playerScrew < block.screwRequired)
            return false;
        Debug.Log($"Unlocking block {blockId} by spending {block.screwRequired} screws.");
        playerScrew -= block.screwRequired;
        block.isUnlocked = true;
        return true;
    }

}
