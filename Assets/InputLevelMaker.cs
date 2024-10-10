using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputLevelMaker : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private InputField inputField;

    void Awake()
    {
        inputField = GetComponent<InputField>();
    }

    // Triggered when the InputField is selected
    public void OnSelect(BaseEventData eventData)
    {
        LevelMaker.instance.isInputData = true;
        Debug.Log("InputField selected for input.");
    }

    // Triggered when the InputField loses focus (you can use OnEndEdit as well)
    public void OnDeselect(BaseEventData eventData)
    {
        LevelMaker.instance.isInputData = false;

        Debug.Log("InputField deselected.");
    }

    // Optional: Trigger when input is finished (pressed enter or field is deselected)
    public void OnInputEnd(string input)
    {
        Debug.Log("Input entered: " + input);
    }
}