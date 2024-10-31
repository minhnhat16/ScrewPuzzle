using System.DataBase;
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
        private void SetButtonInteractAble(bool isInteractable)
        {
            nextLevelButton.interactable = isInteractable;

        }
        public override void OnEndHideDialog()
        {
            base.OnEndHideDialog();
            SetButtonInteractAble(true);

        }
        private void OnNextButtonClicked()
        {
            SetButtonInteractAble(false);
            nextLevelButton.interactable = false;
            var levelManager = LevelManager.Instance;
            int currentLevel = levelManager.currentLevelID + 1 ;

            LevelData data = new();
            data.levelStar = 3;
            data.levelID = currentLevel;
            data.isCompleted = true;
            DataAPIController.instance.SaveNewLevelData(data, () =>
            {
                DialogManager.Instance.HideDialog(dialogIndex);
                levelManager.Reset();
                levelManager.LoadLevel(currentLevel);

            });
        }
    }
}
