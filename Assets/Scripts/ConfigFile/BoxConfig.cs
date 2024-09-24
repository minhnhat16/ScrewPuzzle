using UnityEngine;
using System;
using Enum;

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

    [CreateAssetMenu(fileName = "NewBoxConfig", menuName = "Config/BoxConfig")]
    public class BoxConfig : ScriptableObject
    {
        public BoxConfigRecord[] boxConfigRecords;

        // Custom method for comparing config, can be adjusted
        public ConfigCompare<BoxConfigRecord> DefineConfigCompare()
        {
            var configCompare = new ConfigCompare<BoxConfigRecord>("level");
            return configCompare;
        }
    }
}