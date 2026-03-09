using Ingame;
using Ingame.Board;
using Ingame.Screw;
using System;
using UnityEngine;

/// <summary>
/// Pure C# service.
/// Dùng Func<ILayerManager> để resolve lazily tại call time —
/// tránh null vì layerManager chỉ được set SAU KHI pipeline hoàn tất.
/// </summary>
public class ScrewInteractionService : IScrewInteractionService
{
    // Lazy resolver — gọi mỗi lần HandleScrewSelected, không phải lúc construct
    private readonly Func<ILayerManager> _layerManagerResolver;
    private readonly IArrayScrew _arrayScrew;

    /// <param name="layerManagerResolver">Lambda trả về ILayerManager — resolved tại call time</param>
    /// <param name="arrayScrew">Hold queue cho screw</param>
    public ScrewInteractionService(Func<ILayerManager> layerManagerResolver, IArrayScrew arrayScrew)
    {
        _layerManagerResolver = layerManagerResolver ?? throw new ArgumentNullException(nameof(layerManagerResolver));
        _arrayScrew = arrayScrew ?? throw new ArgumentNullException(nameof(arrayScrew));
        Debug.Log("[ScrewInteractionService] Constructed with lazy ILayerManager resolver.");
    }

    public void HandleScrewSelected(ScrewController screw)
    {
        if (screw == null)
        {
            Debug.LogWarning("[ScrewInteractionService] screw is null.");
            return;
        }

        // Resolve tại call time — lúc này pipeline đã xong, layerManager đã được set
        var layerManager = _layerManagerResolver();
        if (layerManager == null)
        {
            Debug.LogError("[ScrewInteractionService] ILayerManager is null — pipeline chưa hoàn tất?");
            return;
        }

        layerManager.RemoveScrewOnDict(screw, screw.GetSortingOrder());
        _arrayScrew.AddScrew(screw);
    }
}