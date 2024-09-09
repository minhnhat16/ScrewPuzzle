using System;
using UnityEngine;



[Serializable]
public class DailyRewardConfigRecord
{
    [SerializeField]
    private int id;
    [SerializeField]
    private string spriteName;
    [SerializeField]
    private IEDailyType type;
    [SerializeField]
    private DailyReward itemName;
    [SerializeField]
    private int totalItem;
    public int ID { get { return id; } }
    public string SpriteName { get { return spriteName; } }
    public IEDailyType Type { get { return type; } }
    public DailyReward ItemName { get { return itemName; } }
    public int TotalItem { get { return totalItem; } }
}
public class DailyRewardConfig : BYDataTable<DailyRewardConfigRecord>
{
    public override ConfigCompare<DailyRewardConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<DailyRewardConfigRecord>("id");
        return configCompare;
    }
}

