using UnityEngine;

namespace Core.Match
{
    /// <summary>
    /// Screw game — không có pathfinding, item bay thẳng vào container.
    /// </summary>
    public class NoPathValidator : IPathValidator
    {
        public bool CanReach(IMatchItem item, IMatchContainer container) => true;
    }

    /// <summary>
    /// Cat game, Ball game — check đường đi trên grid không bị chặn.
    /// Inject IPathfinder cụ thể tùy game.
    /// </summary>
    public class GridPathValidator : IPathValidator
    {
        public interface IPathfinder
        {
            bool HasClearPath(Vector3 from, Vector3 to);
        }

        private readonly IPathfinder _pathfinder;

        public GridPathValidator(IPathfinder pathfinder)
        {
            _pathfinder = pathfinder;
        }

        public bool CanReach(IMatchItem item, IMatchContainer container)
        {
            if (_pathfinder == null) return false;
            return _pathfinder.HasClearPath(item.Position, container.Position);
        }
    }
}