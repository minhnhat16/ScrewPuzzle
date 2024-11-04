using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TogglePro : Toggle
{
    public Graphic checkIcon;

    protected override void Start()
    {
        base.Start();
        // G?i base ?? ??m b?o các thi?t l?p ban ??u c?a Toggle ???c th?c hi?n

        // Thêm s? ki?n l?ng nghe ?? theo dõi thay ??i tr?ng thái
        onValueChanged.AddListener(OnToggleValueChanged);
        onValueChanged.Invoke(false);
    }
    protected override void OnDisable()
    {
        onValueChanged.RemoveListener(OnToggleValueChanged);
    }
    private void OnToggleValueChanged(bool isOn)
    {
        // B?t ho?c t?t `tickIcon` d?a trên tr?ng thái c?a Toggle
        if (checkIcon != null)
        {
            checkIcon.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
            Debug.Log(" on toggle value changed " + (isOn ? 1f : 0f));
        }
    }

}
