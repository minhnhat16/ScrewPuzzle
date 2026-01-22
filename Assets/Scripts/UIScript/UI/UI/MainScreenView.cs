using Managers;
using System;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
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
        [SerializeField] private LevelPanel levelPanel;
        [SerializeField] private long gold;
        [SerializeField] private Text level_txt;

        [SerializeField] private GoldDisplay goldDp;
        [SerializeField] private GoldDisplay ticketDp;
        private void OnEnable()
        {
            /*playBtn.onClick.AddListener(OnPlayButton);
        dailyReward.onClick.AddListener(OnDailyReward);*/
            shopButton.onClick.AddListener(ShopButton);
            levelButton.onClick.AddListener(LevelButton);
            dailyReward.onClick.AddListener(SpinView);
            rateButton.onClick.AddListener(RateButton);
            playBtn.onClick.AddListener(OnPlayButton);
            specialButton.onClick.AddListener(OnClickSpecialButton);
            settingButton.onClick.AddListener(OnClickSettingButton);
            adsRemover.onClick.AddListener(OnClickAdsRemover);

            DataTrigger.RegisterValueChange(DataPath.TICKET, OnTicketChanged);
            DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
        }



        private void OnDisable()
        {
            shopButton.onClick.RemoveListener(ShopButton);
            levelButton.onClick.RemoveListener(LevelButton);
            dailyReward.onClick.RemoveListener(SpinView);
            rateButton.onClick.RemoveListener(RateButton);
            playBtn.onClick.RemoveListener(OnPlayButton);
            levelButton.onClick.RemoveAllListeners();
            settingButton.onClick.RemoveAllListeners();
            adsRemover.onClick.RemoveAllListeners();


            DataTrigger.UnRegisterValueChange(DataPath.TICKET, OnTicketChanged);
            DataTrigger.UnRegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
        }
        public override void OnStartShowView()
        {
            int currentPlayerLevel = DataAPIController.instance.GetPlayerLevel();
            LevelManager.ins.currentLevelID = currentPlayerLevel;
            SetUpLevel(currentPlayerLevel);
            base.OnStartShowView();
            SetLevelPanelIs(true);
        }
        public override void OnStartHideView()
        {
            base.OnStartHideView();
            SetLevelPanelIs(false);

        }

        public override void Setup(ViewParam viewParam)
        {
            base.Setup(viewParam);

            MainScreenViewParam param = viewParam as MainScreenViewParam;
            if (param != null)
            {
                long userGold = gold = param.totalGold;
                int ticket = param.ticket;
                SetUpGold(userGold);
                goldDp.SetGoldToLable(userGold);
                ticketDp.SetGoldToLable(ticket);
                SetLevelPanelIs(true);
                SetUpLevel(param.level);
            }


        }

        private void SetUpLevel(int level)
        {
            level_txt.text = $"Level {level}";
        }

        private void SetUpGold(long userGold)
        {
            //goldLB.text = GameManager.instance.DevideCurrency(userGold);
        }
        private void OnDailyReward()
        {
            SetLevelPanelIs(false);
            //DailyParam param = new()
            //{
            //    config = ConfigFileManager.Instance.GetConfig<Dx`ailyRewardConfig>(),
            //    data = DataAPIController.instance.GetDailyData(),
            //    totalGold = GameManager.instance.GetPlayerGold(),
            //};

            GiftParam param = new GiftParam();

            var record = ConfigExtensions.GetRewardConfig(ConfigFileManager.Instance, 1);    


            param.rewards = record.Items; 
            DialogManager.ins.ShowDialog(DialogIndex.GiftClaimDialog, param, null);
        }
        private void OnSkinButton()
        {


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

            DialogManager.ins.ShowDialog(DialogIndex.SpecialDialog, param, null);
        }
        private void RateButton()
        {
            DialogManager.ins.ShowDialog(DialogIndex.RateDialog);
        }

        public override void OnInit(Action callback = null)
        {
            levelPanel?.Init(callback);
            base.OnInit(callback);
        }
        public void SetLevelPanelIs(bool isOn)
        {
        }
        private void OnPlayButton()
        {
            int currentLevel = LevelManager.ins.currentLevelID;


            LevelManager.ins.LoadLevel(currentLevel, () =>
            {
                IngameController.ins.PauseGame();
            });
        }

        public void SpinView()
        {
            PuzzleParam param = new PuzzleParam();  
            param.idPuzzle = 2; // Example puzzle ID for spin view
            param.progress = 0f;
            param.target = 10f;
            param.currentTool = 100;
            ViewManager.Instance.SwitchView(ViewIndex.PuzzleView,param);
        }

        private void OnClickSettingButton()
        {
            SettingParam param = new();
            param.isMainScreen = viewIndex.Equals(ViewIndex.MainScreenView);
            param.totalGold = WalletManager.ins.Get(Currency.Gold);
            param.totalTicket = WalletManager.ins.Get(Currency.Ticket);
            param.title = "SETTING";
            DialogManager.ins.ShowDialog(DialogIndex.SettingDialog, param);
        }
        public void ShopButton()
        {

            Debug.Log("Shop button on clicked");
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

            DialogManager.ins.ShowDialog(DialogIndex.QuestDialog, param, null);
        }
        private void LevelButton()
        {
            var levelsConfig = LevelManager.ins.levelConfig;
            var levelData = DataAPIController.instance.GetAllLevelData();
            List<BaseLevelItem> listLevel = new();

            foreach (var levelConfig in levelsConfig)
            {
                int id = levelConfig.levelId;
                var currentLevel = levelData.Find((data) => data.levelID == id);
                bool isComplete = currentLevel?.isCompleted == true;

                //Debug.LogError($"CURRENT LEVEL {id} DATA {currentLevel} and isComplete {isComplete}");
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
        private void OnTicketChanged(object arg0)
        {
            long ticket = DataAPIController.instance.GetTicket();
            ticketDp.SetGoldToLable(ticket);
        }

        private void OnGoldChanged(object arg0)
        {
            long ticket = DataAPIController.instance.GetGold();
            ticketDp.SetGoldToLable(ticket);
        }

    }
}
