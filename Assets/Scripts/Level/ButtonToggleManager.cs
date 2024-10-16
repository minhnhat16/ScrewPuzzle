using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Level
{
    public class ButtonToggleManager : MonoBehaviour
    {
        public List<Button> buttons;
        private Dictionary<Button, bool> _buttonStates; // Dictionary to track button states

        void Start()
        {
            InitButtons();
        }

        void InitButtons()
        {
            _buttonStates = new Dictionary<Button, bool>();

            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i; // Avoid closure issue
                Button btn = buttons[i];
                _buttonStates[btn] = false; // Initialize all buttons as unclicked

                // Add listener for each button
                btn.onClick.AddListener(() => OnButtonClicked(btn));
            }
        }

        void OnButtonClicked(Button btn)
        {
            bool isClicked = _buttonStates[btn];

            if (!isClicked)
            {
                // First click action (button was not clicked before)
                Debug.Log($"{btn.name} clicked for the first time.");
                // Add your activation logic here (e.g., change color, enable edit mode, etc.)
            }
            else
            {
                // Second click action (button was clicked before, so toggle off)
                Debug.Log($"{btn.name} clicked again. Toggling off.");
                // Add your deactivation logic here
            }

            // Toggle the button state
            _buttonStates[btn] = !isClicked;
        }
    }
}