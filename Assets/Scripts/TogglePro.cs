using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePro : Toggle
{
    public Graphic checkIcon;

    protected override void OnEnable()
    {
        base.OnEnable();
        onValueChanged.AddListener(OnToggleValueChanged);
        checkIcon.canvasRenderer.SetAlpha(isOn ? 1f : 0f);

    }
    protected override void OnDisable()
    {
        onValueChanged.RemoveListener(OnToggleValueChanged);
    }
    public void OnToggleValueChanged(bool isOn)
    {
        interactable = !isOn;
        // B?t ho?c t?t `tickIcon` d?a trên tr?ng thái c?a Toggle
        if (checkIcon != null)
        {
            checkIcon.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
            Debug.Log(" on toggle value changed iCon" + (isOn ? 1f : 0f));
        }
    }

}
