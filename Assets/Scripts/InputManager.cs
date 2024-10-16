using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Tạo delegate để lắng nghe các sự kiện phím nhấn
    public delegate void KeyAction();
    public static event KeyAction onKey0;
    public static event KeyAction onKey1;
    public static event KeyAction onKey2;
    public static event KeyAction onKeyA;
    // ... Tương tự cho tất cả các phím số và ký tự khác

    private void Update()
    {
        // Kiểm tra từng phím số từ 0 đến 9
        if (Input.GetKeyDown(KeyCode.Alpha0)) onKey0?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha1)) onKey1?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha2)) onKey2?.Invoke();
        // ... Tương tự cho tất cả các phím số

        // Kiểm tra các phím ký tự từ A đến Z
        if (Input.GetKeyDown(KeyCode.A)) onKeyA?.Invoke();
        // ... Tương tự cho tất cả các phím ký tự
    }
}