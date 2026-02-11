using UnityEngine;
using UnityEngine.UI;

public class SpotlightRaycastBlocker : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private RectTransform hole;
    [SerializeField] private float radius = 150f;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            sp,
            eventCamera,
            out Vector2 localPoint
        );

        float dist = Vector2.Distance(localPoint, hole.anchoredPosition);

        return dist > radius;
    }
}
