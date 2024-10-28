using System;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenView : BaseView
{
    [SerializeField] private Button playBtn;
    [SerializeField] private Button dailyReward;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button levelButton;
    [SerializeField] private LevelPanel levelPanel;
    [SerializeField] private int gold;

    private void OnEnable()
    {
        /*playBtn.onClick.AddListener(OnPlayButton);
        dailyReward.onClick.AddListener(OnDailyReward);*/
        shopButton.onClick.AddListener(ShopButton);
        levelButton.onClick.AddListener(LevelButton);
    }
    private void OnDisable()
    {
        /*playBtn.onClick.RemoveListener(OnPlayButton);*/
    }
    public override void OnStartShowView()
    {
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
        DailyParam param = new();
        param.config = ConfigFileManager.Instance.DailyRewardConfig;
        DialogManager.Instance.ShowDialog(DialogIndex.DailyRewardDialog,param, null);
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

    }
    public void DailyRewardButton()
    {
        //Debug.Log("Daily Reward Button");
        DialogManager.Instance.ShowDialog(DialogIndex.DailyRewardDialog);
    }
    public void SpinView()
    {
        ///Debug.Log("View SPin Button");

        ViewManager.Instance.SwitchView(ViewIndex.CollectionView);
    }
   public void OnPanelCentered(int center, int selected)
    {
        LevelItem item = LevelItemPool.Instance.pool.list[center];
        //playBtn.interactable = isUnlocked;
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
            bool isComplete = currentLevel is { isCompleted: true };
            // Create new LevelItem and add to the list
            LevelItem newItem = new(id, isComplete, false);
            listLevel.Add(newItem);
        }

        // Set parameters and switch view
        LevelParam param = new()
        {
            currentLevel = 1,
            listLevelItems = listLevel
        };
        ViewManager.Instance.SwitchView(ViewIndex.LevelView, param);
    }

}
