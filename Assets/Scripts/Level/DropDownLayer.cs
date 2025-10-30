using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DropDownLayer : MonoBehaviour
{

    public List<int> layerIndices;
    [SerializeField]
    private Dropdown dropDown;

    private void OnEnable()
    {
        dropDown.onValueChanged.AddListener((action) =>
        {
            Debug.Log("On value change drop down layer " + action);
        });
    }

    private void Awake()
    {
        dropDown = GetComponent<Dropdown>();
        List<int> layerIndices = new List<int>();

        for (int i = 10; i < 27; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            Debug.Log("Layer name " + layerName);

            if (!string.IsNullOrEmpty(layerName))
            {
                layerIndices.Add(i); // ✅ lưu index, không dùng GetMask()
            }
        }

        dropDown.ClearOptions();
        dropDown.AddOptions(layerIndices.ConvertAll(i => LayerMask.LayerToName(i)));
    }

    public void OnValueChange(UnityAction<int> action)
    {
        dropDown.onValueChanged.AddListener(action);

    }
    public void RemoveListener(UnityAction<int> action)
    {
        dropDown.onValueChanged.RemoveListener(action);
    }

}
