using Enums;
using Ingame;
using System;
using System.Collections.Generic;

public interface IBoxSequenceService
{
    void Load(IEnumerable<Box> boxes);

    /// <summary>Lấy box tiếp theo theo thứ tự.</summary>
    Box GetNext();

    bool HasNext();

    /// <summary>
    /// Thử lấy box đầu tiên trong queue thỏa <paramref name="predicate"/>.
    /// Nếu tìm thấy: xóa khỏi queue và trả về box đó.
    /// Nếu không: trả về null và queue không thay đổi.
    /// </summary>
    Box TryDequeueMatching(Func<Box, bool> predicate);

    /// <summary>
    /// Query số lượng box theo từng màu trong toàn bộ list — không dequeue.
    /// Dùng để quyết định spawn box màu nào trước.
    /// </summary>
    Dictionary<ColorEnum, int> GetColorCounts();
    int RemoveByColor(ColorEnum targetColor, int count);

    List<Box> GetAllBox();
    void ReturnToFront(Box smart);
}