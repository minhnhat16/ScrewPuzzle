using System;
using System.Collections.Generic;

public static class MissionEvents
{
    public static Action<MissionConfigRecord, MissionProgress> OnMissionProgressChanged;
    public static Action<MissionConfigRecord> OnMissionCompleted;
    public static Action<MissionConfigRecord> OnMissionClaimed;
    public static Action<List<MissionConfigRecord>> OnActiveMissionChanged;
}
