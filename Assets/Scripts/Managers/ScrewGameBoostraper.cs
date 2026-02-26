using Core.Match;
using Ingame;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Wire toàn bộ dependencies cho screw game.
    /// Đây là nơi DUY NHẤT biết các class cụ thể — tất cả chỗ khác chỉ thấy interface.
    /// </summary>
    public class ScrewGameBootstrapper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArrayScrew arrayScrew;
        [SerializeField] private BoxQueue boxQueue;
        [SerializeField] private ScrewManager screwManager;

        private void Awake()
        {
            IMatchRule rule = new TagMatchRule(requiredCount: 3);
            IPathValidator path = new NoPathValidator();
            IContainerQueue containers = boxQueue;  // BoxQueue implement IContainerQueue

            var router = new MatchRouter(rule, path, containers);

            // Inject ArrayScrew
            arrayScrew.Inject(router, screwManager, containers);

            // Inject SideMissionManager — không còn FindAnyObjectByType
            SideMissionManager.ins.Inject(containers);
        }
    }
}