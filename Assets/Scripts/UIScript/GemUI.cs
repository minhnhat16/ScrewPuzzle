using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemUI : CurrencyAnim
{
    public override void Start()
    {
        base.Start();
        Rect = GetComponent<RectTransform>();
        Transf = GetComponent<Transform>();
        Script = GetComponent<CurrencyAnim>();
    }
}
