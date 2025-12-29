

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public enum ChestTier
{
    Common = 0,      // rương gỗ
    Rare = 1,        // rương hồng
    Epic = 2,        // rương thép
    Legendary = 3,   // rương vàng
    Special = 4      // rương đặc biệt cuối stage
}
[Serializable]
public class ChestRecord
{
    [SerializeField]
    private int id;
    [SerializeField]
    private List<RewardItem> rewards;
    [SerializeField]

    private int requiredProgress;

    [SerializeField]
    private ChestTier tier;

    public int Id { get => id; set => id = value; }
    public List<RewardItem> Rewards { get => rewards; set => rewards = value; }
    public ChestTier Tier { get => tier; set => tier = value; }
    public int RequiredProgress { get => requiredProgress; set => requiredProgress = value; }
}

public class ChestConfig : BYDataTable<ChestRecord>
{
    public override ConfigCompare<ChestRecord> DefineConfigCompare()
    {
        return new ConfigCompare<ChestRecord>("id");
    }
}
