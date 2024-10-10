using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadLevelButton : MonoBehaviour
{
    public InputField userInputField; // Reference to the InputField from the UI
    public Text displayText;

    public GameObjectToLevelConverter converter;
    // Start is called before the first frame update
    void Start()
    {
        userInputField.text = "";
    }

    public void GetInputValue()
    {
        // Access the text entered in the input field
        string inputValue = userInputField.text;
        int parsedInt;
        // Display the input value in another UI Text component
        displayText.text = "Entered value: " + inputValue;

        if (int.TryParse(inputValue, out parsedInt))
        {
            displayText.text = "Entered integer value: " + parsedInt;
            Debug.Log("User Input (int): " + parsedInt);
            converter.LoadLevel(parsedInt);
        }
        else
        {
            // If the input is not a valid integer, display an error message
            displayText.text = "Invalid integer input. Please enter a valid number.";
            Debug.LogWarning("Invalid input. Please enter a valid integer.");
        }
    }
}
