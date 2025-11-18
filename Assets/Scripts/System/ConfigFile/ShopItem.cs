using UnityEngine;

namespace ConfigFile
{
    [System.Serializable]
    public class ShopItemRecord
    {
        [SerializeField] private ItemType id;
        [SerializeField] private string name;
        [SerializeField] private int quantity;

        public ItemType Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public int Quantity { get => quantity; set => quantity = value; }
    }

    public class ShopItemConfig : BYDataTable<PackConfigRecord>
    {
        public override ConfigCompare<PackConfigRecord> DefineConfigCompare()
        {
            configCompare = new ConfigCompare<PackConfigRecord>("id");
            return configCompare;
        }

    }
}
