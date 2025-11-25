using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UIScript;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GamePlayView : BaseView
{
    [SerializeField] private RectTransform anchor;
    [SerializeField] private bool isShowingBreak;

    [SerializeField] private RectTransform goldParent;
    [SerializeField] private RectTransform gemParent;

    [SerializeField] private StarBottleFill starBottle;
    [SerializeField] private Text gold_lb;
    [SerializeField] private Text timeCouter;
    [SerializeField] private List<RectTransform> anchorTutorials;

    [SerializeField] private Button settingBtn;
    [SerializeField] private ItemButtonIngame addBoxBtn;
    [SerializeField] private ItemButtonIngame addHoldBtn;
    [SerializeField] private ItemButtonIngame clearOneScrewBtn;

    [SerializeField] private GoldDisplay goldDisplay;

    public UnityEvent<bool> magnetItemEvent = new();
    public UnityEvent<bool> bombItemEvent = new();

    public Text GoldLb => gold_lb;
    public RectTransform Anchor => anchor;

    private void OnEnable()
    {
        WalletManager.ins.OnCurrencyUpdated += OnCurrencyUpdated;

        addHoldBtn.AddListener(AddHoldItemClick);
        addBoxBtn.AddListener(AddBoxItemClick);
        clearOneScrewBtn.AddListener(OneScrewClearClick);
        settingBtn.onClick.AddListener(SettingButton);
    }

    private void OnDisable()
    {
        WalletManager.ins.OnCurrencyUpdated -= OnCurrencyUpdated;

        addHoldBtn.RemoveListener(AddHoldItemClick);
        addBoxBtn.RemoveListener(AddBoxItemClick);
        clearOneScrewBtn.RemoveListener(OneScrewClearClick);
        settingBtn.onClick.RemoveListener(SettingButton);
    }

    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);

        long gold = WalletManager.ins.Get(Currency.Gold);
        goldDisplay.SetGoldToLable(gold);
    }

    public override void OnStartShowView()
    {
        base.OnStartShowView();

        IngameController.ins.onStarChange = starBottle.fillChange;

        MissionParam param = new MissionParam
        {
            target = 10,
            current = 4
        };

        DialogManager.ins.ShowDialog(DialogIndex.MissionDialog, param);
    }

    private void OnCurrencyUpdated(Currency type, long value)
    {
        if (type == Currency.Gold && GoldLb)
        {
            gold_lb.text = value.ToString();
            goldDisplay.SetGoldToLable(value);
        }
    }

    // ============================================================
    // ITEM BUTTON HANDLERS — NEW LOGIC
    // ============================================================

    private void AddHoldItemClick()
    {
        HandleGameplayItemClick(ItemType.Magnet, addHoldBtn.Button, magnetItemEvent);
    }

    private void AddBoxItemClick()
    {
        HandleGameplayItemClick(ItemType.Breaker, addBoxBtn.Button, bombItemEvent);
    }

    private void OneScrewClearClick()
    {
        HandleGameplayItemClick(ItemType.Drill, clearOneScrewBtn.Button, bombItemEvent);
    }

    /// <summary>
    /// Xử lý click item trong gameplay
    /// </summary>
    private void HandleGameplayItemClick(ItemType itemType, Button button, UnityEvent<bool> itemEvent)
    {
        button.interactable = false;

        var itemData = DataAPIController.instance.GetItemData(itemType);
        var itemConfig = ConfigFileManager.Instance.GetItemConfig(itemType);

        if (itemData.total <= 0)   // <= 0 → mở ConfirmDialog
        {
            AddItemDialogParam param = new AddItemDialogParam
            {
                ItemType = itemType,
                detail = itemConfig.Detail,
                ItemPrice = itemConfig.Price,
                IsAdsAvailable = true
            };

            DialogManager.ins.ShowDialog(DialogIndex.AddItemDialog, param, () =>
            {
                button.interactable = true;
            });
            itemEvent?.Invoke(false);
        }
        else
        {
            // đủ item → sử dụng item
            IngameController.ins.onItemInvoke?.Invoke(itemType);
            button.interactable = true;
        }
    }

    public void SettingButton()
    {
        SettingParam param = new()
        {
            isMainScreen = false,
            totalGold = WalletManager.ins.Get(Currency.Gold),
            title = "PAUSE"
        };

        DialogManager.ins.ShowDialog(DialogIndex.SettingDialog, param);
    }

    public void ShowBreak()
    {
        DialogManager.ins.ShowDialog(DialogIndex.BreakDialog, null, () =>
        {
            isShowingBreak = false;
        });
    }
}
