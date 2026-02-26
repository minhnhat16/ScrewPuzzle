using Core.Match;
using Ingame;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Wire toàn bộ dependencies cho screw game.
    /// Đây là nơi DUY NHẤT biết các class cụ thể.
    /// Tất cả chỗ khác chỉ thấy interface.
    ///
    /// Inject order:
    ///  1. BoxQueue → ArrayScrew  (IContainerQueue)
    ///  2. BoxQueue → LevelManager (ILevelBoxQueue)  ← NEW
    ///  3. BoxQueue → SideMissionManager (IContainerQueue)
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
            IContainerQueue containers = boxQueue;    // BoxQueue implement IContainerQueue
            ILevelBoxQueue levelBox = boxQueue;    // BoxQueue implement ILevelBoxQueue

            // ── ArrayScrew ──────────────────────────────────────
            var router = new MatchRouter(rule, path, containers);
            arrayScrew.Inject(router, screwManager, containers);

            // ── LevelManager ────────────────────────────────────
            // Inject trước Init() và LoadLevel() được gọi
            LevelManager.ins.Inject(levelBox);

            // ── SideMissionManager ──────────────────────────────
            SideMissionManager.ins.Inject(containers);
        }
    }
}