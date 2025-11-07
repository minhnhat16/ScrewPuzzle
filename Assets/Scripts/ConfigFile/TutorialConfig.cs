using UnityEngine;
using System;
using System.ConfigFile;
using Enums;

namespace ConfigFile
{
    [Serializable]
    public class TutorialConfigRecord
    {
        [SerializeField]
        public int stepID;
        [SerializeField]
        public string tutorialString;
        [SerializeField]
        public Vector3 cusorPosition;
        [SerializeField]
        public bool isStepDone;
    }
    public class TutorialConfig : BYDataTable<TutorialConfigRecord>
    {
        // Custom method for comparing config, can be adjusted
        public override ConfigCompare<TutorialConfigRecord> DefineConfigCompare()
        {
            var configCompare = new ConfigCompare<TutorialConfigRecord>("stepID");
            return configCompare;
        }
    }
}