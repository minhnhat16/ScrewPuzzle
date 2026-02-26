namespace Core.Match
{
    /// <summary>
    /// Match rule đơn giản nhất: item.Tag == container.AcceptedTag.
    /// Dùng được cho screw game, cat game, bất kỳ game nào match theo tag.
    ///
    /// Thay đổi duy nhất giữa các game là RequiredCount:
    ///   Screw/Cat → 3
    ///   Pair      → 2
    ///   Mahjong   → 1 (match là xoá ngay)
    /// </summary>
    public class TagMatchRule : IMatchRule
    {
        public int RequiredCount { get; }

        public TagMatchRule(int requiredCount = 3)
        {
            RequiredCount = requiredCount;
        }

        public bool CanAccept(IMatchContainer container, IMatchItem item)
        {
            if (container == null || item == null) return false;
            if (container.IsLocked || container.IsFull) return false;
            if (container.IsMoving) return false;

            return container.AcceptedTag == item.Tag;
        }

        public bool IsComplete(IMatchContainer container)
        {
            return container != null && container.Count >= RequiredCount;
        }
    }
}
