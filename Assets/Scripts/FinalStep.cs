using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FinalStep : TutorialStep
{

    public override void Start()
    {
        StartCoroutine(Init((int)Type));
    }
}
