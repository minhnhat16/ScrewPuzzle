#if UNITY_EDITOR
using System;
using System.Collections;
using Enums;
using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMaker : MonoBehaviour
{
    
    [SerializeField] private GameObjectToLevelConverter converter;
    public UnityEvent onScrewClicked;
    public static LevelMaker instance;
    public bool isInputData;
    public bool isEditPartPosition;
    public bool isEditPartColor;
    public bool isEditScrewPosition;
    public bool isSelectColorForScrew;
    public bool isEditHinge;
    public bool isRemoveScrew;
    public bool isEditScrewColor;
    public ColorEnum currentScrewColorID = ColorEnum.Clear;
    [SerializeField] InputField levelInputField;
    [SerializeField] InputField layerInputField;
    [SerializeField] InputField levelSaveInput;
    [SerializeField] Dropdown saveOptionDropDown;

    public ColorDropDown colorDropDown;
    public DropDownLayer layerDropdown;



    public UnityEvent<int, ColorEnum> ontotalScrewChanged = new();
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
        if (onKey2Pressed == null) onKey2Pressed = new KeyEvent();
        if (onKeyAPressed == null) onKeyAPressed = new KeyEvent();
        if (onKeyBPressed == null) onKeyBPressed = new KeyEvent();
        
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện với InputManager
        InputManager.onKey0 += onKey0Pressed.Invoke;
        InputManager.onKey1 += onKey1Pressed.Invoke;
        InputManager.onKey2 += onKey2Pressed.Invoke;
        InputManager.onKeyA += onKeyAPressed.Invoke;
        InputManager.onKeyB += onKeyBPressed.Invoke;
        saveOptionDropDown.onValueChanged.AddListener(delegate { DropdownValueChanged(saveOptionDropDown); });
        // ... Đăng ký các phím còn lại

    }

   

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện khi không cần thiết
        InputManager.onKey0 -= onKey0Pressed.Invoke;
        InputManager.onKey1 -= onKey1Pressed.Invoke;
        InputManager.onKey2 -= onKey2Pressed.Invoke;
        InputManager.onKeyA -= onKeyAPressed.Invoke;
        InputManager.onKeyB -= onKeyBPressed.Invoke;    
        // ... Hủy đăng ký các phím còn lại

    }

    // Phương thức xử lý nhấn chuột vào screw
    public void OnScrewClicked()
    {
        Debug.Log("Screw clicked. Entering selection mode.");
        if (isInputData) return;
        onScrewClicked.Invoke();
    }

    public void ChosePartCoroutine(ScrewController screw)
    {
        StartCoroutine(ChosePart(screw));
    }

    private bool isChoosingPart = false;
    float timer = 0f;
    float timeout = 3f;
    private IEnumerator ChosePart(ScrewController screw)
    {
        if (isChoosingPart) yield break; // tránh start 2 lần
        isChoosingPart = true;

        GameObject partChosen = null;

        // Chờ 1 frame để đảm bảo mọi input từ frame trước đã trôi qua
        yield return null;

        // Chờ đến khi người chơi chọn đúng đối tượng Part
        yield return new WaitUntil(() =>
        {
            partChosen = PartGetInput();
            //screw.C(true);
            return partChosen != null && partChosen.TryGetComponent(out BasePart _);
        });

        // Bảo vệ null
        if (partChosen == null)
        {
            Debug.LogWarning("ChosePart: partChosen bị null sau WaitUntil");
            isChoosingPart = false;
            yield break;
        }

        // Lấy BasePart
        if (!partChosen.TryGetComponent<BasePart>(out var partScript))
        {
            Debug.LogWarning("ChosePart: Object click không có BasePart");
            isChoosingPart = false;
            yield break;
        }

        var bodyPart = partScript.Body;
        if (bodyPart == null)
        {
            Debug.LogError("ChosePart: BasePart.Body null");
            isChoosingPart = false;
            yield break;
        }

        Debug.Log("Part layer selected: " + partChosen.layer);

        // Lấy mouse pos world
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // Tạo dữ liệu hinge
        HingeConnection hinge = new HingeConnection()
        {
            hingePosition = mouseWorldPos,
            bodyPartUniqueID = partScript.uniqueID,
            bodyPartHingePosition = partScript.transform.position,
        };
       

   
        if (timer > timeout)
        {
            Debug.LogWarning("ChosePart timeout");
            isChoosingPart = false;
            yield break;
        }
        // Tạo hinge
        //screw.CreateHingeWithMousePos(bodyPart, hinge);
        isChoosingPart = false;
    }

    public void ChangeScene()
    {
        GameViewUtils.SetGameViewResolution(1080, 1920);
        SceneManager.LoadScene("BootScene");

    }
    private Vector3 GetMouseWorldPosition()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Không tìm thấy Camera.main");
            return Vector3.zero;
        }

        Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        return pos;
    }

    private GameObject PartGetInput()
    {

        Debug.Log("Part get input ");
        // Detect mouse click
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            Debug.Log("Clicked part");
            // Check if the clicked object is a valid part and is active
            if (hit.collider != null && hit.collider.CompareTag("Part"))
            {
                GameObject obj = hit.collider.gameObject;
                if (obj.activeInHierarchy)
                {
                    return obj;
                }
            }
        }
        return null;
    }


    public int GetLayerInputField()
    {
        string inputValue = layerInputField.text;
        int parsedInt;
        // Display the input value in another UI Text component
        layerInputField.text = "Entered value: " + inputValue;

        if (int.TryParse(inputValue, out parsedInt))
        {
            layerInputField.text = "Entered integer value: " + parsedInt;
            Debug.Log("User Input (int): " + parsedInt);
            return parsedInt;
        }
        else
        {
            // If the input is not a valid integer, display an error message
            layerInputField.text = "0";
            Debug.LogWarning("Invalid input. Please enter a valid integer.");
            return 0;
        }
    }

    public void SaveLevel()
    {

        GetCurrentDropdownOption();
    }
    void DropdownValueChanged(Dropdown change)
    {
        // Get the index of the selected option
        int index = change.value;

        // Get the selected option's text
        string selectedOption = change.options[index].text;
        
        levelSaveInput.gameObject.SetActive(index == 2); 
        
        Debug.Log("Selected option: " + selectedOption + "current index" + index);
    }
    public void GetCurrentDropdownOption()
    {
        if (saveOptionDropDown != null)
        {
            // Get the current index
            int currentIndex = saveOptionDropDown.value;
            string currentOption = saveOptionDropDown.options[currentIndex].text;
            Debug.Log("Currently selected option: " + currentOption  + "current index" + currentIndex);
            switch (currentIndex)
            {
                case 0:
                    converter.SaveGameObjectToLevel();
                    break;
                case 1 :
                    converter.nextLevelId = converter.currentLoadedLevel;
                    converter.SaveGameObjectToLevel();
                    break;
                case 2 :
                    converter.nextLevelId = Convert.ToInt32(levelSaveInput.text);
                    converter.SaveGameObjectToLevel();
                    break;
                default:
                    break;
            }
          
            // Get the current option's text
          
        }
    }
    public void ResetAllScrewHinge()
    {
        
    }


    [SerializeField]
    public void SetEditMode(EditMode mode)
    {
        isEditScrewPosition = (mode == EditMode.ScrewPosition);
        isEditScrewColor = (mode == EditMode.ScrewColor);
        isEditHinge = (mode == EditMode.Hinge);
        isEditPartPosition = (mode == EditMode.PartPosition);
        isEditPartColor = (mode == EditMode.PartColor);
        isRemoveScrew = (mode == EditMode.RemoveHinge);
    }
    public void TurnAllEditModeOff()
    {
        isEditScrewPosition = 
            isEditScrewColor = 
                    isEditHinge =
                    isEditPartColor =
                        isEditPartPosition = false;
        converter.ResetAllScrewsFlag();
    }
    public void ClickOnEditScrewPos()
    {
        TurnAllEditModeOff();
        SetEditMode(EditMode.ScrewPosition);
    }

    public void ClickOnEditScrewColor()
    {
        TurnAllEditModeOff();
        SetEditMode(EditMode.ScrewColor);
    }

    public void ClickOnEditScrewHinge()
    {
        TurnAllEditModeOff();

        SetEditMode(EditMode.Hinge);
    }

    public void ClickOnEditPartPosition()
    {
        TurnAllEditModeOff();
        SetEditMode(EditMode.PartPosition);
    }

    public void ClickOnEditPartColor()
    {
        TurnAllEditModeOff();
        SetEditMode(EditMode.PartColor);
    }


    public void ClickOnRemoveHinge()
    {
        TurnAllEditModeOff();
        SetEditMode(EditMode.RemoveHinge);
    }


    public void RemoveAllScrew()
    {

    }
    private void OnValidate()
    {
    }
}

public enum EditMode
{
    ScrewPosition,
    ScrewColor,
    Hinge,
    PartPosition,
    PartColor,
    RemoveHinge
}
#endif
