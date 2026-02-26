using Enums;
using Ingame;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Xml.Schema;
using UIScript;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameView : BaseView
{
    [SerializeField] private RectTransform anchor;
    //[SerializeField] private bool isShowingBreak;

    [SerializeField] private RectTransform goldParent;
    [SerializeField] private RectTransform gemParent;

    [SerializeField] private StarBottleFill starBottle;
    [SerializeField] private Text gold_lb;
    [SerializeField] private Text timeCouter;
    [SerializeField] private Text txt_specialScrew;
    [SerializeField] private Text txt_description;
    [SerializeField] private List<RectTransform> anchorTutorials;

    [SerializeField] private Button settingBtn;
    [SerializeField] private ItemButtonIngame btn_hammer;
    [SerializeField] private ItemButtonIngame btn_drill;
    [SerializeField] private ItemButtonIngame btn_magnet;

    [SerializeField] private GoldDisplay goldDisplay;

    public UnityEvent<bool> itemPerformed = new();


    [SerializeField] private ItemController itemController;

    public Text GoldLb => gold_lb;
    public RectTransform Anchor => anchor;

    // Keep track of which item type's description is currently shown (if any)
    private ItemType? currentDescriptionItem;

    private void Awake()
    {
        itemController = FindAnyObjectByType<ItemController>();
    }
    private void OnEnable()
    {

        btn_drill.AddListener(DrillClicked);
        btn_hammer.AddListener(HammerClicked);
        btn_magnet.AddListener(MagnetClicked);
        settingBtn.onClick.AddListener(SettingButton);
        itemPerformed = itemController.itemPerformed;
        itemPerformed.AddListener(ItemPerformededHandler);

        // Register to be notified when the item dictionary or its entries change.
        DataTrigger.RegisterValueChange(DataPath.ITEMDICT, OnItemDictChanged);

        // Initialize display once on enable
        RefreshAllItemDisplays();
    }



    private void OnDisable()
    {

        btn_drill.RemoveListener(DrillClicked);
        btn_hammer.RemoveListener(HammerClicked);
        btn_magnet.RemoveListener(MagnetClicked);
        settingBtn.onClick.RemoveListener(SettingButton);

        DataTrigger.UnRegisterValueChange(DataPath.ITEMDICT, OnItemDictChanged);
    }
    public override void OnInit(Action callback = null)
    {
        base.OnInit(callback);
        var anchor = SpecialBoxManager.ins.SpecialBoxAnchor;
        anchor.position = ViewManager.Instance.UIToWorld(txt_specialScrew.rectTransform, CameraMain.instance.main) + Vector3.up * 0.5f;
    }
    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);

        long gold = WalletManager.ins.Get(Currency.Gold);

        starBottle.OnReset();

    }

    public override void OnStartShowView()
    {
        base.OnStartShowView();
        starBottle.OnReset();
        txt_specialScrew.text = "0";
        IngameController.ins.OnStarChanged = starBottle.fillChange;
        starBottle.OnReset();

    }
    public override void OnEndHideView()
    {
        base.OnEndHideView();
        starBottle.OnReset();
    }

    //private void OnCurrencyUpdated(Currency type, long value)
    //{
    //    if (type == Currency.Gold && GoldLb)
    //    {
    //        gold_lb.text = value.ToString();
    //    }
    //}

    // ============================================================
    // ITEM BUTTON HANDLERS — NEW LOGIC
    // ============================================================

    private void DrillClicked()
    {
        HandleGameplayItemClick(ItemType.Drill, btn_drill.Button);
    }

    private void HammerClicked()
    {
        HandleGameplayItemClick(ItemType.Breaker, btn_hammer.Button);

    }

    private void MagnetClicked()
    {
        HandleGameplayItemClick(ItemType.Magnet, btn_magnet.Button);

    }
    public void ShowDescription(ItemType itemType)
    {
        // Set current selection so description can be refreshed when item totals change
        currentDescriptionItem = itemType;

        var itemConfig = ConfigFileManager.Instance.GetItemConfig(itemType);
        var sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, itemType.ToString());
        string detail = itemConfig?.Detail ?? string.Empty;

        if (txt_description != null)
            txt_description.text = detail;

        var anim = this.BaseViewAnimation as GamePlayAnim;
        anim.ShowDescription(null);
    }
    /// <summary>
    /// Xử lý click item trong gameplay
    /// </summary>
    private void HandleGameplayItemClick(ItemType itemType, Button button, UnityEvent<bool> itemEvent = null)
    {
        button.interactable = false;

        var itemData = DataAPIController.instance.GetItemData(itemType);
        var itemConfig = ConfigFileManager.Instance.GetItemConfig(itemType);
        var sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, itemType.ToString());

        if (itemData.total <= 0)   // <= 0 → mở ConfirmDialog
        {
            AddItemDialogParam param = new AddItemDialogParam
            {
                ItemType = itemType,
                detail = itemConfig.Detail,
                ItemPrice = itemConfig.Price,
                IsAdsAvailable = true,
                sprite = sprite
            };

            DialogManager.ins.ShowDialog(DialogIndex.ItemDialog, param, () =>
            {
                button.interactable = true;
            });
            itemEvent?.Invoke(false);
        }
        else
        {
            // đủ item → sử dụng item
            ShowDescription(itemType);
            var pos = itemType == ItemType.Breaker ? Vector3.zero : ArrayScrew.ins.GetLastHoldPosition() + new Vector3(1, -0.5f);
            IngameController.ins.OnItemInvoke?.Invoke(itemType, pos);
            button.interactable = true;
        }
    }


    private void ItemPerformededHandler(bool isPerformed)
    {
        if (isPerformed)
        {
            GamePlayAnim anim = this.BaseViewAnimation as GamePlayAnim;
            anim.HideDescription(null);
        }
    }
    public void SettingButton()
    {
        SettingParam param = new()
        {
            isMainScreen = false,
            totalGold = WalletManager.ins.Get(Currency.Gold),
            title = "PAUSE",
            music_enable = SoundHelper.IsMusicEnabled(),
            sfx_enable = SoundHelper.IsSFXEnabled(),
        };

        DialogManager.ins.ShowDialog(DialogIndex.SettingDialog, param);
    }

    public void ShowBreak()
    {
        DialogManager.ins.ShowDialog(DialogIndex.BreakDialog, null, () =>
        {
        });
    }

    internal void UpdateSpecialBoxCount(ColorEnum color, int v)
    {
        txt_specialScrew.text = v.ToString();
    }

    // ============================================================
    // DataTrigger handler / helpers
    // ============================================================
    // Called when DataPath.ITEMDICT or its children are updated via DataModel triggers.
    private void OnItemDictChanged(object arg)
    {
        RefreshAllItemDisplays();

        // If a description is currently shown for an item, refresh its text (useful
        // if description depends on item config or quantity)
        RefreshDescriptionIfVisible();
    }

    private void RefreshAllItemDisplays()
    {
        UpdateItemDisplay(ItemType.Magnet, btn_magnet);
        UpdateItemDisplay(ItemType.Breaker, btn_hammer);
        UpdateItemDisplay(ItemType.Drill, btn_drill);
    }

    private void UpdateItemDisplay(ItemType type, ItemButtonIngame button)
    {
        if (button == null) return;

        var itemData = DataAPIController.instance.GetItemData(type);
        if (itemData == null) return;

        try
        {
            if (button.TextLB != null)
            {
                // use ItemButtonIngame helper to update UI and interactivity
                button.SetItemQuantity(itemData.total);
            }

        }
        catch (Exception)
        {
            // Fail silently — UI update shouldn't crash game
        }
    }

    private void RefreshDescriptionIfVisible()
    {
        if (!currentDescriptionItem.HasValue || txt_description == null)
            return;

        var cfg = ConfigFileManager.Instance.GetItemConfig(currentDescriptionItem.Value);
        txt_description.text = cfg?.Detail ?? string.Empty;
    }
}