using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepThreePlus : TutorialStep
{
    // Start is called before the first frame update
    public override void Start()
    {
        StartCoroutine(Init((int)Type));
    }

  
}
