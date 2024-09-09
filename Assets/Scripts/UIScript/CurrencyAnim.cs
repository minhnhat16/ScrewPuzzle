using DG.Tweening;
using System;
using UnityEngine;

public abstract class CurrencyAnim : MonoBehaviour
{
    [SerializeField] RectTransform rect;
    [SerializeField] Transform transf;
    [SerializeField] CurrencyAnim script;
    public CurrencyAnim Script { get => script; set => script = value; }
    public RectTransform Rect { get => rect; set => rect = value; }
    public Transform Transf { get => transf; set => transf = value; }

    public virtual void Start()
    {
      
    }
    public virtual void Update()
    {

    }
    Tween t;


    public virtual void DoScaleUp(Vector3 fromScale, Vector3 toScale, Action callback)
    {
        transf.localScale = fromScale;
        t = transf.DOScale(toScale, 0.25f).SetEase(Ease.OutBounce);
        t.OnComplete(() =>
        {
            t.Kill();
            callback?.Invoke();
        });
    }
    public virtual void DoRotation(Action callback)
    {
        t = transform.DORotateQuaternion(new Quaternion(0, 0, 0, 360f), 4f);
        t.OnComplete(() =>
        {
            callback?.Invoke();
            t.Kill();
        });

    }
    public virtual void DoMoveToTarget(Vector3 to, Action callback)
    {
        t = rect.DOAnchorPos3D(to, 1f).SetEase(Ease.InQuad);
        t.OnComplete(() =>
        {
            callback?.Invoke();
        });

    }
    public virtual void DoPathTo(Vector3 to)
    {
        int segments = 100;
        float curveIntensity = 100f;
        Vector3[] pathPoints = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float curveFactor = Mathf.Sin(t * Mathf.PI); // Hàm s? ?? t?ng ?? cong (?ây ch? là m?t ví d?)
            Vector3 curvePoint = Vector3.Lerp(transf.position, to, t) + transf.right * curveFactor * curveIntensity;
            pathPoints[i] = curvePoint;
        }

        transf.DOPath(pathPoints, 0.75f, PathType.CatmullRom);
    }
}
