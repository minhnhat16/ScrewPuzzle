    using UnityEngine;

[System.Serializable]
public class PriceConfigRecord
{
    [SerializeField]
    private int id;
    [SerializeField]
    private int idShop;
    [SerializeField]
    private int idItem;
    [SerializeField]
    private int price;
    [SerializeField]
    private int amount;
    [SerializeField]
    private bool available;
    [SerializeField]
    private bool moneyPaid;
    public int Id { get => id; }
    public int IdShop { get => idShop; }
    public int IdItem { get => idItem; }
    public int Price { get => price; }
    public int Amount { get => amount; }
    public bool Available { get => available; }
    public bool MoneyPaid { get => moneyPaid; }
}

public class PriceConfig : BYDataTable<PriceConfigRecord>
{
    public override ConfigCompare<PriceConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<PriceConfigRecord>("id");
        return configCompare;
    }
}