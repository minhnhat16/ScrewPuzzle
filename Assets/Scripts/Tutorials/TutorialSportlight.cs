using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSpotlight : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image image;
    [SerializeField] private Sprite round;
    [SerializeField] private Sprite circle;
    [SerializeField] private CanvasGroup group;
    private Tween moveTween;
    public void Show(
     Transform target,
     float size = 150f,
     float scale = 1f,
    bool useRound = false
 )
    {
        group.alpha = 1;
        image.sprite = useRound ? round : circle;
        Debug.Log($"TutorialSpotlight sprite {image.sprite.name}");
        rect.gameObject.SetActive(true);


        var canvas = rect.GetComponentInParent<Canvas>();
        ScreenToWorld.Instance.WorldToScreenCanvas(target, canvas, out Vector2 pos);

        rect.DOAnchorPos(pos, 0.35f).SetEase(Ease.OutCubic);
        rect.DOSizeDelta(Vector2.one * size, 0.25f).SetEase(Ease.OutBack);
        rect.localScale = Vector3.one * scale;
    }


    public void Hide()
    {
        group.alpha = 0;

        moveTween?.Kill();
        rect.gameObject.SetActive(false);
    }
}
