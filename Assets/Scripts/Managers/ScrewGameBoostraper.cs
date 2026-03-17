using Core.Match;
using DG.Tweening;
using Ingame;
using UnityEngine;

namespace Managers
{
    public class ScrewGameBootstrapper : SingletonMono<ScrewGameBootstrapper>
    {
        [Header("References")]
        [SerializeField] private ArrayScrew arrayScrew;
        [SerializeField] private BoxQueue boxQueue;
        [SerializeField] private ScrewManager screwManager;
        [SerializeField] private Player player;
        [SerializeField] private Ease boxMovingEse;

        [Header("Editor Dev Boot")]
        [Tooltip("Level ID để auto-load khi Play trực tiếp từ InGame scene (Editor only).")]
        [SerializeField] private int devBootLevelId = 1;
        [Tooltip("Bật để auto-bootstrap khi Play từ InGame scene mà không qua MainScreen.")]
        [SerializeField] private bool autoBootInEditor = true;

        public void InitializeForLevel()
        {
            if (arrayScrew == null || boxQueue == null || screwManager == null || player == null)
            {
                Debug.LogError("[ScrewGameBootstrapper] Missing references. Please assign all fields in the Inspector.");
                return;
            }

            IMatchRule rule = new TagMatchRule(requiredCount: 3);
            IPathValidator path = new NoPathValidator();
            IContainerQueue containers = boxQueue;
            ILevelBoxQueue levelBox = boxQueue;

            // ── ArrayScrew ──────────────────────────────────────
            var router = new MatchRouter(rule, path, containers);
            screwManager.ValidateMaps();
            arrayScrew.Inject(router, screwManager, containers, player);
            Debug.Log("[ScrewGameBootstrapper] Injected ArrayScrew");

            // ── LevelManager ────────────────────────────────────
            LevelManager.ins.Inject(levelBox);
            Debug.Log("[ScrewGameBootstrapper] Injected LevelManager");

            // ── SideMissionManager ──────────────────────────────
            SideMissionManager.ins.Inject(containers);
            Debug.Log("[ScrewGameBootstrapper] Injected SideMissionManager");

            // ── BoxQueue Setup ──────────────────────────────────
            var factory = new BoxFactory();
            var sequence = new BoxSequenceService();
            var layout = new BoxSlotLayoutService(boxMovingEse);
            boxQueue.Setup(factory, sequence, layout);
            Debug.Log("[ScrewGameBootstrapper] Setup BoxQueue services");

            // ── BoxQueue ← ArrayScrew ───────────────────────────
            boxQueue.SetArrayScrew(arrayScrew);
            Debug.Log("[ScrewGameBootstrapper] SetArrayScrew into BoxQueue " + (arrayScrew == null));

            // ── ScrewInteractionService → Player ─────────────────
            var screwService = new ScrewInteractionService(
                () => LevelManager.ins.layerManager as ILayerManager,
                arrayScrew as IArrayScrew
            );
            player.Inject(screwService);
            Debug.Log("[ScrewGameBootstrapper] Injected ScrewInteractionService into Player");
        }
    }
}