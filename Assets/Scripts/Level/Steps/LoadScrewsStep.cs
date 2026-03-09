using Ingame;
using LevelSystem.Core;
using System.Collections;
using UnityEngine;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 5: Spawn ScrewManager và tất cả screw vào scene.
    /// Delegate spawn cho IScrewSpawnService.
    /// </summary>
    public class LoadScrewsStep : ILevelLoadStep
    {
        public string StepName => "Load Screws";

        private readonly GameObject _screwManagerPrefab;
        private readonly IScrewSpawnService _screwSpawnService;


        private static Vector3 screwManagerPosition = new Vector3(0,-1,0);

        public LoadScrewsStep(GameObject screwManagerPrefab, IScrewSpawnService screwSpawnService)
        {
            _screwManagerPrefab = screwManagerPrefab;
            _screwSpawnService = screwSpawnService;
        }

        public IEnumerator Execute(LevelContext ctx)
        {
            if (_screwManagerPrefab == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "ScrewManager prefab is null in LoadScrewsStep.";
                yield break;
            }

            // Spawn ScrewManager container
            var screwManagerGO = Object.Instantiate(
                _screwManagerPrefab,
                ctx.LevelObject.transform
            );
            screwManagerGO.transform.localPosition = screwManagerPosition;

            if (!screwManagerGO.TryGetComponent<ScrewManager>(out var screwManager))
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "ScrewManager component not found on prefab.";
                yield break;
            }

            ctx.ScrewManager = screwManager;

            // ── Set vào LevelManager NGAY ĐÂY ──────────────────────
            // HingeController.Start() → InitHingeJoints() → LevelManager.ins.ScrewManager
            // được gọi trong SpawnScrews() bên dưới — phải set trước khi spawn
            if (LevelManager.ins != null)
            {
                LevelManager.ins.ScrewManager = screwManager;
                Debug.Log("[LoadScrewsStep] ScrewManager set vào LevelManager trước khi spawn screws.");
            }
            else
            {
                Debug.LogWarning("[LoadScrewsStep] LevelManager.ins is null — HingeController sẽ không register được.");
            }

            // Spawn individual screws
            yield return _screwSpawnService.SpawnScrews(
                ctx.LevelData,
                ctx.LayerManager,
                screwManager,
                screwManagerGO.transform
            );
        }
    }
}