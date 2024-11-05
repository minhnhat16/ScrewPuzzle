using System.Collections.Generic;
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

    public List<CollectionConfigRecord> collectionBG;
    public List<CollectionConfigRecord> collectionBoard;
    public List<CollectionConfigRecord> collectionScrew;
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
        bgToggle.onValueChanged.AddListener(BackGroundClicked);
        screwToggle.onValueChanged.AddListener(ScrewColorClicked);
        boardToggle.onValueChanged.AddListener(BoardColorClicked);
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
        CollectionDialogParam param = (CollectionDialogParam)dialogParam;
        var _collectionRecords = param.collection.GetAllRecord();
        var _screwSkinRecord = collectionScrew = _collectionRecords.FindAll(item => item.type == CollectionLable.Screw);
        var _boardRecord = collectionBoard = _collectionRecords.FindAll(item => item.type == CollectionLable.BoardColor);
        var _bgRecords = collectionBG = _collectionRecords.FindAll(item => item.type == CollectionLable.BackGround);

        InitBackGroundToggleGroup(_bgRecords);
        InitBoardToggleGroup(_boardRecord);
        InitScrewColorToggleGroup(_screwSkinRecord);
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
    private void InitBackGroundToggleGroup(List<CollectionConfigRecord> records)
    {
        List<CollectionItem> items = new List<CollectionItem>(_toggleBGs.GetComponentsInChildren<CollectionItem>());
        Debug.Log(" collection item count " + items.Count);
        if (records.Count <= 0) return;
        for (int i = 0; i < records.Count; i++)
        {
            string spriteName = records[i].iconName;
            var sprite = System.SpriteLibControl.Instance.GetSpriteByName(spriteName);
            items[i].Init(i, sprite);
        }
        items[0].Toggle.onValueChanged.Invoke(true);

    }
    private void InitBoardToggleGroup(List<CollectionConfigRecord> records)
    {
        List<CollectionItem> items = new List<CollectionItem>(_toggleBoardColors.GetComponentsInChildren<CollectionItem>());
        Debug.Log(" collection item count " + items.Count);
        if (records.Count <= 0) return;

        for (int i = 0; i < records.Count; i++)
        {
            string spriteName = records[i].iconName;
            var sprite = System.SpriteLibControl.Instance.GetSpriteByName(spriteName);
            items[i].Init(i, sprite);
        }
        items[0].Toggle.onValueChanged.Invoke(true);
    }
    private void InitScrewColorToggleGroup(List<CollectionConfigRecord> records)
    {
        List<CollectionItem> items = new List<CollectionItem>(_toggleScrews.GetComponentsInChildren<CollectionItem>());
        Debug.Log(" collection item count " + items.Count);
        if (records.Count <= 0) return;
        for (int i = 0; i < records.Count; i++)
        {
            string spriteName = records[i].iconName;
            var sprite = System.SpriteLibControl.Instance.GetSpriteByName(spriteName);
            items[i].Init(i, sprite);
        }
        items[0].Toggle.onValueChanged.Invoke(true);
    }
    void BoardColorClicked(bool isChangedValue)
    {
        boardGroup.gameObject.SetActive(isChangedValue);
        bgGroup.gameObject.SetActive(!isChangedValue);
        screwGroup.gameObject.SetActive(!isChangedValue);

    }
    void BackGroundClicked(bool isChangedValue)
    {
        bgGroup.gameObject.SetActive(isChangedValue);
        boardGroup.gameObject.SetActive(!isChangedValue);
        screwGroup.gameObject.SetActive(!isChangedValue);

    }
    void ScrewColorClicked(bool isChangedValue)
    {
        screwGroup.gameObject.SetActive(isChangedValue);
        bgGroup.gameObject.SetActive(!isChangedValue);
        boardGroup.gameObject.SetActive(!isChangedValue);
    }

    void SwitchButtonChose(CollectionLable lable)
    {
    }


}
