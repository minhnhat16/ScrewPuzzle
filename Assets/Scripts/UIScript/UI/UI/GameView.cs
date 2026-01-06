using Enums;
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
    [SerializeField] private bool isShowingBreak;

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

    public Text GoldLb => gold_lb;
    public RectTransform Anchor => anchor;

    private void OnEnable()
    {

        btn_drill.AddListener(DrillClicked);
        btn_hammer.AddListener(HammerClicked);
        btn_magnet.AddListener(MagnetClicked);
        settingBtn.onClick.AddListener(SettingButton);
        itemPerformed = ItemController.ins.itemPerformed; 
        itemPerformed.AddListener(ItemPerformededHandler);

    }



    private void OnDisable()
    {

        btn_drill.RemoveListener(DrillClicked);
        btn_hammer.RemoveListener(HammerClicked);
        btn_magnet.RemoveListener(MagnetClicked);
        settingBtn.onClick.RemoveListener(SettingButton);
    }
    public override void OnInit(Action callback = null)
    {
        base.OnInit(callback);
        var anchor = SpecialBoxManager.ins.SpecialBoxAnchor;
        anchor.position = ViewManager.Instance.UIToWorld(txt_specialScrew.rectTransform,CameraMain.instance.main) + Vector3.up *0.5f;
    }
    public override void Setup(ViewParam viewParam)
    {
        base.Setup(viewParam);

        long gold = WalletManager.ins.Get(Currency.Gold);


    }

    public override void OnStartShowView()
    {
        base.OnStartShowView();
        starBottle.OnReset();
        txt_specialScrew.text = "0";    
        IngameController.ins.onStarChange = starBottle.fillChange;
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
        var itemConfig = ConfigFileManager.Instance.GetItemConfig(itemType);
        var sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, itemType.ToString());
        string detail = itemConfig.Detail;  


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

            IngameController.ins.onItemInvoke?.Invoke(itemType);
            button.interactable = true;
        }
    }


    private void ItemPerformededHandler(bool isPerformed)
    {
        if(isPerformed)
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

    internal void UpdateSpecialBoxCount(ColorEnum color, int v)
    {
        txt_specialScrew.text = v.ToString();
    }
}
