using Managers;
using System;
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
        [SerializeField] private Text levelLb;
        [SerializeField] private Text rewardLB;
        [SerializeField] private Text goldLb;
        [SerializeField] private GoldDisplay goldDisplay;

        private void OnEnable()
        {
            nextLevelButton.onClick.AddListener(OnNextButtonClicked);
        }
        private void OnDisable()
        {
            nextLevelButton.onClick.RemoveListener(OnNextButtonClicked);
        }
        public override void Setup(DialogParam dialogParam)
        {
            WinParam param = (WinParam)dialogParam;
            string levelStr = param.level.ToString();
            string scoreStr = param.score.ToString();
            long userGold = param.totalGold;
            string rewardString = param.reward.ToString();
            SetLevelLB(levelStr);
            SetScore(scoreStr);
            SetReward(rewardString);

            goldDisplay= GetComponentInChildren<GoldDisplay>();
            goldDisplay.SetGoldToLable(userGold);
            IngameController.ins.PauseGame();
        }


        private void SetButtonInteractAble(bool isInteractable)
        {
            Debug.Log("Set Button InteractAble " + isInteractable);
            nextLevelButton.interactable = isInteractable;

        }

        public void SetLevelLB(string text)
        {
            levelLb.text = $"Level: {text}";
        }
        public void SetScore(string scoreString)
        {
            score.text = $"Score: {scoreString}";
        }

        private void SetReward(string rewardString)
        {
            rewardLB.text = $"x{rewardString}"; 
        }
        public override void OnEndHideDialog()
        {
            base.OnEndHideDialog();
            SetButtonInteractAble(true);
            IngameController.ins.ResumeGame();
        }
        private void OnNextButtonClicked()
        {
            SetButtonInteractAble(false);
            nextLevelButton.interactable = false;
            var levelManager = LevelManager.ins;
            int currentLevel = levelManager.currentLevelID + 1 ;

            LevelData data = new();
            data.levelStar = 3;
            data.levelID = currentLevel;
            data.isCompleted = true;
            levelManager.OnReset();
            DataAPIController.instance.SaveNewLevelData(data, () =>
            {
                SetButtonInteractAble(true);
                DialogManager.ins.HideDialog(dialogIndex);
                levelManager.LoadLevel(currentLevel);

            });
        }
    }
}
