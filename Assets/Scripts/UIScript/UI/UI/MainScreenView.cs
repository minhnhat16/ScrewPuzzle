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

            SetLevelPanelIs(true);
        }
        private void OnDailyReward()
        {
            SetLevelPanelIs(false);
            DailyParam param = new()
            {
                config = ConfigFileManager.Instance.DailyRewardConfig,
                data = DataAPIController.instance.GetDailyData(),
            };
            DialogManager.Instance.ShowDialog(DialogIndex.DailyRewardDialog,param, null);
        }
        private void OnSkinButton()
        {
            CollectionDialogParam param = new();
            param.collection = ConfigFileManager.Instance.CollectionConfig;
            //var screwCollectionData = DataAPIController.instance.GetAllScrewSkinData();
            //var boardCollectionData = DataAPIController.instance.GetBoardData();
            DialogManager.Instance.ShowDialog(DialogIndex.CollectionDialog, param, null);

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
            int currentLevel =   LevelManager.Instance.currentLevelID;
            LevelManager.Instance.LoadLevel(currentLevel);
        }
    
        public void SpinView()
        {
            ///Debug.Log("View SPin Button");

            ViewManager.Instance.SwitchView(ViewIndex.CollectionView);
        }
     

        public void ShopButton()
        {
            var param = new  ShopViewParam();
            param.gold = gold;
            ViewManager.Instance.SwitchView(ViewIndex.ShopView,param);
        }
        private void LevelButton()
        {
            var levelsConfig = LevelManager.Instance.levelConfig;
            var levelData = DataAPIController.instance.GetAllLevelData();
            List<LevelItem> listLevel = new();

            foreach (var levelConfig in levelsConfig)
            {
                int id = levelConfig.levelId;
                var currentLevel= levelData.Find((data)=>data.levelID == id);
                bool isComplete = currentLevel?.isCompleted == true;

                Debug.LogError($"CURRENT LEVEL {id} DATA {currentLevel} and isComplete {isComplete}");
                // Create new LevelItem and add to the list
                LevelItem newItem = new()
                {
                    IDLevel = id,
                    IsCompleted = isComplete,
                    IsHardLevel = false
                };
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
