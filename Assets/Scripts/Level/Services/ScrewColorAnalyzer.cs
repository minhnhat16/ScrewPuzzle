using Level;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Phân tích phân phối màu của screws trong một Level.
/// Dùng để:
///  - Validate level (số screw mỗi màu phải chia hết cho 3)
///  - Tính LevelDifficulty trong LevelManager
///  - Debug report trong editor
/// </summary>
public class ScrewColorAnalyzer
{
    /// <summary>
    /// Trả về dict: key = idColor (int), value = số lượng screw có màu đó.
    /// </summary>
    public Dictionary<int, int> GetColorCount(Level.Level level)
    {
        var result = new Dictionary<int, int>();

        if (level?.screws == null) return result;

        foreach (var screw in level.screws)
        {
            if (screw == null) continue;

            if (!result.ContainsKey(screw.idColor))
                result[screw.idColor] = 0;

            result[screw.idColor]++;
        }

        return result;
    }

    /// <summary>
    /// Log báo cáo: mỗi màu có bao nhiêu screw, chia hết cho 3 không.
    /// Dùng trong editor tool / LevelManager.LogScrewColorReport().
    /// </summary>
    public void LogDivisibilityReport(Level.Level level)
    {
        if (level == null)
        {
            Debug.LogWarning("[ScrewColorAnalyzer] Level is null.");
            return;
        }

        var colorCount = GetColorCount(level);
        var sb = new StringBuilder();
        sb.AppendLine($"[ScrewColorAnalyzer] Level {level.levelId} — {colorCount.Count} colors:");

        bool allValid = true;

        foreach (var kv in colorCount)
        {
            bool divisible = kv.Value % 3 == 0;
            if (!divisible) allValid = false;

            string status = divisible ? "✓" : "✗ (NOT divisible by 3)";
            sb.AppendLine($"  Color {kv.Key}: {kv.Value} screws {status}");
        }

        sb.AppendLine(allValid ? "  → Level is VALID ✓" : "  → Level has INVALID color counts ✗");
        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// Kiểm tra nhanh level có hợp lệ không (tất cả màu đều chia hết cho 3).
    /// </summary>
    public bool IsLevelValid(Level.Level level)
    {
        var colorCount = GetColorCount(level);
        foreach (var count in colorCount.Values)
        {
            if (count % 3 != 0) return false;
        }
        return true;
    }
}