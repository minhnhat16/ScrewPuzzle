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
            var screwManager = LevelManager.ins.ScrewManager;
            screwManager.transform.localPosition = screwManagerPosition;

            if (!screwManager)
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
                screwManager.transform
            );
        }
    }
}