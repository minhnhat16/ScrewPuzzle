using System;
using System.Collections.Generic;

public static class ChestEvent
{
    // stage hoàn thành chest
    public static Action<int> OnChestOpened;

    // stage được unlock
    public static Action<int> OnChestUnlock;
}
