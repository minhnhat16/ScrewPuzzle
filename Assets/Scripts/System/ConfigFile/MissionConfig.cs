using Enums;
using System;
using UnityEngine;



[Serializable]
public class MissionConfigRecord
{
    [SerializeField]
    private int id;
    [SerializeField]
    private MissionType missionType;
    [SerializeField]
    private string description;
    [SerializeField]
    private string iconName;
    [SerializeField]
    private int target;
    [SerializeField]
    private ColorEnum color;
    [SerializeField]
    private ItemType rewardItemType;
    [SerializeField]
    private int rewardAmount;

    public int Id { get => id; set => id = value; }
    public MissionType MissionType { get => missionType; set => missionType = value; }
    public string Description { get => description; set => description = value; }
    public string IconName { get => iconName; set => iconName = value; }
    public int Target { get => target; set => target = value; }
    public ColorEnum Color { get => color; set => color = value; }
    public ItemType RewardItemType { get => rewardItemType; set => rewardItemType = value; }
    public int RewardAmount { get => rewardAmount; set => rewardAmount = value; }
}

[Serializable]
public class MissionConfig : BYDataTable<MissionConfigRecord>
{
    public override ConfigCompare<MissionConfigRecord> DefineConfigCompare()
    {
        return new ConfigCompare<MissionConfigRecord>("id");
    }
}
