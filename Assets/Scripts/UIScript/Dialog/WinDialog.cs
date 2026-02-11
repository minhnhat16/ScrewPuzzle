using Coffee.UIEffects;
using Coffee.UIExtensions;
using DG.Tweening;
using Managers;
using System;
using System.DataBase;
using System.Net.Sockets;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UIScript.Dialog
{
    public class WinDialog : BaseDialog
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Text score;
        [SerializeField] private Image rewardImg;
        [SerializeField] private Text levelLb;
        [SerializeField] private Text rewardLB;
        [SerializeField] private GoldDisplay goldDisplay;
        [SerializeField] private GoldDisplay ticket;
        [SerializeField] private Image fillCorn;
        [SerializeField] private RectTransform piggy;

        // new: percent label showing fill percentage
        [SerializeField] private Text fillPercentText;
        [SerializeField] WinParam param;    
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
            base.Setup(dialogParam);
            this.param = (WinParam)dialogParam;
            string levelStr = param.level.ToString();
            //string scoreStr = param.score.ToString();
            long userGold = param.totalGold;
            long ticketParam = param.ticket;
            string rewardString = param.reward.ToString();
            SetLevelLB(levelStr);
            //SetScore(scoreStr);
            SetReward(rewardString);

            goldDisplay.SetGoldToLable(userGold);
            ticket.SetGoldToLable(ticketParam);
            IngameController.ins.PauseGame();
        }

        public override void OnStartShowDialog()
        {
            Debug.Log("On start show dialog Win dialog");
            base.OnStartShowDialog();
            SoundHelper.PlaySFX(SoundManager.SFX.Win);
            SetRewardProgressCount(Random.Range(0f, 1f), () =>
            {
                PoppIcon();
            });
        }

        private void PoppIcon()
        {
            piggy.DOPunchScale(new Vector3(1.2f, 1.2f), 0.1f);
            effect?.Play();
        }

        private void SetButtonInteractAble(bool isInteractable)
        {
            Debug.Log("Set Button InteractAble " + isInteractable);
            nextLevelButton.interactable = isInteractable;

        }

        public void SetLevelLB(string text)
        {
            if (levelLb == null) return;
            levelLb.text = $"Level: {text}";
        }
        public void SetScore(string scoreString)
        {
            score.text = $"Score: {scoreString}";
        }

        private void SetReward(string rewardString)
        {
            if (rewardLB == null) return;
            rewardLB.text = $"x{rewardString}";
        }
        public override void OnEndHideDialog()
        {
            base.OnEndHideDialog();
            SetButtonInteractAble(true);
            IngameController.ins.ResumeGame();
        }

        private Tween fillTween;
        [SerializeField]
        private UIParticle effect;

        private void SetRewardProgressCount(float progress, Action callback = null)
        {
            progress = Mathf.Clamp01(progress);


            Debug.Log("Set progress count ");
            fillTween?.Kill();

            // ensure initial percent text matches current fill
            float start = fillCorn != null ? fillCorn.fillAmount : 0f;
            if (fillPercentText != null)
                fillPercentText.text = Mathf.RoundToInt(start * 100f) + "%";

            // Tween the value and update both image fill and percent text every frame
            fillTween = DOTween.To(() => start, v =>
            {
                start = v;
                if (fillCorn != null) fillCorn.fillAmount = v;
                if (fillPercentText != null) fillPercentText.text = Mathf.RoundToInt(v * 100f) + "%";
            }, progress, 1f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // ensure final value is exact
                if (fillCorn != null) fillCorn.fillAmount = progress;
                if (fillPercentText != null) fillPercentText.text = Mathf.RoundToInt(progress * 100f) + "%";
                callback?.Invoke();
            });
        }
        private void OnNextButtonClicked()
        {
            SetButtonInteractAble(false);
            nextLevelButton.interactable = false;
            var levelManager = LevelManager.ins;
            int currentLevel = levelManager.currentLevelID + 1;

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