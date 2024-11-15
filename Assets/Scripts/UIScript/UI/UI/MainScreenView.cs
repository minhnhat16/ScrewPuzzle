using Managers;
using System;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.UI.UI
{
    public class MainScreenView : BaseView
    {
        [SerializeField] private Button playBtn;
        [SerializeField] private Button dailyReward;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button levelButton;
        [SerializeField] private Button rateButton;
        [SerializeField] private Button skinButton;
        [SerializeField] private Button specialButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button adsRemover;
        [SerializeField] private Text goldLB;
        [SerializeField] private LevelPanel levelPanel;
        [SerializeField] private int gold;

        private void OnEnable()
        {
            /*playBtn.onClick.AddListener(OnPlayButton);
        dailyReward.onClick.AddListener(OnDailyReward);*/
            shopButton.onClick.AddListener(ShopButton);
            levelButton.onClick.AddListener(LevelButton);
            dailyReward.onClick.AddListener(OnDailyReward);
            rateButton.onClick.AddListener(RateButton);
            playBtn.onClick.AddListener(OnPlayButton);
            skinButton.onClick.AddListener(OnSkinButton);
            specialButton.onClick.AddListener(OnClickSpecialButton);
            settingButton.onClick.AddListener(OnClickSettingButton);
            adsRemover.onClick.AddListener(OnClickAdsRemover);
        }

      

        private void OnDisable()
        {
            shopButton.onClick.RemoveListener(ShopButton);
            levelButton.onClick.RemoveListener(LevelButton);
            dailyReward.onClick.RemoveListener(OnDailyReward);
            rateButton.onClick.RemoveListener(RateButton);
            playBtn.onClick.RemoveListener(OnPlayButton);
        }
        public override void OnStartShowView()
        {
            int currentPlayerLevel = DataAPIController.instance.GetPlayerLevel();
            LevelManager.Instance.currentLevelID = currentPlayerLevel;
            base.OnStartShowView();
            SetLevelPanelIs(true);
        }
        public override void OnStartHideView()
        {
            base.OnStartHideView();
            SetLevelPanelIs(false);

        }
        public override void OnInit()
        {
            base.OnInit();
        }
        public override void Setup(ViewParam viewParam)
        {
            base.Setup(viewParam);

            MainScreenViewParam param = viewParam as MainScreenViewParam;
            int userGold = gold = param.totalGold;

            SetUpGold(userGold);
            SetLevelPanelIs(true);
        }

        private void SetUpGold(int userGold)
        {
            goldLB.text = GameManager.instance.DevideCurrency(userGold);
        }
        private void OnDailyReward()
        {
            SetLevelPanelIs(false);
            DailyParam param = new()
            {
                config = ConfigFileManager.Instance.DailyRewardConfig,
                data = DataAPIController.instance.GetDailyData(),
                totalGold = GameManager.instance.GetPlayerGold(),
            };
            DialogManager.Instance.ShowDialog(DialogIndex.DailyRewardDialog, param, null);
        }
        private void OnSkinButton()
        {
            CollectionDialogParam param = new()
            {
                collection = ConfigFileManager.Instance.CollectionConfig,
                currentSkin = DataAPIController.instance.GetCurrentScrewData(),
                currentBG = DataAPIController.instance.GetCurrentBackGroundData(),
                currentBoard = DataAPIController.instance.GetCurrentBoardData(),
                totalGold = GameManager.instance.GetPlayerGold(),
            };

            DialogManager.Instance.ShowDialog(DialogIndex.CollectionDialog, param, null);

        }

        private void OnClickSpecialButton()
        {
            SpecialDialogParam param = new();
            param.isPaymentAvailable = true;
            param.isPaid = false;

            param.time = DateTime.Now.AddDays(2).ToString();
            param.price = 600000;
            param.currency = "VND";
            param.totalGold = GameManager.instance.GetPlayerGold();
            List<ShopItem> specialItems = new List<ShopItem>();

            DialogManager.Instance.ShowDialog(DialogIndex.SpecialDialog, param, null);
        }
        private void RateButton()
        {
            DialogManager.Instance.ShowDialog(DialogIndex.RateDialog);
        }

        public override void OnInit(Action callback)
        {
            levelPanel.Init(callback);

        }
        public void SetLevelPanelIs(bool isOn)
        {
        }
        private void OnPlayButton()
        {
            int currentLevel = LevelManager.Instance.currentLevelID;
            LevelManager.Instance.LoadLevel(currentLevel);
        }

        public void SpinView()
        {
            ///Debug.Log("View SPin Button");

            ViewManager.Instance.SwitchView(ViewIndex.CollectionView);
        }

        private void OnClickSettingButton()
        {
            SettingParam param = new();
            param.isMainScreen = viewIndex.Equals(ViewIndex.MainScreenView);
            param.totalGold = GameManager.instance.GetPlayerGold();
            if (param.totalGold == null) Debug.Log("total gold is null");
            param.title = "SETTING";
            DialogManager.Instance.ShowDialog(DialogIndex.SettingDialog, param);
        }
        public void ShopButton()
        {
            var param = new ShopViewParam();
            param.gold = gold;
            ViewManager.Instance.SwitchView(ViewIndex.ShopView, param);
        }
        private void OnClickAdsRemover()
        {
            AdsRemoveParam param = new();
            param.isPaymentAvailable = true;
            param.isPaid = false;

            param.price = 600000;
            param.currency = "VND";
            param.totalGold = GameManager.instance.GetPlayerGold();

            DialogManager.Instance.ShowDialog(DialogIndex.AdsRemoveDialog, param, null);
        }
        private void LevelButton()
        {
            var levelsConfig = LevelManager.Instance.levelConfig;
            var levelData = DataAPIController.instance.GetAllLevelData();
            List<BaseLevelItem> listLevel = new();

            foreach (var levelConfig in levelsConfig)
            {
                int id = levelConfig.levelId;
                var currentLevel = levelData.Find((data) => data.levelID == id);
                bool isComplete = currentLevel?.isCompleted == true;

                Debug.LogError($"CURRENT LEVEL {id} DATA {currentLevel} and isComplete {isComplete}");
                // Create new LevelItem and add to the list
                BaseLevelItem newItem = new BaseLevelItem(id, isComplete, false);
                listLevel.Add(newItem);
            }

            // Set parameters and switch view
            LevelParam param = new()
            {
                currentLevel = DataAPIController.instance.GetPlayerLevel(),
                listLevelItems = listLevel
            };
            ViewManager.Instance.SwitchView(ViewIndex.LevelView, param);
        }

    }
}
