using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace ConfigFile
{
    [System.Serializable]
    public class PackConfigRecord
    {
        [SerializeField] private PackEnum id;
        [SerializeField] private PackType pack;
        [SerializeField] private string name;
        [SerializeField] private long price;

        [SerializeField] List<ShopItemRecord> items;
    }

    public class PackConfig : BYDataTable<PackConfigRecord>
    {
        public override ConfigCompare<PackConfigRecord> DefineConfigCompare()
        {
            configCompare = new ConfigCompare<PackConfigRecord>("id");
            return configCompare;
        }
        
    }
}
