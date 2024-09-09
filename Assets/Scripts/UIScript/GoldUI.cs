using DG.Tweening;
using UnityEngine;

public class GoldUI : CurrencyAnim
{
    public override void Start()
    {
        base.Start();
        Rect = GetComponent<RectTransform>();
        Transf = GetComponent<Transform>();
        Script = GetComponent<CurrencyAnim>();
    }
}
