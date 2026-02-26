using Core.Match;
using Ingame.Board;
using LevelSystem.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step cuối trong pipeline load level.
    ///
    /// Trách nhiệm:
    ///  1. Build layerQueue từ LayerManager.Layers
    ///  2. Gọi LayerVisibilityController.Init(queue) để setup visibility
    ///  3. Log summary
    ///
    /// KHÔNG làm: spawn, physics, boxqueue — đó là việc của các step trước.
    /// </summary>
    public class FinalizeStep : ILevelLoadStep
    {
        public string StepName => "Finalize";

        private readonly IContainerQueue _boxQueue;

        public FinalizeStep(IContainerQueue boxQueue)
        {
            _boxQueue = boxQueue;
        }

        public IEnumerator Execute(LevelContext ctx)
        {
            // Buffer frame để mọi spawn settle xong
            yield return null;

            var lm = ctx.LayerManager;
            if (lm == null)
            {
                Debug.LogWarning("[FinalizeStep] LayerManager is null — skipping visibility init.");
                yield break;
            }

            var visCtrl = lm.visibilityController;
            if (visCtrl == null)
            {
                visCtrl = ctx.LevelObject?.GetComponentInChildren<LayerVisibilityController>(true);
                if (visCtrl != null)
                    lm.visibilityController = visCtrl;
            }

            if (visCtrl != null)
            {
                var queue = new Queue<BaseLayer>(lm.Layers ?? new List<BaseLayer>());

                visCtrl.Init(queue);
            }
            else
            {
                Debug.LogWarning("[FinalizeStep] LayerVisibilityController not found — layers will not have visibility applied.");
            }

            Debug.Log($"[FinalizeStep] Level {ctx.LevelId} ready. " +
                      $"Layers: {lm.Layers?.Count ?? 0} | " +
                      $"Parts: {lm.Parts?.Count ?? 0} | " +
                      $"Screws: {ctx.ScrewManager?.Screws?.Count ?? 0}");
        }
    }
}