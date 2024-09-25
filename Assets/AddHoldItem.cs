using System.Collections;
using System.Collections.Generic;
using Managers;
using UIScript;
using UnityEngine;
using UnityEngine.UI;

public class AddHoldItem : ItemButton
{
    public override void OnEnable()
    {
    }
    public override void OnClick()
    {
        Debug.Log("on button click");
        IngameController.Instance.onItemInvoke?.Invoke(Type);
    }

    public void OnButtonClicked()
    {
        Debug.Log("on button click OnButtonClicked");

    }
}   
