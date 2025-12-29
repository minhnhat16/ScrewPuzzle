
using ConfigFile;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


//============================================================
// Reward Item struct (matching previous design)
//============================================================
[System.Serializable]
public class RewardItem
{
    public ItemType itemType;
    public string icon_name;
    public int amount;

    public RewardItem(string icon_name, int amount)
    {
        this.icon_name = icon_name;
        this.amount = amount;
    }
}
[System.Serializable]
public class RewardConfigRecord
{
    [SerializeField]
    private int id;
    [SerializeField]
    private List<RewardItem> items;

    public int Id { get => id; set => id = value; }
    public List<RewardItem> Items { get => items; set => items = value; }
}

public class RewardConfig : BYDataTable<RewardConfigRecord>
{
    public override ConfigCompare<RewardConfigRecord> DefineConfigCompare()
    {
        Debug.Log("Definde config compare reward " );
        configCompare = new ConfigCompare<RewardConfigRecord>("id");
        return configCompare;
    }
}