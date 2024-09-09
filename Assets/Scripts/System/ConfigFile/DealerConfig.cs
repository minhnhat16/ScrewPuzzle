using UnityEngine;


[System.Serializable]
public class DealerConfigRecord
{
    [SerializeField]
    private int id;
    [SerializeField]
    private float x;
    [SerializeField]
    private float y;
    [SerializeField]
    private float z;
    [SerializeField]
    private SlotStatus status;
}
public class DealerConfig : BYDataTable<DealerConfigRecord>
{
    public override ConfigCompare<DealerConfigRecord> DefineConfigCompare()
    {
        configCompare = new ConfigCompare<DealerConfigRecord>("id");
        return configCompare;
    }
}
