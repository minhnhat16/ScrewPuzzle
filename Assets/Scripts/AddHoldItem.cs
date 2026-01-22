using System.Collections;
using System.Collections.Generic;
using Ingame;
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
        var pos = ArrayScrew.Instance.GetHoldPos()  + new Vector3(1,7);
        IngameController.ins.onItemInvoke?.Invoke(Type,pos);
    }

    public void OnButtonClicked()
    {
        Debug.Log("on button click OnButtonClicked");

    }
}   
