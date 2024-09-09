using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ItemConfigRecord
{
    [SerializeField]    
    private int id;
    [SerializeField]
    private string spriteName;
    [SerializeField]
    private ItemType type;
    [SerializeField]
    private string skinType;
    public int ID { get { return id; } }
    public string SpriteName { get { return spriteName; } }
    public ItemType Type { get { return type; } }
    public string SkinType { get { return skinType; } }
}
public class ItemConfig : BYDataTable<ItemConfigRecord>
{
    public override ConfigCompare<ItemConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<ItemConfigRecord>("id");
        return configCompare;
    }
}
