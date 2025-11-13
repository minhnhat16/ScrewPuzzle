using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using UnityEngine.Events;
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

    [SerializeField] private StarBottleFill starBottle;
    [SerializeField] private Text gold_lb;
    [SerializeField] private Text timeCouter;
    [SerializeField] private List<RectTransform> anchorTutorials;

    [SerializeField] private Button settingBtn;
    [SerializeField] private Button addBoxBtn;
    [SerializeField] private Button addHoldBtn;
    [SerializeField] private Button clearOneScrewBtn;

    [SerializeField] private GoldDisplay goldDisplay;
    [SerializeField] bool isNewPlayer;

    [HideInInspector]
    public UnityEvent<bool> magnetItemEvent = new();
    [HideInInspector]
    public UnityEvent<bool> bombItemEvent = new();

    public Text GoldLb { get { return gold_lb; } }

    public RectTransform Anchor { get => anchor; set => anchor = value; }
    public List<RectTransform> AnchorTutorials { get => anchorTutorials; set => anchorTutorials = value; }

    public UnityEvent<bool> onNewPlayer = new();

    private void OnEnable()
    {
        addHoldBtn.onClick.AddListener(AddHoldItemClick);
        addBoxBtn.onClick.AddListener(AddBoxItemClick);
        clearOneScrewBtn.onClick.AddListener(OneScrewClearClick);
        settingBtn.onClick.AddListener(SettingButton);
        //onNewPlayer.AddListener(OnNewPlayer);
    }
    private void OnDisable()
    {
        addHoldBtn.onClick.RemoveListener(AddHoldItemClick);
        addBoxBtn.onClick.RemoveListener(AddBoxItemClick);
        clearOneScrewBtn.onClick.RemoveListener(OneScrewClearClick);

        //settingBtn.onClick.RemoveListener(SettingButton);
        //onNewPlayer.RemoveListener(OnNewPlayer);
    }
    public override void OnStartShowView()
    {
        base.OnStartShowView();
        IngameController.Instance.onStarChange = starBottle.fillChange ;

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
        starBottle.Reset();
        //IngameController.instance.SaveCardListToSLots();
    }
    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);
        GamePlayViewParam param = viewParam as GamePlayViewParam;

        int userGold= gold = param.totalGold;
        goldDisplay.SetGoldToLable(userGold);
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
        yield return new WaitUntil(() => DataAPIController.instance.GetItemData(ItemType.Magnet) is not null);
        ItemData bombTotal = DataAPIController.instance.GetItemData(ItemType.Magnet);
        ItemData magnetTotal = DataAPIController.instance.GetItemData(ItemType.Drill);
        /*bomb_lb.text = $"{bombTotal.total}";
        magnet_lb.text = $"{magnetTotal.total}";
        magnet_lb.text = $"{magnetTotal.total}";*/
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

    public IEnumerator ButtonCouroutine(Button button, bool isPlaying)
    {
        yield return new WaitUntil(() => isPlaying == false);
        Debug.LogWarning("Button Couroutine invoke");
        button.interactable = true;
    }


    public void AddHoldItemClick()
    {
        // Disable the button to avoid multiple clicks
        addHoldBtn.interactable = false;
        var itemType = ItemType.Magnet;
        // Get the item data for the 'AddHold' item type
        var itemData = DataAPIController.instance.GetItemData(itemType);

        // If the total amount of the item is 0 or less, show confirmation dialog
        if (itemData.total <= 0)
        {
            AddItemDialogParam param = new AddItemDialogParam
            {
                ItemType = itemType
            };

            // Show the item confirmation dialog
            DialogManager.Instance.ShowDialog(DialogIndex.AddItemDialog, param);

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
        var itemType = ItemType.Breaker;
        // Get the item data for the 'AddHold' item type
        var itemData = DataAPIController.instance.GetItemData(itemType);

        // If the total amount of the item is 0 or less, show confirmation dialog
        if (itemData.total <= 0)
        {
            Debug.Log("Item total < 0");
            AddItemDialogParam param = new AddItemDialogParam()
            {
                ItemType = itemType
            };

            // Show the item confirmation dialog
            DialogManager.Instance.ShowDialog(DialogIndex.AddItemDialog, param);

            // Invoke bombItemEvent with false indicating no item available
            bombItemEvent?.Invoke(false);
        }
        else
        {
            Debug.Log("Item total > 0");
            addBoxBtn.interactable = true;
            // If there are available items, invoke the item event
            IngameController.Instance.onItemInvoke?.Invoke(itemType);
            // Re-enable the button so it's clickable again
        }
    }
    public void OneScrewClearClick()
    {
        // Disable the button to avoid multiple clicks
        addBoxBtn.interactable = false;
        var itemType = ItemType.Drill;
        // Get the item data for the 'AddHold' item type
        var itemData = DataAPIController.instance.GetItemData(itemType);

        // If the total amount of the item is 0 or less, show confirmation dialog
        if (itemData.total <= 0)
        {
            AddItemDialogParam param = new AddItemDialogParam
            {
                ItemType = itemType
            };

            // Show the item confirmation dialog
            DialogManager.Instance.ShowDialog(DialogIndex.AddItemDialog, param);

            // Invoke bombItemEvent with false indicating no item available
            bombItemEvent?.Invoke(false);
        }
        else
        {
            // If there are available items, invoke the item event
            IngameController.Instance.onItemInvoke?.Invoke(itemType);

            // Re-enable the button so it's clickable again
            clearOneScrewBtn.interactable = true;
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
        param.totalGold = gold;
        param.title = "PAUSE";
        DialogManager.Instance.ShowDialog(DialogIndex.SettingDialog, param, null);
    }
    public void RateButton()
    {
        PauseButton();
        ZenSDK.instance.Rate();
    }
}
