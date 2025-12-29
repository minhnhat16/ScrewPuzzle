using ConfigFile;
using Enums;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ConfigExtensions
{
    public static Color GetColor(this ConfigFileManager cfg, ColorEnum colorId)
    {
        var colorConfig = cfg.GetConfig<ColorConfig>();
        return colorConfig.GetRecordByKeySearch(colorId).Color;
    }

    internal static List<PackConfigRecord> GetAllPackConfig(this ConfigFileManager cfg)
    {
        return cfg.GetConfig<PackConfig>().GetAllRecord();
    }

    public static ItemConfigRecord GetItemConfig(this ConfigFileManager cfg, ItemType type)
    {
        var itemConfig =  cfg.GetConfig<ItemConfig>();
        return itemConfig.GetRecordByKeySearch(type);
    }

    public static RewardConfigRecord GetRewardConfig(this ConfigFileManager cfg, int id)
    {
        var rewardConfig = cfg.GetConfig<RewardConfig>();
        return rewardConfig.GetRecordByKeySearch(id);
    }

    public static List<MissionConfigRecord> GetMissionsByID(this ConfigFileManager cfg, List<int> idMissions)
    {
        var missions = cfg.GetConfig<MissionConfig>().GetAllRecord();
        var missionMatch = missions
            .FindAll(m => idMissions.Contains(m.Id));
        return missionMatch;
    }

    internal static int GetStageCount()
    {
        return 5;
    }

    internal static PuzzlePartenRecord GetPartenBy(this ConfigFileManager cfg,int id)
    {
        var parten = cfg.GetConfig<PuzzleParternConfig>().GetRecordByKeySearch(id);
        return parten;
    }

    internal static List<PuzzleCellRecord> GetAllPuzzleCellConfig(this ConfigFileManager cfg)
    {
        var cells = cfg.GetConfig<PuzzleCellConfig>().GetAllRecord();
        return cells;
    }
}