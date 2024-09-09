using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private Toggle buttonTypes;
    [SerializeField] private Text text_lb;

    [HideInInspector]
    public UnityEvent<int> Event = new UnityEvent<int>();

    private void OnEnable()
    {
        
    }
    public virtual void OnToggleValueChanged(bool isOn)
    {

    }
}
