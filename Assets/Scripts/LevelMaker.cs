using System;
using System.Collections;
using Ingame;
using Ingame.Screw;
using Level;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

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
    public bool isEditScrewColor;
    public int currentScrewColorID = 2;
    [SerializeField] InputField levelInputField;
    [SerializeField] InputField layerInputField;
    [SerializeField] InputField levelSaveInput;
    [SerializeField] Dropdown saveOptionDropDown;
    
    
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
        // ... Tương tự cho tất cả các phím khác
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện với InputManager
        InputManager.onKey0 += onKey0Pressed.Invoke;
        InputManager.onKey1 += onKey1Pressed.Invoke;
        InputManager.onKey2 += onKey2Pressed.Invoke;
        InputManager.onKeyA += onKeyAPressed.Invoke;
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
        // ... Hủy đăng ký các phím còn lại
    }

    // Phương thức xử lý nhấn chuột vào screw
    public void OnScrewClicked()
    {
        Debug.Log("Screw clicked. Entering selection mode.");
        if (isInputData) return;
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
        yield return new WaitForEndOfFrame();
        // Wait until a valid part is clicked
        yield return new WaitUntil(() => 
        {
            partChosen = PartGetInput();
            return partChosen != null;
        });

        // Once a part is selected, get its Rigidbody2D and create a hinge
        var partScript = partChosen.GetComponent<BasePart>();
        var bodyPart = partScript.Body;

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        HingeConnection hingeConnection = new HingeConnection()
        {
            hingePosition = mouseWorldPosition,
            bodyPartUniqueID = partScript.uniqueID,
            bodyPartHingePosition = partScript.transform.position,
        };
        screw.CreateHingeWithMousePos(bodyPart, hingeConnection);
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
    public void SetEditMode(EditMode mode)
    {
        isEditScrewPosition = (mode == EditMode.ScrewPosition);
        isEditScrewColor = (mode == EditMode.ScrewColor);
        isEditHinge = (mode == EditMode.Hinge);
        isEditPartPosition = (mode == EditMode.PartPosition);
        isEditPartColor = (mode == EditMode.PartColor);
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
}

public enum EditMode
{
    ScrewPosition,
    ScrewColor,
    Hinge,
    PartPosition,
    PartColor
}