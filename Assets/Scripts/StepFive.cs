using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepFive : TutorialStep
{
    public override void Start()
    {
        StartCoroutine(Init((int)Type));
    }
}
