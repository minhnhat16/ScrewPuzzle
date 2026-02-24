using System.Collections;
using UnityEngine;

public class InitMissionTask : IBootTask
{
    public string Name => "InitMission";

    public IEnumerator Execute()
    {
        Debug.Log("[BOOT] InitMission...");
        bool done = false;
        yield return MissionManager.ins.Init(() => done = true);
        yield return new WaitUntil(() => done);
    }
}