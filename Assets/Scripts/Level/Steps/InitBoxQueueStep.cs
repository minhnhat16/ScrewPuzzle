using Core.Match;
using Ingame.Board;
using LevelSystem.Core;
using System.Collections;

namespace LevelSystem.Steps
{
    /// <summary>
    /// Step 3: Khởi tạo BoxQueue với config của level hiện tại.
    /// Tách riêng để BoxQueue logic không lẫn vào loading logic.
    /// </summary>
    public class InitBoxQueueStep : ILevelLoadStep
    {
        public string StepName => "Init Box Queue";

        // Dùng interface thay vì BoxQueue.ins trực tiếp → DIP
        private readonly IContainerQueue _boxQueue;

        public InitBoxQueueStep(IContainerQueue boxQueue)
        {
            _boxQueue = boxQueue;
        }

        public IEnumerator Execute(LevelContext ctx)
        {
            _boxQueue.LoadBoxConfigRecord(ctx.LevelData.boxConfig);
            _boxQueue.Init();
            yield return null;
        }
    }
}