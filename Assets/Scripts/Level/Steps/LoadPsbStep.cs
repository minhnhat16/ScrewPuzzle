using LevelSystem.Core;
using System.Collections;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 0 (tr??c LoadLayers): ??m b?o PSB sprites c?a level hi?n t?i ?ã ???c load.
    /// Dùng PsbSlidingWindowLoader — ch? load c?n thi?t, unload level c?.
    /// </summary>
    public class LoadPsbStep : ILevelLoadStep
    {
        public string StepName => "Load PSB Sprites";

        public IEnumerator Execute(LevelContext ctx)
        {
            yield return PsbSlidingWindowLoader.ins.EnsureLoaded(ctx.LevelId);
        }
    }
}