using LevelSystem.Core;
using System.Collections;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 3: Khởi tạo BoxQueue với config của level hiện tại.
    ///
    /// Dùng ILevelBoxQueue thay vì IContainerQueue để gọi được
    /// LoadBoxConfigRecord() mà không cần cast hay dùng BoxQueue.ins.
    /// </summary>
    public class InitBoxQueueStep : ILevelLoadStep
    {
        public string StepName => "Init Box Queue";

        private readonly ILevelBoxQueue _boxQueue;

        public InitBoxQueueStep(ILevelBoxQueue boxQueue)
        {
            _boxQueue = boxQueue;
        }

        public IEnumerator Execute(LevelContext ctx)
        {
            if (ctx.LevelData?.boxConfig == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "BoxConfig is null — cannot init BoxQueue.";
                yield break;
            }

            _boxQueue.LoadBoxConfigRecord(ctx.LevelData.boxConfig);
            _boxQueue.Initialize(isTutorial: false);

            yield return null;
        }
    }
}