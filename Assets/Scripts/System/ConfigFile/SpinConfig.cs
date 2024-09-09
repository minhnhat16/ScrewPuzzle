using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class SpinConfigRecord
{
    [SerializeField]    
    private int id;
    [SerializeField]
    private SpinEnum type;
    [SerializeField]
    private int amount;
    [SerializeField]
    private float rate;
    [SerializeField]
    private string itemImg;

    public int Id { get { return id; }  }
    public SpinEnum Type { get { return type; } }
    public int Amount { get { return amount; } }
    public float Rate { get { return rate; }}
    public string ItemImg { get { return itemImg; } }
}
public class SpinConfig : BYDataTable<SpinConfigRecord>
{
    public override ConfigCompare<SpinConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<SpinConfigRecord>("id");
        return configCompare;
    }
}
