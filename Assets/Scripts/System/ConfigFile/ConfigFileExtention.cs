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
}