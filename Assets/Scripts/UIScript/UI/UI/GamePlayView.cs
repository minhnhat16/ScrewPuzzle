using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GamePlayView : BaseView
{
    //[HideInInspector] GamePlayAnim anim;
    [SerializeField] private RectTransform anchor;
    [SerializeField] private bool isShowingBreak;
    [SerializeField] private int _changeGold;
    [SerializeField] private int gold;
    [SerializeField] private int gem;
    [SerializeField] private float breakCounter = 300f; //

    [SerializeField] private RectTransform goldParent;
    [SerializeField] private RectTransform gemParent;
    [SerializeField] private Text gold_lb;
    [SerializeField] private Text gem_lb;
    [SerializeField] private Text magnet_lb;
    [SerializeField] private Text bomb_lb;
    [SerializeField] private Text curentCard_lb;
    [SerializeField] private Text maxCard_lb;
    [SerializeField] private Text timeCouter;
     [SerializeField] private List<RectTransform> anchorTutorials;
    [SerializeField] private Vector3 goldPos;
    [SerializeField]private Button settingBtn;
     [SerializeField] private Button addBoxBtn;
     [SerializeField] private Button addHoldBtn;
     [SerializeField] private Button clearOneScrewBtn;
    [SerializeField] bool isNewPlayer;
    [SerializeField] ExperienceBar expBar;

    [HideInInspector]
    public UnityEvent<bool> magnetItemEvent = new();
    [HideInInspector]
    public UnityEvent<bool> bombItemEvent = new();

    public Text GoldLb { get { return gold_lb; } }
    public Text GemLB { get { return gem_lb; } }

    public RectTransform Anchor { get => anchor; set => anchor = value; }
    public RectTransform GoldParent { get => goldParent; set => goldParent = value; }
    public RectTransform GemParent { get => gemParent; set => gemParent = value; }
    public List<RectTransform> AnchorTutorials { get => anchorTutorials; set => anchorTutorials = value; }
    public Button Magnet_btn { get => addBoxBtn; set => addBoxBtn = value; }
    public Button Bomb_Btn { get => addHoldBtn; set => addHoldBtn = value; }

    public UnityEvent<bool> onNewPlayer = new();

    private void OnEnable()
    {
        //DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, (data) =>
        //{
        //    if (data == null) return;
        //    CurrencyWallet newData = data as CurrencyWallet;
        //    gold = newData.amount;
        //    gold_lb.text = GameManager.instance.DevideCurrency(gold);
        //});
        //DataTrigger.RegisterValueChange(DataPath.GEMINVENT, (data) =>
        //{
        //    if (data == null) return;
        //    CurrencyWallet newData = data as CurrencyWallet;
        //    gem = newData.amount;
        //    gem_lb.text = GameManager.instance.DevideCurrency(gem);
        //});
        //DataTrigger.RegisterValueChange(DataPath.MAGNET, (data) =>
        //{
        //    if (data == null) return;
        //    ItemData newData = data as ItemData;
        //    if (newData.total > 0) magnet_lb.text = $"{newData.total}";
        //    else magnet_lb.text = "0";
        //});
        //DataTrigger.RegisterValueChange(DataPath.BOMB, (data) =>
        // {
        //     if (data == null) return;
        //     ItemData newData = data as ItemData;
        //     if (newData.total > 0) bomb_lb.text = $"{newData.total}";
        //     else bomb_lb.text = "0";

        // });
        //DataTrigger.RegisterValueChange(DataPath.LASTSAVETIME, (data) =>
        //{
        //    if (data == null) return;
        //    string newData = data as string;

        //});
        //DataTrigger.RegisterValueChange(DataPath.MAXCARDPOOL, (data) =>
        //{
        //    if (data == null) return;
        //    int newData = (int)data;

        //});

        //addHoldBtn.onClick.AddListener(AddHoldItemClick);
        //addBoxBtn.onClick.AddListener(AddBoxItemClick);
        //settingBtn.onClick.AddListener(SettingButton);
        //onNewPlayer.AddListener(OnNewPlayer);
    }
    private void OnDisable()
    {
        //addHoldBtn.onClick.RemoveListener(AddHoldItemClick);
        //addBoxBtn.onClick.RemoveListener(AddBoxItemClick);
        //settingBtn.onClick.RemoveListener(SettingButton);
        //onNewPlayer.RemoveListener(OnNewPlayer);
    }
    public override void OnStartShowView()
    {
        base.OnStartShowView();
        //expBar = GetComponentInChildren<ExperienceBar>();
        //expBar.Init();
        //StartCoroutine(GetItemFormData());
        //StartCoroutine(BreakCouroutine());
    }
    public string CheckTotalItem(int total)
    {
        if (total > 0) return total.ToString();
        else return "0";
    }
    public override void OnStartHideView()
    {
        base.OnStartHideView();
        //IngameController.instance.SaveCardListToSLots();
    }
    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);
        //GamePlayViewParam param = viewParam as GamePlayViewParam;
        //isNewPlayer = param.isNewPlayer;
        //int gold = DataAPIController.instance.GetGold();
        //int gem = DataAPIController.instance.GetGem();
        //this.gold = gold;
        //this.gem = gem;

        //gold_lb.text = GameManager.instance.DevideCurrency(gold);
        //gem_lb.text = GameManager.instance.DevideCurrency(gem);
        //if (isNewPlayer) onNewPlayer?.Invoke(isNewPlayer);

        //addHoldBtn.interactable = true;
        //addBoxBtn.interactable = true;
    }
    public void SetTimeCounter(DateTime time)
    {
        int minute = time.Minute;
        int second = time.Second;
        timeCouter.text = $" 500 in {minute}:{second}";
    }
    IEnumerator BreakCouroutine()
    {
        
        while (true)
        {
            yield return new WaitForSeconds(breakCounter);
            if (!isShowingBreak)
            {
                ShowBreak();
            }
        }
    }
    public void OnNewPlayer(bool newPlayer)
    {
    }
   
    IEnumerator GetItemFormData()
    {
        yield return new WaitUntil(() => DataAPIController.instance.GetItemData(ItemType.AddHold) is not null);
        ItemData bombTotal = DataAPIController.instance.GetItemData(ItemType.AddHold);
        ItemData magnetTotal = DataAPIController.instance.GetItemData(ItemType.ClearOneScrew);
        bomb_lb.text = $"{bombTotal.total}";
        magnet_lb.text = $"{magnetTotal.total}";
        magnet_lb.text = $"{magnetTotal.total}";
    }
    public void ShowGoldAnim(int gold)
    {
        _changeGold = gold;
        int calGold = _changeGold - this.gold;
        if (calGold == 0) return;

        this.gold = _changeGold;
        //Debug.LogWarning("GOLD SHOW ANIM");
        gold_lb.text = gold.ToString();

    }

    public IEnumerator ButtonCouroutine(Button button,bool isPlaying)
    {
        yield return new WaitUntil(() => isPlaying == false);
        Debug.LogWarning("Button Couroutine invoke");
        button.interactable = true;
    }
 
    public void AddHoldItemClick()
    {
        // Disable the button to avoid multiple clicks
        addHoldBtn.interactable = false;
        var itemType = ItemType.AddHold;
        // Get the item data for the 'AddHold' item type
        var itemData = DataAPIController.instance.GetItemData(itemType);

        // If the total amount of the item is 0 or less, show confirmation dialog
        if (itemData.total <= 0)
        {
            ItemConfirmParam param = new ItemConfirmParam
            {
                type =itemType
            };

            // Show the item confirmation dialog
            DialogManager.Instance.ShowDialog(DialogIndex.ItemConfirmDialog, param);

            // Invoke bombItemEvent with false indicating no item available
            bombItemEvent?.Invoke(false);
        }
        else
        {
            // If there are available items, invoke the item event
            IngameController.Instance.onItemInvoke?.Invoke(itemType);

            // Re-enable the button so it's clickable again
            addHoldBtn.interactable = true;
        }
    }

    public void AddBoxItemClick()
    {
        // Disable the button to avoid multiple clicks
        addBoxBtn.interactable = false;
        var itemType = ItemType.AddBox;
        // Get the item data for the 'AddHold' item type
        var itemData = DataAPIController.instance.GetItemData(itemType);

        // If the total amount of the item is 0 or less, show confirmation dialog
        if (itemData.total <= 0)
        {
            ItemConfirmParam param = new ItemConfirmParam
            {
                type = itemType
            };

            // Show the item confirmation dialog
            DialogManager.Instance.ShowDialog(DialogIndex.ItemConfirmDialog, param);

            // Invoke bombItemEvent with false indicating no item available
            bombItemEvent?.Invoke(false);
        }
        else
        {
            // If there are available items, invoke the item event
            IngameController.Instance.onItemInvoke?.Invoke(itemType);

            // Re-enable the button so it's clickable again
            addHoldBtn.interactable = true;
        }
    }
    public void OneScrewClearClick()
    {
        // Disable the button to avoid multiple clicks
        addBoxBtn.interactable = false;
        var itemType = ItemType.ClearOneScrew;
        // Get the item data for the 'AddHold' item type
        var itemData = DataAPIController.instance.GetItemData(itemType);

        // If the total amount of the item is 0 or less, show confirmation dialog
        if (itemData.total <= 0)
        {
            ItemConfirmParam param = new ItemConfirmParam
            {
                type = itemType
            };

            // Show the item confirmation dialog
            DialogManager.Instance.ShowDialog(DialogIndex.ItemConfirmDialog, param);

            // Invoke bombItemEvent with false indicating no item available
            bombItemEvent?.Invoke(false);
        }
        else
        {
            // If there are available items, invoke the item event
            IngameController.Instance.onItemInvoke?.Invoke(itemType);

            // Re-enable the button so it's clickable again
            addHoldBtn.interactable = true;
        }
    }

    public void PauseButton()
    {
        //SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX_3);
    }

    public void ShowBreak()
    {
        DialogManager.Instance.ShowDialog(DialogIndex.BreakDialog, null, () =>
         {
             isShowingBreak = false;
         });
    }
    public void SettingButton()
    {
        PauseButton();
        //SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX_3);
        SettingParam param = new();
        param.isMainScreen = false;
        DialogManager.Instance.ShowDialog(DialogIndex.SettingDialog, param, null);
    }
    public void RateButton()
    {
        PauseButton();
        ZenSDK.instance.Rate();
    }
}
