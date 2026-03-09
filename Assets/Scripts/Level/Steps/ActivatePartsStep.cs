using Ingame;
using Ingame.Board;
using LevelSystem.Core;
using System.Collections;
using UnityEngine;

namespace LevelSystem.Steps
{
    public class ActivatePartsStep : ILevelLoadStep
    {
        public string StepName => "Activate Parts";

        public IEnumerator Execute(LevelContext ctx)
        {
            if (ctx.LayerManager == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "LayerManager is null in ActivatePartsStep.";
                yield break;
            }

            var lm = ctx.LayerManager;
            var parts = lm.Parts;

            if (parts == null || parts.Count == 0)
            {
                Debug.LogWarning("[ActivatePartsStep] No parts to activate.");
                yield break;
            }

            var prevSimMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;

            LayerUtils.SetAllKinematic(parts);

            foreach (var part in parts)
            {
                if (part == null) continue;
                part.gameObject.SetActive(true);
            }

            if (lm.Layers != null)
            {
                for (int i = 0; i < lm.Layers.Count; i++)
                    LayerUtils.ActiveObjectInLayer(isOn: true, layer: i, lm: lm);
            }

            yield return null;
            LayerUtils.SetAllDynamic(parts);

            Physics2D.simulationMode = prevSimMode;

            // ── Đăng ký listener SAU KHI parts active + hinge đã settle ──
            // RegisterPartListener() phải gọi sau tất cả setup để tránh
            // OnStateChanged firing sớm trong quá trình physics warm-up
            if (lm.Layers != null)
            {
                foreach (var layer in lm.Layers)
                {
                    if (layer == null) continue;
                    layer.RegisterPartListener();
                    Debug.Log($"[ActivatePartsStep] RegisterPartListener → layer '{layer.name}' " +
                              $"({layer.parts.Count} parts)");
                }
            }

            Debug.Log($"[ActivatePartsStep] Activated {parts.Count} parts across {lm.Layers?.Count ?? 0} layers.");
        }
    }
}