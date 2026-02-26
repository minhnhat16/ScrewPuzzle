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
            screwManagerGO.transform.localPosition = Vector3.zero;

            var screwManager = screwManagerGO.GetComponent<ScrewManager>();
            if (screwManager == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "ScrewManager component not found on prefab.";
                yield break;
            }

            ctx.ScrewManager = screwManager;

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