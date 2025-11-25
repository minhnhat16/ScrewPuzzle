using Enums;
using Ingame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SideMissionManager : SingletonMono<SideMissionManager>
{
    SideMission currentMission;
    /// <summary>
    /// Tạo nhiệm vụ 3 vít cùng màu khi level load xong
    /// </summary>
    public SideMission GenerateColorMission(Level.Level level,BoxQueue queue,int require = 3)
    {
        // Đếm số lượng screw theo màu
        var colorCount = level.screws
            .GroupBy(s => s.idColor)
            .ToDictionary(g => g.Key, g => g.Count());

        // Tìm các màu có ít nhất 3 screw
        var validColors = colorCount
            .Where(p => p.Value >= 3)
            .Select(p => p.Key)
            .ToList();

        if (validColors.Count == 0)
        {
            Debug.Log("No color with enough screws (>=3) for side mission.");
            return null;
        }

        // Random chọn 1 màu
        int targetColor = validColors[Random.Range(0, validColors.Count)];

        SideMission mission = new()
        {
            targetColorID = targetColor,
            requiredCount = require,
            currentCount = 0
        };

        currentMission = mission;
        Debug.Log($"Side mission created: Unscrew 3 screws of color {targetColor}");
        BoxQueue.ins.RemoveBoxByColor((ColorEnum)targetColor,require/3);
        return mission;
    }
}

public class SideMission
{
    public int targetColorID;   // ID màu
    public int requiredCount;   // số vít cần gỡ
    public int currentCount;    // tiến độ hiện tại
}
