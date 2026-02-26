using LevelSystem.Core;
using System.Collections;
using UnityEngine;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 6: Activate tất cả parts sau khi spawn xong.
    /// Đảm bảo Rigidbody2D và Collider được enable đúng thứ tự
    /// (không enable quá sớm trước khi hinge được setup).
    /// </summary>
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

            // Đợi 1 frame để physics settle sau khi spawn
            yield return new WaitForEndOfFrame();

            var parts = ctx.LayerManager.Parts;
            if (parts == null || parts.Count == 0)
            {
                Debug.LogWarning("[ActivatePartsStep] No parts to activate.");
                yield break;
            }

            foreach (var part in parts)
            {
                if (part == null) continue;
                part.gameObject.SetActive(true);
            }

            // Thêm 1 frame để Rigidbody2D nhận transform đúng
            yield return null;
        }
    }
}