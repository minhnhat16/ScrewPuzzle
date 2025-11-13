using UnityEngine;

public class InputManager : MonoBehaviour
{
    public delegate void KeyAction();
    public static event KeyAction onKey0;
    public static event KeyAction onKey1;
    public static event KeyAction onKey2;
    public static event KeyAction onKeyA;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) onKey0?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha1)) onKey1?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2)) onKey2?.Invoke();
        if (Input.GetKeyDown(KeyCode.A)) onKeyA?.Invoke();
    }
}