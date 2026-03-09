using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Reads screw colors from the top N layers of the board.
/// Read-only — does not modify LayerManager state.
/// </summary>
public class TopLayerScrewProvider : ITopLayerScrewProvider
{
    private readonly LayerManager _layerManager;

    public TopLayerScrewProvider(LayerManager layerManager)
    {
        _layerManager = layerManager;
    }

    public HashSet<ColorEnum> GetTopLayerColors(int layerDepth = 2)
    {
        var result = new HashSet<ColorEnum>();
        if (_layerManager?.screwDict == null) return result;

        var targetLayers = _layerManager.screwDict.Keys
            .OrderBy(k => k)
            .Take(layerDepth);

        foreach (int layerIdx in targetLayers)
        {
            if (!_layerManager.screwDict.TryGetValue(layerIdx, out var screws) || screws == null)
                continue;

            foreach (var screw in screws)
            {
                if (screw != null && !screw.IsInHold)
                    result.Add(screw.GetColor());
            }
        }

        return result;
    }
}