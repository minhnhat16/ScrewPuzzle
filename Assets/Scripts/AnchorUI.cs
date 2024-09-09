using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorUI : MonoBehaviour
{
    public Transform objectToFollow;
    public Vector2 offset;
    private RectTransform rectTransform = null;
    private Camera cam;

    private void Start()
    {
        cam = CameraMain.instance.main == null ? CameraMain.instance.GetCam() : CameraMain.instance.main ;
        rectTransform = GetComponent<RectTransform>();
        if (!objectToFollow) return;
        rectTransform.position = RectTransformUtility.WorldToScreenPoint(cam, objectToFollow.position) + offset;
    }

}
