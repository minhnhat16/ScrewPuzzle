using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepOne : TutorialStep
{
    // Start is called before the first frame update
    public override void Start()
    {
        StartCoroutine(Init((int)Type));
    }

}
