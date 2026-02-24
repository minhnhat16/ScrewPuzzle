using Ingame.Screw;

public interface ILayerManager
{
    void AddScrewToDict(ScrewController screw, int sortingOrder);
    void RemoveScrewOnDict(ScrewController screw, int sortingOrder);
}