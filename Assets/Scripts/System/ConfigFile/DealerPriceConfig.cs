using UnityEngine;


[System.Serializable]
public class DealerPriceConfigRecord
{
    [SerializeField]
    private int levelId;
    [SerializeField]
    private Currency currencyType;
    [SerializeField]
    private int cost;
    [SerializeField]
    private int levelGold;
    [SerializeField]
    private int levelGem;
    public int LevelId { get => levelId; set => levelId = value; }
    public Currency CurrencyType { get => currencyType; set => currencyType = value; }
    public int Cost { get => cost; set => cost = value; }
    public int LevelGold { get => levelGold; set => levelGold = value; }
    public int LevelGem { get => levelGem; set => levelGem = value; }
}
public class DealerPriceConfig : BYDataTable<DealerPriceConfigRecord>
{
    public override ConfigCompare<DealerPriceConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<DealerPriceConfigRecord>("levelId");
        return configCompare;
    }
}
