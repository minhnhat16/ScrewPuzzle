using UnityEngine;
using System;
using System.ConfigFile;
using Enum;

namespace ConfigFile
{
    [Serializable]
    public class BoxConfigRecord
    {
        [SerializeField]
        public int NumberOfScrewHoles;
        [SerializeField]
        public ColorEnum BoxColor;
    }
    [CreateAssetMenu(fileName = "NewBoxConfig", menuName = "Config/BoxConfig")]
    public class BoxConfig : BYDataTable<BoxConfigRecord>
    {
        
        // Custom method for comparing config, can be adjusted
        public override ConfigCompare<BoxConfigRecord> DefineConfigCompare()
        {
            var configCompare = new ConfigCompare<BoxConfigRecord>("level");
            return configCompare;
        }
    }
}