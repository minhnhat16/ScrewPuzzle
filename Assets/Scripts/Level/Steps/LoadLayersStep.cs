using Ingame.Board;
using LevelSystem.Core;
using System.Collections;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 4: Spawn tất cả layer và part vào scene.
    /// Delegate thực thi cho IPartSpawnService — step chỉ điều phối.
    /// </summary>
    public class LoadLayersStep : ILevelLoadStep
    {
        public string StepName => "Load Layers";

        private readonly IPartSpawnService _partSpawnService;

        public LoadLayersStep(IPartSpawnService partSpawnService)
        {
            _partSpawnService = partSpawnService;
        }

        public IEnumerator Execute(LevelContext ctx)
        {
            if (ctx.LevelData == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "LevelData is null in LoadLayersStep.";
                yield break;
            }

            if (ctx.LayerManager == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "LayerManager is null in LoadLayersStep.";
                yield break;
            }

            yield return _partSpawnService.SpawnLayers(ctx.LevelData, ctx.LayerManager);
        }
    }
}