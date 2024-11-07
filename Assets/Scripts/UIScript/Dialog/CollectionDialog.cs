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
    public GoldDisplay goldDisplay;
    public RectTransform bgGroup;
    public RectTransform boardGroup;
    public RectTransform screwGroup;

    public Button closeBtn;

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
        closeBtn.onClick.AddListener(OnCloseButton);
    }
    private void OnDisable()
    {
        bgToggle.onValueChanged.RemoveListener(BackGroundClicked);
        screwToggle.onValueChanged.RemoveListener(ScrewColorClicked);
        boardToggle.onValueChanged.RemoveListener(BoardColorClicked);
        closeBtn.onClick.RemoveListener(OnCloseButton);
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
        var crBackGround = param.currentBG;
        var crScrewSkin = param.currentSkin;
        var crBoardColor = param.currentBoard;

        goldDisplay.SetGoldToLable(param.totalGold);

        InitBackGroundToggleGroup(crBackGround,_bgRecords);
        InitBoardToggleGroup(crBoardColor,_boardRecord);
        InitScrewColorToggleGroup(crScrewSkin,_screwSkinRecord);

        bgToggle.isOn = true;
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
    private void InitBackGroundToggleGroup(BackGroundData crBackGround,List<CollectionConfigRecord> records)
    {
        List<CollectionItem> items = new List<CollectionItem>(_toggleBGs.GetComponentsInChildren<CollectionItem>());
        Debug.Log(" collection item count " + items.Count);
        if (records.Count <= 0) return;
        for (int i = 0; i < records.Count; i++)
        {
            string spriteName = records[i].iconName;
            var sprite = System.SpriteLibControl.Instance.GetSpriteByName(spriteName);
            bool isToggleOn = spriteName.CompareTo(crBackGround.name) == 0;
            Debug.Log($"Init BackGround Toggle Group id{i} and isToggleOn:{isToggleOn}");
            items[i].Init(i, sprite, isToggleOn,CollectionLable.BackGround);
        }

    }
    private void InitBoardToggleGroup(BoardColorData crBoardColor, List<CollectionConfigRecord> records)
    {
        List<CollectionItem> items = new List<CollectionItem>(_toggleBoardColors.GetComponentsInChildren<CollectionItem>());
        Debug.Log(" collection item count " + items.Count);
        if (records.Count <= 0) return;

        for (int i = 0; i < records.Count; i++)
        {
            string spriteName = records[i].iconName;
            var sprite = System.SpriteLibControl.Instance.GetSpriteByName(spriteName);
            bool isToggleOn = spriteName.CompareTo(crBoardColor.name) == 0;
            Debug.Log($"Init Board Toggle Group id{i} and isToggleOn:{isToggleOn}");

            items[i].Init(i, sprite, isToggleOn, CollectionLable.BoardColor);

        }
    }
    private void InitScrewColorToggleGroup(ScrewSkinData crScrewSkin, List<CollectionConfigRecord> records)
    {
        List<CollectionItem> items = new List<CollectionItem>(_toggleScrews.GetComponentsInChildren<CollectionItem>());
        Debug.Log(" collection item count " + items.Count);
        if (records.Count <= 0) return;

        for (int i = 0; i < records.Count; i++)
        {
            string spriteName = records[i].iconName;
            var sprite = System.SpriteLibControl.Instance.GetSpriteByName(spriteName);
            bool isToggleOn = spriteName.CompareTo(crScrewSkin.name) == 0;
            Debug.Log($"{i} and isToggleOn:{isToggleOn}");

            items[i].Init(i, sprite, isToggleOn, CollectionLable.Screw);
        }
    }
    void BoardColorClicked(bool isChangedValue)
    {
        boardToggle.interactable = !isChangedValue;
        boardGroup.gameObject.SetActive(isChangedValue);
        bgGroup.gameObject.SetActive(!isChangedValue);
        screwGroup.gameObject.SetActive(!isChangedValue);

    }
    void BackGroundClicked(bool isChangedValue)
    {
        bgToggle.interactable = !isChangedValue;

        bgGroup.gameObject.SetActive(isChangedValue);
        boardGroup.gameObject.SetActive(!isChangedValue);
        screwGroup.gameObject.SetActive(!isChangedValue);

    }
    void ScrewColorClicked(bool isChangedValue)
    {

        screwToggle.interactable = !isChangedValue;
        screwGroup.gameObject.SetActive(isChangedValue);
        bgGroup.gameObject.SetActive(!isChangedValue);
        boardGroup.gameObject.SetActive(!isChangedValue);
    }

    public void OnCloseButton()
    {
        
        DialogManager.Instance.HideDialog(dialogIndex);
    }

}
