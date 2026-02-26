using UnityEngine;

namespace Core.Match
{
    /// <summary>
    /// Bộ não của routing: quyết định item có vào được container không.
    /// Dùng IMatchRule để check tag/rule, IPathValidator để check đường đi.
    ///
    /// Screw game: inject TagMatchRule + NoPathValidator
    /// Cat game:   inject TagMatchRule + GridPathValidator
    ///
    /// Class này KHÔNG biết gì về MonoBehaviour, animation, hay Unity cụ thể.
    /// Hoàn toàn testable.
    /// </summary>
    public class MatchRouter
    {
        private readonly IMatchRule _rule;
        private readonly IPathValidator _pathValidator;
        private readonly IContainerQueue _containerQueue;

        public MatchRouter(
            IMatchRule rule,
            IPathValidator pathValidator,
            IContainerQueue containerQueue)
        {
            _rule = rule;
            _pathValidator = pathValidator;
            _containerQueue = containerQueue;
        }

        /// <summary>
        /// Kết quả routing: item sẽ đi đâu.
        /// </summary>
        public enum RouteResult
        {
            RoutedToContainer,  // item đã được add vào container
            HoldInQueue,        // không có container phù hợp → giữ trong slot
            Blocked,            // path bị chặn → giữ trong slot
            Rejected,           // item null hoặc lỗi
        }

        /// <summary>
        /// Thử route item vào container phù hợp.
        /// Caller (TempQueue) dùng RouteResult để quyết định tiếp theo.
        /// </summary>
        public RouteResult TryRoute(IMatchItem item, out IMatchContainer targetContainer)
        {
            targetContainer = null;

            if (item == null)
            {
                Debug.LogWarning("[MatchRouter] item null.");
                return RouteResult.Rejected;
            }

            // Tìm container theo tag
            targetContainer = _containerQueue.FindSuitable(item.Tag);

            if (targetContainer == null)
                return RouteResult.HoldInQueue;

            // Check rule (tag match + không full + không lock...)
            if (!_rule.CanAccept(targetContainer, item))
                return RouteResult.HoldInQueue;

            // Check đường đi
            if (!_pathValidator.CanReach(item, targetContainer))
                return RouteResult.Blocked;

            // Add vào container
            bool added = _containerQueue.AddItemToContainer(item, targetContainer);

            return added ? RouteResult.RoutedToContainer : RouteResult.HoldInQueue;
        }
    }
}