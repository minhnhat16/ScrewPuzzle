using Ingame;
using LevelSystem.Core;
using System.Collections;
using UnityEngine;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step cuối activate: SetActive(true) tất cả parts rồi chuyển sang Dynamic.
    /// </summary>
    public class ActivatePartStep : ILevelLoadStep
    {
        public string StepName => "Activate Part — Set Dynamic";

        public IEnumerator Execute(LevelContext ctx)
        {
            if (ctx.LayerManager == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "LayerManager is null in ActivatePartStep.";
                yield break;
            }

            var parts = ctx.LayerManager.Parts;
            if (parts == null || parts.Count == 0)
            {
                Debug.LogWarning("[ActivatePartStep] No parts found.");
                yield break;
            }

            // Frame 1: Awake/Start/OnEnable + transform settle
            yield return null;

            // Frame 2: HingeJoint2D autoConfigureConnectedAnchor tính anchor
            yield return null;

            // Chuyển tất cả sang Dynamic — anchor đã đúng, gravityScale=0
            LayerUtils.SetAllDynamic(parts);

            Debug.Log($"[ActivatePartStep] Set {parts.Count} parts to Dynamic.");
        }
    }
}