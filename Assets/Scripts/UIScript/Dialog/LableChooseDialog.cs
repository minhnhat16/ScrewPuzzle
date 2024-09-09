using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LableChooseDialog : BaseDialog
{
    public List<LableTab> lableList;
    public LableTab currentTab;
    public Text gold_lb;
    public Text gem_lb;

    private int gold;
    private int gem;
    [SerializeField] Button btn_Setting;
    [HideInInspector]
    public UnityEvent<Lable> onClickedRate = new();
    public UnityEvent<Lable> onClickedHome = new();
    public UnityEvent<Lable> onClickedSpin = new();
    public UnityEvent<Lable> onClickedCollection = new();
    private void OnEnable()
    {
        btn_Setting.onClick.AddListener(SettingDialogButton);
        onClickedRate = lableList[0].onChooseLable;
        onClickedHome = lableList[1].onChooseLable;
        onClickedSpin = lableList[2].onChooseLable;
        onClickedCollection = lableList[3].onChooseLable;
        onClickedRate.AddListener(RateClicked);
        onClickedHome.AddListener(HomeClicked);
        onClickedSpin.AddListener(SpinClicked);
        onClickedCollection.AddListener(CollectionClicked);
        DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, (data) =>
        {
            if (data == null) return;
            CurrencyWallet newData = data as CurrencyWallet;
            gold = newData.amount;
            gold_lb.text = GameManager.instance.DevideCurrency(gold);
        });
        DataTrigger.RegisterValueChange(DataPath.GEMINVENT, (data) =>
        {
            if (data == null) return;
            CurrencyWallet newData = data as CurrencyWallet;
            gem = newData.amount;
            gem_lb.text = GameManager.instance.DevideCurrency(gem);
        });
    }
    private void OnDisable()
    {
        //onGoldChanged.RemoveListener(GoldChange);
        onClickedRate.RemoveListener(RateClicked);
        onClickedHome.RemoveListener(HomeClicked);
        onClickedSpin.RemoveListener(SpinClicked);
        onClickedCollection.RemoveListener(CollectionClicked);
    }
    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        gold = DataAPIController.instance.GetGold();
        gem = DataAPIController.instance.GetGem();
        gold_lb.text = GameManager.instance.DevideCurrency(gold);
        gem_lb.text = GameManager.instance.DevideCurrency(gem);
        currentTab = lableList[1];
        if (ViewManager.Instance.currentView.viewIndex == ViewIndex.MainScreenView)
        {
            Debug.Log("MainScreenView");
            GetLable(Lable.Home).OnButtonClicked();
            var view = ViewManager.Instance.currentView as MainScreenView;
            view.SetLevelPanelIs(true);
        }
        else if (ViewManager.Instance.currentView.viewIndex == ViewIndex.CollectionView)
        {
            Debug.Log("CollectionView   ");
            GetLable(Lable.Collection).OnButtonClicked();
        }
        else
        {
            GetLable(Lable.Home).OnButtonClicked();
            //var view = ViewManager.Instance.currentView as MainScreenView;
            //view.SetLevelPanelIs(true);
        }
    }
    void HomeClicked(Lable lable)
    {
        if (lable != Lable.Home) return;
        SwitchButtonChose(lable);
        Debug.Log("home clicked");
        if(ViewManager.Instance.currentView.viewIndex != ViewIndex.MainScreenView) ViewManager.Instance.SwitchView(ViewIndex.MainScreenView);

    }
    void RateClicked(Lable lable)
    {
        if (lable != Lable.Rate) return;
        SwitchButtonChose(lable);
        DialogManager.Instance.ShowDialog(DialogIndex.RateDialog, null, () =>
        {
            if (ViewManager.Instance.currentView.viewIndex != ViewIndex.MainScreenView) return;
           var main =  (MainScreenView)ViewManager.Instance.currentView;
            main.SetLevelPanelIs(false); 
        });
    }
    void SpinClicked(Lable lable)
    {
        if (lable != Lable.Spin) return;
        SwitchButtonChose(lable);
        DialogManager.Instance.ShowDialog(DialogIndex.SpinDialog,null, () =>
        {
            if(ViewManager.Instance.currentView.viewIndex == ViewIndex.MainScreenView)
            {
                var main = ViewManager.Instance.currentView as MainScreenView;
                main.SetLevelPanelIs(false);
            }
        });

    }
    void CollectionClicked(Lable lable)
    {
        if (lable != Lable.Collection) return;
        SwitchButtonChose(lable);
        CollectionParam param = new();
        param.ownedCard = DataAPIController.instance.GetCardDataCount(IngameController.instance.CurrentCardType);
        param.totalCard = ConfigFileManager.Instance.ColorConfig.GetAllRecord().Count;
        if (ViewManager.Instance.currentView.viewIndex != ViewIndex.CollectionView)
        {
            ViewManager.Instance.SwitchView(ViewIndex.CollectionView,param);
        }

        }
        void SwitchButtonChose(Lable lable)
    {
       foreach(var tab in lableList)
        {
            if (tab.type != lable) tab.OnButtonUnchose();
            //else tab.OnButtonClicked();
        }
    }
    LableTab GetLable(Lable lable)
    {
        return lableList[(int)lable];
    }
    public void SettingDialogButton()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        SettingParam param = new();
        param.isMainScreen = true;
        DialogManager.Instance.ShowDialog(DialogIndex.SettingDialog, param, null);
    }
    public void DailyDialogButton()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        DialogManager.Instance.ShowDialog(DialogIndex.DailyRewardDialog, null);

    }
    public void PauseDialogOff()
    {
        DialogManager.Instance.HideDialog(DialogIndex.SettingDialog);
    }
}
