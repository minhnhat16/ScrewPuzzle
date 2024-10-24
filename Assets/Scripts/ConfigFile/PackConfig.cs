using UnityEngine;

namespace ConfigFile
{
    [System.Serializable]
    public class PackConfigRecord
    {
        [SerializeField] private int id;
        [SerializeField] private int idItem;
        [SerializeField] private float price;
        [SerializeField] private string ribbonText;
        [SerializeField] private string iconName;
        [SerializeField] private string ribbonColorName;

     

        [SerializeField] private string iconItem1;
        [SerializeField] private int quantityItem1;
        [SerializeField] private string iconItem2;
        [SerializeField] private int quantityItem2;
        [SerializeField] private string iconItem3;
        [SerializeField] private int quantityItem3;
        public int ID => id;

        public int IDItem => idItem;

        public float Price => price;

        public string RibbonText => ribbonText;
        public string IconName => iconName;

        public string RibbonColorName => ribbonColorName;

        public int QuantityItem1 => quantityItem1;

        public int QuantityItem2 => quantityItem2;

        public int QuantityItem3 => quantityItem3;

        public string IconItem1 => iconItem1;

        public string IconItem2 => iconItem2;

        public string IconItem3 => iconItem3;

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
