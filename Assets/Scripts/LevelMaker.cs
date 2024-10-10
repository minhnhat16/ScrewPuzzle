using System.Collections;
using Ingame;
using Ingame.Screw;
using UnityEngine;
using UnityEngine.Events;

public class LevelMaker : MonoBehaviour
{
    [SerializeField] private GameObjectToLevelConverter converter;
    public UnityEvent onScrewClicked;
    public static LevelMaker instance;// Thay Event thành UnityEvent
    [System.Serializable]
    public class KeyEvent : UnityEvent { }

    #region: EventKey
    // UnityEvents cho các phím số từ 0 đến 9
    public KeyEvent onKey0Pressed;
    public KeyEvent onKey1Pressed;
    public KeyEvent onKey2Pressed;
    public KeyEvent onKey3Pressed;
    public KeyEvent onKey4Pressed;
    public KeyEvent onKey5Pressed;
    public KeyEvent onKey6Pressed;
    public KeyEvent onKey7Pressed;
    public KeyEvent onKey8Pressed;
    public KeyEvent onKey9Pressed;

    // UnityEvents cho các phím ký tự từ A đến Z
    public KeyEvent onKeyAPressed;
    public KeyEvent onKeyBPressed;
    public KeyEvent onKeyCPressed;
    // ... tương tự cho các phím khác
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        // Đăng ký các sự kiện nếu chưa có
        RegisterKeyPress();
    }

    private void RegisterKeyPress()
    {
        // Đăng ký sự kiện nếu chưa có
        if (onKey0Pressed == null) onKey0Pressed = new KeyEvent();
        if (onKey1Pressed == null) onKey1Pressed = new KeyEvent();
        if (onKeyAPressed == null) onKeyAPressed = new KeyEvent();
        // ... Tương tự cho tất cả các phím khác
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện với InputManager
        InputManager.onKey0 += onKey0Pressed.Invoke;
        InputManager.onKey1 += onKey1Pressed.Invoke;
        InputManager.onKeyA += onKeyAPressed.Invoke;
        // ... Đăng ký các phím còn lại
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện khi không cần thiết
        InputManager.onKey0 -= onKey0Pressed.Invoke;
        InputManager.onKey1 -= onKey1Pressed.Invoke;
        InputManager.onKeyA -= onKeyAPressed.Invoke;
        // ... Hủy đăng ký các phím còn lại
    }

    // Phương thức xử lý nhấn chuột vào screw
    public void OnScrewClicked()
    {
        Debug.Log("Screw clicked. Entering selection mode.");
        // Logic để xử lý khi nhấn vào screw
        // Có thể gọi sự kiện hay logic khác ở đây
        onScrewClicked.Invoke();
    }

    public void ChosePartCoroutine(ScrewLevelMaker screw)
    {
        StartCoroutine(ChosePart(screw));
    }

    private IEnumerator ChosePart(ScrewLevelMaker screw)
    {
        GameObject partChosen = null;

        // Wait until a valid part is clicked
        yield return new WaitUntil(() => 
        {
            partChosen = PartGetInput();
            return partChosen != null;
        });

        // Once a part is selected, get its Rigidbody2D and create a hinge
        var partScript = partChosen.GetComponent<BasePart>();
        var bodyPart = partScript.Body;
        screw.CreateHinge(bodyPart);
    }

    private GameObject PartGetInput()
    {
        // Detect mouse click
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            // Check if the clicked object is a valid part
            if (hit.collider != null && hit.collider.CompareTag("Part"))
            {
                return hit.collider.gameObject;
            }
        }
        return null; 
    }

}
