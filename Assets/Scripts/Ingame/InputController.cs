using System;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public event Action<Vector2> OnTap;
    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDrag;
    public event Action<Vector2> OnDragEnd;

    private Vector2 startPos;
    private bool isDragging;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            OnTap?.Invoke(startPos);    
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDragging && Vector2.Distance(startPos, Input.mousePosition) > 10f)
            {
                isDragging = true;
                OnDragStart?.Invoke(startPos);
            }

            if (isDragging)
                OnDrag?.Invoke(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
                OnDragEnd?.Invoke(Input.mousePosition);

            isDragging = false;
        }
    }
}