#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorDropDown : MonoBehaviour
{
   public List<Button> buttons = new List<Button>();
    // Start is called before the first frame update
    void Start()
    {
        InitButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i  ; // Local copy to avoid closure issues
            buttons[i].GetComponentInChildren<Text>().text = $"Color {index}";
            
            // Add onClick event listener
            buttons[i].onClick.AddListener(() => OnButtonClicked(index + 2));
        }
    }

    void OnButtonClicked(int buttonIndex)
    {
        Debug.Log($"Button {buttonIndex} clicked!");
        LevelMaker.instance.currentScrewColorID = buttonIndex;
        // You can handle what happens when the button is clicked here
        // For example, use buttonIndex to select a color or perform an action.
    }
}
#endif