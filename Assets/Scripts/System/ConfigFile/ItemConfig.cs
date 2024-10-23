using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ItemConfigRecord
{
    [SerializeField] private int price;
    [SerializeField] private ItemType type;
    [SerializeField] private int quantity;
    [SerializeField] private string detail;

    public string Detail
    {
        get => detail;
        set => detail = value;
    }

    public int Price => price;

    public ItemType Type => type;

    public int Quantity => quantity;
}

public class ItemConfig : BYDataTable<ItemConfigRecord>
{
    public override ConfigCompare<ItemConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<ItemConfigRecord>("type");
        return configCompare;
    }
}