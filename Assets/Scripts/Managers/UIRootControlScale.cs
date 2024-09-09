using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRootControlScale : MonoBehaviour
{
    [SerializeField] CanvasScaler[] canvasScalers;
    public float rate;
    public float scale;
    int UILayer;
    private void Start()
    {
        rate = 720f / 1280f;
        //Debug.Log("WIDTH AND HIGHT" + Screen.width + " " + Screen.height);
        UILayer = LayerMask.NameToLayer("SubRender");
        float currentRate = (float)Screen.width / (float)Screen.height;
        scale = currentRate > rate ? 1 : 0;
        //Debug.Log("CURRENT RATE" + (currentRate > rate ? 1 : 0));
        foreach (CanvasScaler cs in canvasScalers)
        {
            //Debug.Log("CanvasScaler" + cs + "Scale" + scale);
            cs.matchWidthOrHeight = scale;
        }
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

}
