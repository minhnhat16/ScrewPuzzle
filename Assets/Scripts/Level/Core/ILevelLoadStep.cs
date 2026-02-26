using System.Collections;

namespace LevelSystem.Core
{
    /// <summary>
    /// Interface cho mỗi bước trong pipeline load level.
    /// Tuân theo OCP: thêm bước mới chỉ cần implement interface này,
    /// không cần sửa code cũ.
    /// </summary>
    public interface ILevelLoadStep
    {
        /// <summary>Tên step để debug log</summary>
        string StepName { get; }

        /// <summary>
        /// Thực thi step. Set ctx.IsSuccess = false nếu thất bại.
        /// </summary>
        IEnumerator Execute(LevelContext ctx);
    }
}