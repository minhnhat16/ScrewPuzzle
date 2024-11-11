using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class CollectionConfigRecord
{
    public CollectionLable type;
    public string iconName;
}

[Serializable]
public class CollectionConfig : BYDataTable<CollectionConfigRecord>
{
    public override ConfigCompare<CollectionConfigRecord> DefineConfigCompare()
    {
        var configCompare = new ConfigCompare<CollectionConfigRecord>("type");
        return configCompare;
    }
}
