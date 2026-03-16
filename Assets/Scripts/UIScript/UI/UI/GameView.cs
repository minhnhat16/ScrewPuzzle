using Enums;
using Ingame;
using Managers;
using System;
using System.Collections.Generic;
using System.DataBase;
using UIScript;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameView : BaseView
{
    [SerializeField] private RectTransform anchor;
    [SerializeField] private RectTransform goldParent;
    [SerializeField] private RectTransform gemParent;
    [SerializeField] private StarBottleFill starBottle;
    [SerializeField] private Text gold_lb;
    [SerializeField] private Text timeCouter;
    [SerializeField] private Text txt_specialScrew;
    [SerializeField] private Text txt_description;
    [SerializeField] private List<RectTransform> anchorTutorials;
    [SerializeField] private Button settingBtn;
    [SerializeField] private Button reloadButton;
    [SerializeField] private ItemButtonIngame btn_hammer;
    [SerializeField] private ItemButtonIngame btn_drill;
    [SerializeField] private ItemButtonIngame btn_magnet;
    [SerializeField] private GoldDisplay goldDisplay;

    // ── SerializeField thay vì FindAnyObjectByType ──────────────
    [SerializeField] private ItemController itemController;

    public UnityEvent<bool> itemPerformed = new();
    public Text GoldLb => gold_lb;
    public RectTransform Anchor => anchor;
    public RectTransform StarAnchor => starBottle.transform as RectTransform;
    private ItemType? currentDescriptionItem;

    private void Awake()
    {
        // Fallback nếu chưa assign trong Inspector
        if (itemController == null)
            itemController = FindAnyObjectByType<ItemController>();
    }

    private void OnEnable()
    {
        btn_drill.AddListener(DrillClicked);
        btn_hammer.AddListener(HammerClicked);
        btn_magnet.AddListener(MagnetClicked);
        settingBtn.onClick.AddListener(SettingButton);
        reloadButton.onClick.AddListener(ReloadClicked);

        // ── FIX 1: Subscribe vào event của itemController, không gán đè reference ──
        if (itemController != null)
            itemController.itemPerformed.AddListener(ItemPerformededHandler);

        DataTrigger.RegisterValueChange(DataPath.ITEMDICT, OnItemDictChanged);
        RefreshAllItemDisplays();
    }

    private void OnDisable()
    {
        btn_drill.RemoveListener(DrillClicked);
        btn_hammer.RemoveListener(HammerClicked);
        btn_magnet.RemoveListener(MagnetClicked);
        settingBtn.onClick.RemoveListener(SettingButton);
        reloadButton.onClick.RemoveListener(ReloadClicked);

        // ── FIX 2: Unsubscribe đúng cách ────────────────────────
        if (itemController != null)
            itemController.itemPerformed.RemoveListener(ItemPerformededHandler);

        DataTrigger.UnRegisterValueChange(DataPath.ITEMDICT, OnItemDictChanged);
    }

    // ── Tách lambda thành method để có thể RemoveListener đúng ──
    private void ReloadClicked() => LevelManager.ins.ReLoadLevel();

    public override void OnInit(Action callback = null)
    {
        base.OnInit(callback);
        var anchor = SpecialBoxManager.ins.SpecialBoxAnchor;
        anchor.position = ViewManager.Instance.UIToWorld(
            txt_specialScrew.rectTransform, CameraMain.instance.main) + Vector3.up * 0.5f;
    }

    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);
        starBottle.OnReset();
    }

    public override void OnStartShowView()
    {
        base.OnStartShowView();
        starBottle.OnReset();
        txt_specialScrew.text = "0";
        IngameController.ins.OnStarChanged = starBottle.fillChange;
        HideDescription();
    }

    public override void OnEndHideView()
    {
        base.OnEndHideView();
        starBottle.OnReset();
    }

    // ============================================================
    // ITEM BUTTON HANDLERS
    // ============================================================

    private void DrillClicked() => HandleGameplayItemClick(ItemType.Drill, btn_drill.Button);
    private void HammerClicked() => HandleGameplayItemClick(ItemType.Breaker, btn_hammer.Button);
    private void MagnetClicked() => HandleGameplayItemClick(ItemType.Magnet, btn_magnet.Button);

    private void HandleGameplayItemClick(ItemType itemType, Button button)
    {
        button.interactable = false;

        var itemData = DataAPIController.instance.GetItemData(itemType);
        var itemConfig = ConfigFileManager.Instance.GetItemConfig(itemType);
        var sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, itemType.ToString());

        if (itemData == null || itemData.total <= 0)
        {
            var param = new AddItemDialogParam
            {
                ItemType = itemType,
                detail = itemConfig?.Detail,
                ItemPrice = itemConfig?.Price ?? 0,
                IsAdsAvailable = true,
                sprite = sprite
            };

            DialogManager.ins.ShowDialog(DialogIndex.ItemDialog, param, () =>
            {
                button.interactable = true;
            });
        }
        else
        {
            DataAPIController.instance.UseItem(itemType, 1);

            ShowDescription(itemType);

            var pos = itemType == ItemType.Breaker
                ? Vector3.zero
                : ArrayScrew.ins.GetLastHoldPosition() + new Vector3(1f, -0.5f, 0f);

            IngameController.ins.OnItemInvoke?.Invoke(itemType, pos);
            button.interactable = true;
        }
    }

    public void ShowDescription(ItemType itemType)
    {
        currentDescriptionItem = itemType;

        var itemConfig = ConfigFileManager.Instance.GetItemConfig(itemType);
        if (txt_description != null)
            txt_description.text = itemConfig?.Detail ?? string.Empty;

        var anim = BaseViewAnimation as GamePlayAnim;
        anim?.ShowDescription(null);
    }

    public void HideDescription()
    {
        currentDescriptionItem = null;
        var anim = BaseViewAnimation as GamePlayAnim;
        anim?.HideDescription(null);
    }

    /// <summary>
    /// Được gọi từ itemController.itemPerformed event.
    /// isPerformed = true  → item auto (Magnet/Drill/AddBox) đã xong → hide
    /// isPerformed = false → item manual (Breaker) đã perform xong → hide
    /// Cả hai đều hide — phân biệt ở chỗ AI GỌI invoke, không phải ở đây.
    /// </summary>
    private void ItemPerformededHandler(bool isPerformed)
    {
        HideDescription();
    }

    public void SettingButton()
    {
        DialogManager.ins.ShowDialog(DialogIndex.SettingDialog, new SettingParam
        {
            isMainScreen = false,
            totalGold = WalletManager.ins.Get(Currency.Gold),
            title = "PAUSE",
            music_enable = SoundHelper.IsMusicEnabled(),
            sfx_enable = SoundHelper.IsSFXEnabled(),
        });
    }

    public void ShowBreak()
    {
        DialogManager.ins.ShowDialog(DialogIndex.BreakDialog, null, null);
    }

    internal void UpdateSpecialBoxCount(ColorEnum color, int v)
    {
        txt_specialScrew.text = v.ToString();
    }

    // ============================================================
    // DataTrigger helpers
    // ============================================================

    private void OnItemDictChanged(object arg)
    {
        RefreshAllItemDisplays();
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
        if (button?.TextLB == null) return;

        var itemData = DataAPIController.instance.GetItemData(type);
        if (itemData == null) return;

        try { button.SetItemQuantity(itemData.total); }
        catch { /* Fail silently — UI không được crash game */ }
    }

    private void RefreshDescriptionIfVisible()
    {
        if (!currentDescriptionItem.HasValue || txt_description == null) return;
        var cfg = ConfigFileManager.Instance.GetItemConfig(currentDescriptionItem.Value);
        txt_description.text = cfg?.Detail ?? string.Empty;
    }
}