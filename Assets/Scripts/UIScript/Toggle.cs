using System;
using UnityEngine;
using UnityEngine.UI;

    // Rename this class to avoid circular inheritance with UnityEngine.UI.Toggle
    [Serializable]
    public class CustomToggle : MonoBehaviour
    {
        public Toggle m_Toggle;
        [SerializeField]
        private Image activeIcon;
        [SerializeField]
        private Image disabledIcon;

        protected   void OnEnable()
        {
            m_Toggle.onValueChanged.AddListener(SwapSprite);
        }
        protected  void OnDisable()
        {
            m_Toggle.onValueChanged.RemoveListener(SwapSprite);
        }
        private void SwapSprite(bool value)
        {
            float activeAlpha = value ? 1f : 0f;
            float disabledAlpha = value ? 0f : 1f;

            activeIcon.CrossFadeAlpha(activeAlpha, 0.2f, false);
            disabledIcon.CrossFadeAlpha(disabledAlpha, 0.2f, false);
        }

        private void Start()
        {
            SwapSprite(true);
        }
    }
