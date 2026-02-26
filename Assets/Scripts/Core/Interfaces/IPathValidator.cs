namespace Core.Match
{
    /// <summary>
    /// Kiểm tra item có thể đến được container không.
    ///
    /// Screw game → NoPathValidator (luôn true, screw bay thẳng)
    /// Cat game   → GridPathValidator (check đường đi trên grid)
    /// Ball game  → LineOfSightValidator (check không bị chặn)
    /// </summary>
    public interface IPathValidator
    {
        bool CanReach(IMatchItem item, IMatchContainer container);
    }
}