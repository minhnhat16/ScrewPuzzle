namespace Core.Match
{
    /// <summary>
    /// Đây là thứ DUY NHẤT thay đổi giữa các game:
    ///
    /// Screw game  → TagMatchRule (3 cùng màu)
    /// Cat game    → TagMatchRule (3 cùng loại cat)  ← cùng rule!
    /// Pair game   → TagMatchRule (2 cùng tag)       ← chỉ đổi RequiredCount
    /// Suit game   → SuitMatchRule (cùng suit bài)   ← rule khác
    /// </summary>
    public interface IMatchRule
    {
        /// <summary>Số item cần thiết để container hoàn thành</summary>
        int RequiredCount { get; }

        /// <summary>
        /// Item này có được phép vào container không?
        /// Check tag, trạng thái container, điều kiện đặc biệt...
        /// </summary>
        bool CanAccept(IMatchContainer container, IMatchItem item);

        /// <summary>Container đã hoàn thành chưa?</summary>
        bool IsComplete(IMatchContainer container);
    }
}