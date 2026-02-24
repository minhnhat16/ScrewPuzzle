using Ingame.Screw;
using System;

public interface IPlayer
{
    bool IsInputLocked { get; }

    void LockInput();
    void UnlockInput();

    event Action<ScrewController> OnScrewSelected;
}