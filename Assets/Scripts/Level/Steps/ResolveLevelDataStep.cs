using LevelSystem.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 2: Lấy LevelData từ dictionary đã load sẵn.
    /// Tách riêng để dễ swap nguồn data (local, remote, addressables).
    /// Includes validation that levels dict is not empty before searching.
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
            // Validate input
            if (_levels == null || _levels.Count == 0)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = $"Level data dictionary is empty or null. Cannot resolve Level ID {ctx.LevelId}. " +
                                   "Ensure levels are loaded before starting pipeline.";
                Debug.LogError($"[ResolveLevelDataStep] {ctx.ErrorMessage}");
                yield break;
            }

            // Try to find the level
            if (!_levels.TryGetValue(ctx.LevelId.ToString(), out var levelData))
            {
                // Log available IDs for debugging
                var availableIds = string.Join(", ", _levels.Keys);
                ctx.IsSuccess = false;
                ctx.ErrorMessage = $"Level ID {ctx.LevelId} not found in loaded levels. " +
                                   $"Available IDs: [{availableIds}]";
                Debug.LogError($"[ResolveLevelDataStep] {ctx.ErrorMessage}");
                yield break;
            }

            if (levelData == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = $"Level ID {ctx.LevelId} found but is null.";
                yield break;
            }

            ctx.LevelData = levelData;
            Debug.Log($"[ResolveLevelDataStep] Resolved Level ID {ctx.LevelId}: {levelData.name}");
            yield return null;
        }
    }
}