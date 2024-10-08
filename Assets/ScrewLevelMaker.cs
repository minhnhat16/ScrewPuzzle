using System.Collections;
using System.Collections.Generic;
using Ingame.Screw;
using UnityEngine;
using UnityEngine.Events;

public class ScrewLevelMaker : Screw
{
    [HideInInspector] public UnityEvent OnClickOnEditMode;
    private bool isHeld = false;
    public override void Awake()
    {
        base.Awake();
        _transform = GetComponent<Transform>();
        Position = GetComponent<Transform>().position;
        _circleCollider2D = GetComponentInChildren<CircleCollider2D>();
        render = GetComponentInChildren<SpriteRenderer>();
        layerMask = gameObject.layer;
        OnClickOnEditMode.AddListener(ClickScrewEdit);
    }
    private void ClickScrewEdit()
    {
        Debug.Log("Click screw editor");
        // Detect Mouse Click
        if (Input.GetMouseButtonDown(0))
        {
            
            OnMouseClick();
        }

        // Detect Mouse Hold
        if (Input.GetMouseButton(0) && isHeld)
        {
            OnMouseHold();
        }

        // Detect Mouse Release
        if (Input.GetMouseButtonUp(0) && isHeld)
        {
            OnMouseRelease();
        }
    }

    private void OnMouseClick()
    {
        isHeld = true;
        Debug.Log("Mouse Clicked on Screw.");
        // Add logic for when the screw is clicked
    }

    private void OnMouseHold()
    {
        // Get the mouse position in world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z; // Maintain the object's z position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        // Set the game object's position to follow the mouse position
        transform.position = worldPosition;

        Debug.Log("Mouse is holding and dragging the Screw.");
    }

    private void OnMouseRelease()
    {
        isHeld = false;
        Debug.Log("Mouse Released the Screw.");
        // Add logic for when the mouse is released after holding
    }
}

