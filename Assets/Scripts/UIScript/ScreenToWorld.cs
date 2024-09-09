using UnityEngine;

public static class CanvasPositioningExtensions
{
    public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null, bool useNormalizeViewPort = false)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        var viewportPosition = camera.WorldToViewportPoint(worldPosition);

        if (useNormalizeViewPort)
        {
            Rect normalizedViewPort = camera.rect;
            viewportPosition.x = viewportPosition.x * normalizedViewPort.width + normalizedViewPort.x;
            viewportPosition.y = viewportPosition.y * normalizedViewPort.height + normalizedViewPort.y;
        }

        return canvas.ViewportToCanvasPosition(viewportPosition);
    }
    public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
        var viewportPosition = camera.WorldToViewportPoint(worldPosition);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
    {
        var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                                           screenPosition.y / Screen.height,
                                           0);
        return canvas.ViewportToCanvasPosition(viewportPosition);
    }

    public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
    {
        var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
        var canvasRect = canvas.GetComponent<RectTransform>();
        var scale = canvasRect.sizeDelta;
        return Vector3.Scale(centerBasedViewPortPosition, scale);
    }
}
public class ScreenToWorld : MonoBehaviour
{
    public static ScreenToWorld Instance;
    public Camera m_UICamera;
    public Camera m_WCamera;
    public RectTransform m_Image;
    public RectTransform m_Parent;
    public RectTransform m_AnchorView;

    public Canvas m_Canvas;
    public Canvas m_viewCanvas;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    public void SetWorldToCanvasPosition(RectTransform gameObject, Transform anchorPoint)
    {
        //m_UICamera = CameraMain.instance.GetCam();
        if (gameObject == null) return;
        Vector3 ViewportPosition = CanvasPositioningExtensions.WorldToCanvasPosition(m_viewCanvas, anchorPoint.position, m_WCamera, false);
        gameObject.SetParent(m_AnchorView);
        gameObject.anchoredPosition = ViewportPosition;
        SetStretch(gameObject);
    }
    public void SetWorldToCanvasPosition(RectTransform gameObject)
    {
        //m_UICamera = CameraMain.instance.GetCam();
        if (gameObject == null) return;
        Vector3 ViewportPosition = CanvasPositioningExtensions.WorldToCanvasPosition(m_viewCanvas, gameObject.transform.position, m_WCamera, false);
        gameObject.SetParent(m_AnchorView);
        gameObject.anchoredPosition = ViewportPosition;
        SetStretch(gameObject);
    }
    public void SetStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
    }
    public void SetWorldToCanvas(UpgradeSlotButton btn)
    {
        //Debug.LogWarning(gameObject.name + " parent " + m_Parent.name);
        btn.Rect.SetParent(m_Parent);
    }
    public void SetWorldToCanvas(RectTransform gameObject)
    {
        if (gameObject == null)
        {
            //Debug.LogError(gameObject.name + " parent " + m_Parent.name);
            return;
        }
        //Debug.LogWarning(gameObject.name + " parent " + m_Parent.name);
        gameObject.SetParent(m_Parent);
        ////Debug.LogWarning(gameObject.name + " parent after " + m_Parent.name);
    }
    public void SetWorldToViewCanvas(RectTransform gameObject)
    {
        if (gameObject == null) return;
        gameObject.SetParent(m_AnchorView);

    }
    public void SetWorldToAnchorView(Vector3 position, RectTransform toPos)
    {
        ViewManager.Instance.dicView.TryGetValue(ViewIndex.GamePlayView, out BaseView gameplay);
        var view = (GamePlayView)gameplay;
        var anchor = view.Anchor;
        var viewPos = CanvasPositioningExtensions.WorldToCanvasPosition(m_viewCanvas, position, m_WCamera);

        //set parent and set local scale for ui
        toPos.SetParent(anchor);
        toPos.localScale = Vector3.one;
        toPos.anchoredPosition3D = Vector3.zero;
        toPos.anchoredPosition3D = viewPos;

    }
    public Vector3 PreverseConvertPosition(Vector3 position)
    {
        m_WCamera = SlotCamera.Instance.S_Camera;
        // Convert the position from Canvas 1 to world position using Main Camera
        Vector3 viewportPosition = m_UICamera.WorldToViewportPoint(position);

        // Convert the viewport position to a screen position
        Vector3 screenPosition = m_UICamera.ViewportToScreenPoint(viewportPosition);

        // Convert the screen position to a world position using UI Camera
        Vector3 newWorldPosition = m_WCamera.ScreenToWorldPoint(screenPosition);

        return new Vector3(newWorldPosition.x, newWorldPosition.y, 0);
    }

    public Vector3 ConvertPositionNew(Vector3 position)
    {
        Vector3 vect = CanvasPositioningExtensions.WorldToCanvasPosition(m_viewCanvas, position, m_WCamera, false);
        return vect;
    }
}
