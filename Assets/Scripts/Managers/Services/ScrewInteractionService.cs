using Ingame.Board;
using Ingame.Screw;

public class ScrewInteractionService : IScrewInteractionService
{
    private readonly ILayerManager _layerManager;
    private readonly IArrayScrew _arrayScrew;

    public ScrewInteractionService(
        ILayerManager layerManager,
        IArrayScrew arrayScrew)
    {
        _layerManager = layerManager;
        _arrayScrew = arrayScrew;
    }

    public void HandleScrewSelected(ScrewController screw)
    {
        _layerManager.RemoveScrewOnDict(screw, screw.GetSortingOrder());
        _arrayScrew.AppendScrew(screw);
    }
}