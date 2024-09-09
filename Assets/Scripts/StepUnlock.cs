using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StepUnlock : TutorialStep
{
    public override void Start()
    {
        StartCoroutine(Init((int)Type));
      
    }
  
}
