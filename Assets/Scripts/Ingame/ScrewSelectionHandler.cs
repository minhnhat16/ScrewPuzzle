using Ingame;
using Ingame.Board;
using Ingame.Screw;
using UnityEngine;

public class ScrewSelectionHandler : MonoBehaviour
{
    [SerializeField] private ArrayScrew arrayScrew;
    [SerializeField] private LayerManager layerManager;
    [SerializeField] private Player Player;
    private void OnEnable() => Player.OnScrewSelected += HandleScrewSelected;
    private void OnDisable() => Player.OnScrewSelected -= HandleScrewSelected;

    private void HandleScrewSelected(ScrewController screw)
    {
        layerManager.RemoveScrewOnDict(screw, screw.GetSortingOrder());
        arrayScrew.Enqueue(screw);
    }
}