using System;
using System.Diagnostics;

public class PuzzleBoardLogic
{
    private PuzzleBoardRuntime runtime;
    private int playerScrew =100;

    public Action<int,bool> OnBlockUnlocked;

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
            OnBlockUnlocked?.Invoke(blockId,true);
            return;
        }
    }
}
