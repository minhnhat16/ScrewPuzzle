using System;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenView : BaseView
{
    //public int totalGold;
    //public TextMeshProUGUI gold_lb;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button dailyReward;
    [SerializeField] private LevelPanel levelPanel;
    private void OnEnable()
    {
        /*playBtn.onClick.AddListener(OnPlayButton);
        dailyReward.onClick.AddListener(OnDailyReward);*/
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
}
