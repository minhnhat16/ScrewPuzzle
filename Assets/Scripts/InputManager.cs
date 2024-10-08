using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Tạo delegate để lắng nghe các sự kiện phím nhấn
    public delegate void KeyAction();
    public static event KeyAction onSpaceKey;
    public static event KeyAction onEscapeKey;

    private void Update()
    {
        // Kiểm tra sự kiện nhấn phím và phát ra sự kiện tương ứng
        if (Input.GetKeyDown(KeyCode.Space))
        {
            onSpaceKey?.Invoke(); // Kích hoạt sự kiện khi nhấn phím Space
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            onEscapeKey?.Invoke(); // Kích hoạt sự kiện khi nhấn phím Escape
        }
    }
}