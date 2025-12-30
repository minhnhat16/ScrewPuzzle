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


    public void LoadFromSave(List<BlockParam> saved)
    {
        blocks.Clear(); // ❗ BẮT BUỘC

        if (saved == null) return;

        foreach (var p in saved)
        {
            blocks[p.blockId] = new BlockState
            {
                screwRequired = p.screwRequired,
                removedCells = p.removedCells.Count,
                isUnlocked = p.unlocked
            };
        }
    }
    // ===============================
    // API
    // ===============================
    public void RegisterBlock(int blockId, int screwRequired,int removedCell)
    {
        if (blocks.ContainsKey(blockId))
        {
            return; // đã load từ save rồi
        }

        blocks[blockId] = new BlockState
        {
            screwRequired = screwRequired,
            removedCells = removedCell,
            isUnlocked = false
        };
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
