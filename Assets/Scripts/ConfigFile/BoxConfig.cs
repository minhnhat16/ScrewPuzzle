using System;
using Enum;
using UnityEngine;

namespace ConfigFile
{
    [Serializable]
    public class BoxConfigRecord
    {
        [SerializeField]
        public int numberOfScrewHoles;
        [SerializeField]
        public ColorEnum boxColor;
    }
    public class BoxConfig: BYDataTable<BoxConfigRecord>
    {
        public override ConfigCompare<BoxConfigRecord> DefineConfigCompare()
        {
            configCompare = new ConfigCompare<BoxConfigRecord>("name");
            return configCompare;
        }
    }
}

