using Ingame;
using Ingame.Board;
using Ingame.Screw;
using UnityEngine;

/// <summary>
/// Bridge giữa Player.OnScrewSelected và ScrewInteractionService.
/// Chỉ dùng khi cần hook thêm logic bên ngoài (VD: tutorial, analytics).
/// Trong flow thông thường, Player đã tự gọi ScrewInteractionService
/// qua HandleTappableTapped — không cần class này.
/// </summary>
public class ScrewSelectionHandler : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private LayerManager layerManager;

    private IScrewInteractionService _screwService;

    public void Inject(IScrewInteractionService screwService)
    {
        _screwService = screwService;
    }

    private void OnEnable() => player.OnScrewSelected += HandleScrewSelected;
    private void OnDisable() => player.OnScrewSelected -= HandleScrewSelected;

    private void HandleScrewSelected(ScrewController screw)
    {
        if (_screwService == null)
        {
            Debug.LogWarning("[ScrewSelectionHandler] _screwService chưa được inject. Bỏ qua.");
            return;
        }

        // Delegate toàn bộ cho ScrewInteractionService:
        // RemoveScrewOnDict + MatchRouter.TryRoute (route thẳng vào box nếu cùng màu)
        _screwService.HandleScrewSelected(screw);
    }
}