using Coffee.UIEffects;
using Coffee.UIExtensions;
using DG.Tweening;
using Ingame;
using Managers;
using System;
using System.Collections;
using System.DataBase;
using System.Net.Sockets;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class WinDialog : BaseDialog
    {
        [Header("Reward")]
        [SerializeField] private int baseGoldReward = 50;
        [SerializeField] private int goldRewardStepPerLevel = 10;
        [SerializeField][Min(1)] private int levelsPerGrandReward = 5;
        [SerializeField][Min(0)] private int grandRewardGoldAmount = 500;

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
        private bool rewardGrantedThisShow;
        private bool grandRewardHitThisShow;

        private void OnEnable()
        {
            nextLevelButton.onClick.AddListener(OnNextButtonClicked);
            DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
            DataTrigger.RegisterValueChange(DataPath.TICKET, OnTicketChanged);
            RefreshCurrencyUI();
        }
        private void OnDisable()
        {
            nextLevelButton.onClick.RemoveListener(OnNextButtonClicked);
            DataTrigger.UnRegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
            DataTrigger.UnRegisterValueChange(DataPath.TICKET, OnTicketChanged);
        }
        public override void Setup(DialogParam dialogParam)
        {
            base.Setup(dialogParam);
            this.param = (WinParam)dialogParam;
            rewardGrantedThisShow = false;
            grandRewardHitThisShow = false;

            if (param.typeReward == 0)
                param.typeReward = ItemType.Gold;

            if (param.reward <= 0 && param.typeReward == ItemType.Gold)
                param.reward = CalculateGoldReward(param.level, out grandRewardHitThisShow);

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
            IngameController.ins.Pause();
        }

        public override void OnStartShowDialog()
        {
            Debug.Log("On start show dialog Win dialog");
            base.OnStartShowDialog();
            SoundHelper.PlaySFX(SoundManager.SFX.Win);
            GrantWinReward();
            SetRewardProgressCount(CalculateGrandRewardProgress(param.level), () =>
            {
                PoppIcon();
            });
            StartCoroutine(ShowCompletedSideMissionReward());
        }

        private IEnumerator ShowCompletedSideMissionReward()
        {
            yield return null;
            SideMissionManager.ins?.TryShowCompletedMissionRewardDialog();
        }

        private void PoppIcon()
        {
            piggy.DOPunchScale(new Vector3(1.2f, 1.2f), 0.1f);
            effect?.Play();
        }

        private int CalculateGoldReward(int completedLevelCount, out bool hitGrandReward)
        {
            completedLevelCount = Mathf.Max(1, completedLevelCount);
            int normalReward = baseGoldReward + Mathf.Max(0, completedLevelCount - 1) * goldRewardStepPerLevel;

            hitGrandReward = levelsPerGrandReward > 0 && completedLevelCount % levelsPerGrandReward == 0;
            if (hitGrandReward)
                normalReward += grandRewardGoldAmount;

            return normalReward;
        }

        private float CalculateGrandRewardProgress(int completedLevelCount)
        {
            if (levelsPerGrandReward <= 0)
                return 0f;

            completedLevelCount = Mathf.Max(1, completedLevelCount);
            int progressInCycle = completedLevelCount % levelsPerGrandReward;

            if (progressInCycle == 0)
                return 1f;

            return (float)progressInCycle / levelsPerGrandReward;
        }

        private void GrantWinReward()
        {
            if (rewardGrantedThisShow || param == null || param.reward <= 0)
                return;

            rewardGrantedThisShow = true;

            if (param.typeReward == ItemType.Gold)
            {
                var rewardAnim = FindAnyObjectByType<RewardAnimationService>();
                if (rewardAnim != null && rewardImg != null)
                    rewardAnim.SetFlyOrigin(rewardImg.rectTransform);

                WalletManager.ins.Add(Currency.Gold, param.reward);
                RewardEvents.Fire(ItemType.Gold, param.reward);
                return;
            }

            DataAPIController.instance.AddItemTotal(param.typeReward, param.reward);
            RewardEvents.Fire(param.typeReward, param.reward);
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
            rewardLB.text = grandRewardHitThisShow
                ? $"x{rewardString} BONUS"
                : $"x{rewardString}";
        }
        public override void OnEndHideDialog()
        {
            base.OnEndHideDialog();
            SetButtonInteractAble(true);
            IngameController.ins.Resume();
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

            var levelManager = LevelManager.ins;
            var ingameCtrl = IngameController.ins;

            int currentLevel = DataAPIController.instance.GetPlayerLevel();
            int nextLevel = currentLevel + 1;

            LevelData data = new()
            {
                levelStar = 3,
                levelID = currentLevel,
                isCompleted = true
            };

            // 1. Save data trước
            DataAPIController.instance.SaveNewLevelData(data, () =>
            {
                TutorialManager.ins?.ResetTutorialState();

                // 2. Reset state level cũ
                levelManager.OnReset();

                // 3. Hide win dialog + show loading screen
                DialogManager.ins.HideDialog(dialogIndex);
                ShowLoadingThenLoadLevel(levelManager, ingameCtrl, nextLevel);
            });
        }

        private void ShowLoadingThenLoadLevel(LevelManager levelManager, IngameController ingameCtrl, int nextLevel)
        {
            var loadingView = ViewManager.Instance.GetView<LoadingView>();
            loadingView.ResetProgress();
            // Hiện LoadingView ngay lập tức — reset progress về 0
            ViewManager.Instance.SwitchView(ViewIndex.LoadingView, null, () =>
            {
              
            });

            // Load level — pipeline chạy, TaskManager.TotalProgress tự cập nhật LoadingView
            levelManager.LoadLevel(nextLevel, () =>
            {
                // Pipeline xong → switch sang GameView rồi bắt đầu gameplay
                ViewManager.Instance.SwitchView(ViewIndex.GameView, null, () =>
                {
                    ingameCtrl.StartLevel();
                    SetButtonInteractAble(true);
                    Debug.Log($"[WinDialog] Level {nextLevel} loaded and started ✅");
                });
            });
        }

        private void RefreshCurrencyUI()
        {
            goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
            ticket.SetGoldToLable(DataAPIController.instance.GetTicket());
        }

        private void OnGoldChanged(object _)
        {
            goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
        }

        private void OnTicketChanged(object _)
        {
            ticket.SetGoldToLable(DataAPIController.instance.GetTicket());
        }
    }
}
