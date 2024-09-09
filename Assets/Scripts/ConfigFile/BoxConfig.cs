using Enum;
using UnityEngine;

namespace ConfigFile
{
    public class BoxConfigRecord
    {
        public int numberOfScrewHoles;
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

