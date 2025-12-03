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

    [SerializeField] private Text titleLB;
    [SerializeField] RectTransform below;

    private void OnEnable()
    {

        homeButton.onClick.AddListener(HomeButton);

        tg_music.m_Toggle.onValueChanged.AddListener(SettingMusic);
        tg_soundSfx.m_Toggle.onValueChanged.AddListener(SettingSFX);
    }
    private void OnDisable()
    {
        homeButton.onClick.RemoveAllListeners();
        tg_music.m_Toggle.onValueChanged.RemoveListener(SettingMusic);
        tg_soundSfx.m_Toggle.onValueChanged.RemoveListener(SettingSFX);
    }
    private void SettingSFX(bool value)
    {
        SoundManager.instance.SettingSFXVolume(value);
    }

    private void SettingMusic(bool value)
    {
        SoundManager.instance.SettingMusicVolume(value);
    }


    public override void Setup(DialogParam dialogParam)
    {
        SettingParam param = (SettingParam)dialogParam;
        long userGold = param.totalGold;
        bool isMainScreen = param.isMainScreen;
        SetupButton(isMainScreen);
        goldDisplay.SetGoldToLable(userGold);
        SetupPauseGame(isMainScreen);
    
        //isMainScreen = param.isMainScreen;
        //        below.gameObject.SetActive(!param.isMainScreen);
    }

    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        ZenSDK.instance.ShowFullScreen();
        //tg_music.SwapSprite(true);
        //tg_soundSfx.SwapSprite(true);

        //tg_soundVib.SwapSprite(true);
    }
    public override void OnEndShowDialog()
    {
        base.OnEndShowDialog();
        tg_soundVib.m_Toggle.isOn = true;
        tg_soundSfx.m_Toggle.isOn = true;
        tg_music.m_Toggle.isOn = true;

    }
    public override void OnEndHideDialog()
    {
        base.OnEndHideDialog();
        IngameController.ins.ResumeGame();
    }
    public void PlayButton()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
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
        IngameController.ins.PauseGame();
    }
    private void OnDropdownValueChanged(int index)
    {
        // Get the selected option text
        string selectedOption = language_dr.options[index].text;

        // Display the selected option
        //Debug.Log("Selected Option: " + selectedOption + " with index " + language_dr.options[index]);
    }

}
