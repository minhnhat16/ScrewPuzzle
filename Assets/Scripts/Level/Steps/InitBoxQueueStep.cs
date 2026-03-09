using LevelSystem.Core;
using System.Collections;

namespace LevelSystem.Steps
{
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
            if (ctx.LevelData.boxConfig == null)
            {
                ctx.IsSuccess = false;
                ctx.ErrorMessage = "BoxConfig is null — cannot init BoxQueue.";
                yield break;
            }

            // Reset trước khi load level mới — tránh state cũ từ level trước
            _boxQueue.OnReset();

            _boxQueue.LoadBoxConfigRecord(ctx.LevelData.boxConfig);
            _boxQueue.Initialize(isTutorial: false);
            yield return null;
        }
    }
}