using Managers;
using System;
using System.DataBase;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingDialog : BaseDialog
{
    [SerializeField] private bool isMusicOn;
    [SerializeField] private bool isSFXOn;
    [SerializeField] private bool isVibOn;
    [SerializeField] private bool isMainScreen;
    [SerializeField] private CustomToggle tg_music;
    [SerializeField] private CustomToggle tg_soundSfx;
    [SerializeField] private CustomToggle tg_soundVib;

    [SerializeField] private Dropdown language_dr;

    [SerializeField] private Button homeButton;

    [SerializeField] private GoldDisplay goldDisplay;
    [SerializeField] private GoldDisplay ticketDisplay;

    [SerializeField] private Text titleLB;
    [SerializeField] RectTransform below;
    private SettingParam param;

    private void OnEnable()
    {

        homeButton.onClick.AddListener(HomeButton);

        tg_music.m_Toggle.onValueChanged.AddListener(SettingMusic);
        tg_soundSfx.m_Toggle.onValueChanged.AddListener(SettingSFX);
        DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
        DataTrigger.RegisterValueChange(DataPath.TICKET, OnTicketChanged);
        RefreshCurrencyUI();
    }
    private void OnDisable()
    {
        homeButton.onClick.RemoveAllListeners();
        tg_music.m_Toggle.onValueChanged.RemoveListener(SettingMusic);
        tg_soundSfx.m_Toggle.onValueChanged.RemoveListener(SettingSFX);
        DataTrigger.UnRegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
        DataTrigger.UnRegisterValueChange(DataPath.TICKET, OnTicketChanged);
    }
    private void SettingSFX(bool value)
    {
        SoundHelper.SettingSFXVolume(value);
        SoundHelper.PlaySFX(SoundManager.SFX.UI_Toggle);
    }

    private void SettingMusic(bool value)
    {
        SoundHelper.SetMusic(value);
        SoundHelper.PlaySFX(SoundManager.SFX.UI_Toggle);

    }


    public override void Setup(DialogParam dialogParam)
    {
        param = (SettingParam)dialogParam;
        long userGold = param.totalGold;
        long userTicket = param.totalTicket;
        bool isMainScreen = param.isMainScreen;
        SetupButton(isMainScreen);


        Debug.Log("Usser gold " + userGold);
        goldDisplay.SetGoldToLable(userGold);
        ticketDisplay.SetGoldToLable(userTicket);
        SetupPauseGame(isMainScreen);


        tg_soundSfx.m_Toggle.isOn = param.sfx_enable;
        tg_music.m_Toggle.isOn = param.music_enable;


        tg_soundSfx.SwapSprite(param.sfx_enable);
        tg_music.SwapSprite(param.music_enable);
        //isMainScreen = param.isMainScreen;
        //        below.gameObject.SetActive(!param.isMainScreen);
    }

    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();


        Debug.Log($"SFX: {param.sfx_enable}, Music: {param.music_enable}");

    }
    public override void OnEndShowDialog()
    {
        base.OnEndShowDialog();
        ZenSDK.instance.ShowFullScreen(GameConstants.SETTING_KEY, DataAPIController.instance.GetPlayerLevel().ToString());

    }
    public override void OnEndHideDialog()
    {
        base.OnEndHideDialog();
        IngameController.ins.Resume();
    }
    public void PlayButton()
    {
        SoundHelper.PlaySFX(SoundManager.SFX.ButtonClick);
        DialogManager.ins.HideDialog(dialogIndex, () =>
        {

        });

    }


    public void HomeButton()
    {
        DialogManager.ins.HideDialog(this.dialogIndex, () =>
        {
            DialogManager.ins.ShowDialog(DialogIndex.QuitDialog);

        });
    }


    public void CloseBtn()
    {
        // SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        //Debug.Log("Close button on " + this.dialogIndex);
        DialogManager.ins.HideDialog(dialogIndex, () =>
        {

        });
    }

    private void SetupButton(bool isMainScreen)
    {
        //language_dr.gameObject.SetActive(isMainScreen);
        homeButton.gameObject.SetActive(!isMainScreen);
    }

    private void SetupPauseGame(bool isMainScreen)
    {
        if (isMainScreen) return;
        IngameController.ins.Pause();
    }

    private void RefreshCurrencyUI()
    {
        goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
        ticketDisplay.SetGoldToLable(DataAPIController.instance.GetTicket());
    }

    private void OnGoldChanged(object _)
    {
        goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
    }

    private void OnTicketChanged(object _)
    {
        ticketDisplay.SetGoldToLable(DataAPIController.instance.GetTicket());
    }

}
