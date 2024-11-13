namespace ConfigFile
{
    using Enums;
    using UnityEngine;

    namespace ConfigFile
    {
        public class ScrewConfigRecord
        {
            public int type;
            public ColorEnum screwColor;
        }
        public class ScrewConfig: BYDataTable<ScrewConfig>
        {
            public override ConfigCompare<ScrewConfig> DefineConfigCompare()
            {
                configCompare = new ConfigCompare<ScrewConfig>("name");
                return configCompare;
            }
        }
    }
}