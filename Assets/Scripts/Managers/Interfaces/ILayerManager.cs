using Ingame.Screw;

public interface ILayerManager
{
    void AddScrewToDict(ScrewController screw, int sortingOrder);
    void RemoveScrew(ScrewController screw);
}