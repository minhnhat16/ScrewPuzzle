

using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class QuestRecord
{
    //id as stage
    [SerializeField]
    private int id;
    [SerializeField]
    private int chestID;
    [SerializeField]
    private List<int> missionRecords;

    public int Id { get => id; set => id = value; }
    public int ChestID { get => chestID; set => chestID = value; }
    public List<int> MissionIds { get => missionRecords; set => missionRecords = value; }
}


public class QuestConfig : BYDataTable<QuestRecord>
{
    public override ConfigCompare<QuestRecord> DefineConfigCompare()
    {
        return new ConfigCompare<QuestRecord>("id");
    }
}

