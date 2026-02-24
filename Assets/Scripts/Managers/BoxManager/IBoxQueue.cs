using ConfigFile;
using Ingame;
using Ingame.Screw;
using System;
using System.Collections.Generic;

public interface IBoxQueue
{
    int ActiveBoxCount { get; }
    event Action<Box> OnBoxFull;
    event Action<Box> OnBoxSpawned;
    event Action<Box> OnBoxRemoved;
    event Action<SideMission> OnSpecialModeStarted;
    void LoadLevelBoxes(List<BoxConfigRecord> records);
    void Initialize(bool isTutorial);
    void ResetQueue();
    void NotifyBoxFull(Box box);
    void EnableSpecialMode(SideMission mission);
    void ProcessScrews(IEnumerable<ScrewController> screws);
    void UnlockNextBox();
}