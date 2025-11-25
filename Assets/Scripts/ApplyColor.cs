using Ingame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyColor : MonoBehaviour
{
    public FlexibleColorPicker colorPicker;
    public Sprite sprite;
    public static ApplyColor instance;
    public void Awake()
    {
            if (instance != null) instance = this;
            instance = this;
    }
    public void ApplyColorToSprite(BasePart part)
    {
        if (part == null)
        {
            Debug.Log("Part is null cant change color");

            return;
        }
        Debug.Log("Part changed color to " + colorPicker.color);

        part.Renderer.color = colorPicker.color;
    }
}
