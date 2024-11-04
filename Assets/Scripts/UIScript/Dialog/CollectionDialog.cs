using Managers;
using System.Collections.Generic;
using System.DataBase;
using UIScript.UI.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CollectionDialog : BaseDialog
{
    public LableTab currentTab;
    public Text gold_lb;
    public Text gem_lb;

    private int gold;
    private int gem;
    public RectTransform bgGroup;
    public RectTransform boardGroup;
    public RectTransform screwGroup;

    public Toggle bgToggle;
    public Toggle boardToggle;
    public Toggle screwToggle;

    public ToggleGroup _togglesCollection;
    public ToggleGroup _toggleBGs;
    public ToggleGroup _toggleBoardColors;
    public ToggleGroup _toggleScrews;

    [SerializeField] Button btn_Setting;
    [HideInInspector]
    public UnityEvent<CollectionLable> onClickBg = new();
    public UnityEvent<CollectionLable> onClickBoardColor = new();
    public UnityEvent<CollectionLable> onClickedScrewColor = new();

    public UnityEvent<int> onChangeBG = new();
    public UnityEvent<int> onChangeBoardColor = new();
    public UnityEvent<int> onChangeScrew = new();

    private void OnEnable()
    {
        screwToggle.onValueChanged.AddListener(BackGroundClicked);
    }
    private void OnDisable()
    {
        //onGoldChanged.RemoveListener(GoldChange);
     /*   onClickBg.RemoveListener(BackGroundClicked);
        onClickBoardColor.RemoveListener(BoardColorClicked);
        onClickedScrewColor.RemoveListener(ScrewColorClicked);*/
    }
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        Debug.Log("Set up before show dialog");
        InitBackGroundToggleGroup();
    }
    public override void OnStartShowDialog()
    {
      
       /* if (ViewManager.Instance.currentView.viewIndex == ViewIndex.MainScreenView)
        {
            Debug.Log("MainScreenView");
            var view = ViewManager.Instance.currentView as MainScreenView;
            view.SetLevelPanelIs(true);
        }
        else if (ViewManager.Instance.currentView.viewIndex == ViewIndex.CollectionView)
        {
            Debug.Log("CollectionView   ");
        }
        else
        {
        }*/
    }
    private void InitBackGroundToggleGroup()
    {
        List<CollectionItem> items = new List<CollectionItem>(_toggleBGs.GetComponentsInChildren<CollectionItem>());
        Debug.Log(" collection item count " + items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Init(i);
        }
        items[0].Toggle.onValueChanged.Invoke(true);

    }

    void BoardColorClicked(CollectionLable lable)
    {
        if (lable != CollectionLable.BoardColor) return;
        SwitchButtonChose(lable);
        Debug.Log("home clicked");
        if (ViewManager.Instance.currentView.viewIndex != ViewIndex.MainScreenView) ViewManager.Instance.SwitchView(ViewIndex.MainScreenView);

    }
    void BackGroundClicked(bool isChangedValue)
    {
        bgGroup.gameObject.SetActive(isChangedValue);

    }
    void ScrewColorClicked(CollectionLable lable)
    {
        if (lable != CollectionLable.Screw) return;
        SwitchButtonChose(lable);
        DialogManager.Instance.ShowDialog(DialogIndex.SpinDialog, null, () =>
         {
             if (ViewManager.Instance.currentView.viewIndex == ViewIndex.MainScreenView)
             {
                 var main = ViewManager.Instance.currentView as MainScreenView;
                 main.SetLevelPanelIs(false);
             }
         });

    }
    void SwitchButtonChose(CollectionLable lable)
    {
    }
   
   
}
