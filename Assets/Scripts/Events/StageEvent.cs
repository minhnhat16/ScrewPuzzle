using System;

public static class StageEvents
{
    // stageId, current, required
    public static Action<int, float, int> OnChestProgressChanged;

    // stage hoàn thành chest
    public static Action<int> OnStageCompleted;

    // stage được unlock
    public static Action<int> OnStageUnlocked;
}
