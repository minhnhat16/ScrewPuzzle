using LevelSystem.Core;
using System.Collections;
using System.Collections.Generic;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 2: Lấy LevelData từ dictionary đã load sẵn.
    /// Tách riêng để dễ swap nguồn data (local, remote, addressables).
    /// </summary>
    public class ResolveLevelDataStep : ILevelLoadStep
    {
        public string StepName => "Resolve Level Data";

        private readonly Dictionary<string, Level.Level> _levels;

        public ResolveLevelDataStep(Dictionary<string, Level.Level> levels)
        {
            _levels = levels;
        }

        public IEnumerator Execute(LevelContext ctx)
        {
            if (!_levels.TryGetValue(ctx.LevelId.ToString(), out var levelData))
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = $"Level ID {ctx.LevelId} not found in loaded levels.";
                yield break;
            }

            ctx.LevelData = levelData;
            yield return null;
        }
    }
}