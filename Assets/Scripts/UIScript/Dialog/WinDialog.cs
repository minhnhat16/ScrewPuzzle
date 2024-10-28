using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class WinDialog : BaseDialog
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Text score;
        [SerializeField] private Image rewardImg;

        private void OnEnable()
        {
            nextLevelButton.onClick.AddListener(OnNextButtonClicked);
        }

        private void OnNextButtonClicked()
        {
            var levelManager = LevelManager.Instance;
            int currentLevel = levelManager.currentLevelID; 
            levelManager.LoadLevel(++currentLevel);
            DialogManager.Instance.HideDialog(dialogIndex);
        }
    }
}
