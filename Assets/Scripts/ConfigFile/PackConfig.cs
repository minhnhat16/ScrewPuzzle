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
        [SerializeField] private Currency currencyType;
        [SerializeField] List<ShopItemRecord> items;

        public PackEnum Id { get => id; set => id = value; }
        public PackType Pack { get => pack; set => pack = value; }
        public string Name { get => name; set => name = value; }
        public long Price { get => price; set => price = value; }
        public List<ShopItemRecord> Items { get => items; set => items = value; }
        public Currency CurrencyType { get => currencyType; set => currencyType = value; }
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
