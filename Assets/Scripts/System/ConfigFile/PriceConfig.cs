using UnityEngine;


[System.Serializable]
public class PriceConfigRecord
{

    [SerializeField] private PackEnum type;
    [SerializeField] private ItemType idItem;
    [SerializeField] private int price;
    [SerializeField] private int amount;
    [SerializeField] private bool available;

    [SerializeField] private string spriteName;
    [SerializeField] private bool moneyPaid;
 
    public int Price
    {
        get => price;
    }

    public int Amount
    {
        get => amount;
    }

    public bool Available
    {
        get => available;
    }

    public bool MoneyPaid
    {
        get => moneyPaid;
    }

    public string SpriteName
    {
        get => spriteName ;
    }
}

public class PriceConfig : BYDataTable<PriceConfigRecord>
{
    public override ConfigCompare<PriceConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<PriceConfigRecord>("id");
        return configCompare;
    }
}