using Enums;
using System.Collections.Generic;

/// <summary>
/// Queries screw colors present in the top visible board layers.
/// Read-only — does NOT remove screws from the board.
/// </summary>
public interface ITopLayerScrewProvider
{
    /// <summary>
    /// Returns the set of colors that have at least one screw
    /// in the top <paramref name="layerDepth"/> layers of the board.
    /// </summary>
    HashSet<ColorEnum> GetTopLayerColors(int layerDepth = 2);
}