using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelSystem.Core
{
    /// <summary>
    /// Pipeline điều phối các bước load level theo thứ tự.
    /// LevelManager chỉ cần gọi pipeline này, không cần biết bên trong có gì.
    /// </summary>
    public class LevelLoadPipeline
    {
        private readonly List<ILevelLoadStep> _steps = new();

        public LevelLoadPipeline AddStep(ILevelLoadStep step)
        {
            _steps.Add(step);
            return this; // fluent API để chain
        }

        public IEnumerator Run(LevelContext ctx, Action onComplete = null)
        {
            foreach (var step in _steps)
            {
                Debug.Log($"[Pipeline] Running: {step.StepName}");
                yield return step.Execute(ctx);

                if (!ctx.IsSuccess)
                {
                    Debug.LogError($"[Pipeline] Step '{step.StepName}' failed: {ctx.ErrorMessage}");
                    yield break;
                }

                Debug.Log($"[Pipeline] Done: {step.StepName}");
            }

            onComplete?.Invoke();
        }
    }
}
