using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ConfigFile
{
    

    public class TutorialConfig : BYDataTable<TutorialStep>
    {
        public override ConfigCompare<TutorialStep> DefineConfigCompare()
        {
            // tutorialId + stepID = unique
            return new ConfigCompare<TutorialStep>(
                nameof(TutorialStep.stepId)
            );
        }
   
    }
}
