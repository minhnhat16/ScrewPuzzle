using System;
using UnityEngine;

[Serializable]
public class LevelConfigRecord 
{
    [SerializeField]
    private int id; // Level ID
    [SerializeField]
    private int experience; //Expearience for levelup
    [SerializeField]
    private bool isComplete;
    [SerializeField]
    private CardColorPallet freeColor;
    [SerializeField]
    private CardColorPallet premiumColor;

    public int Id { get { return id; } }
    public int Experience { get { return experience; } }
    public bool IsComplete { get { return isComplete; } }

    public CardColorPallet FreeColor { get => freeColor; set => freeColor = value; }
    public CardColorPallet PremiumColor { get => premiumColor; set => premiumColor = value; }
}

public class LevelConfig : BYDataTable<LevelConfigRecord>
{
    public override ConfigCompare<LevelConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<LevelConfigRecord>("id");
        return configCompare;
    }
}
