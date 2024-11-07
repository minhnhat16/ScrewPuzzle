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


    [SerializeField] private Image musicOn;
    [SerializeField] private Image musicOff;
    [SerializeField] private Image sfxOn;
    [SerializeField] private Image sfxOff;
    [SerializeField] private Image vibOn;
    [SerializeField] private Image vibOff;

    [SerializeField] private Dropdown language_dr;

    [SerializeField] private Button homeButton;

    [SerializeField] private Text goldLb;
    [SerializeField] private Text titleLB;
    [SerializeField] RectTransform below;
    [HideInInspector]
    public UnityEvent<bool> musicEvent = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<bool> sfxEvent = new UnityEvent<bool>();
    [HideInInspector]
    public UnityEvent<bool> vibEvent = new UnityEvent<bool>();
    private void OnEnable()
    {
        musicEvent.AddListener(MusicChange);
        sfxEvent.AddListener(SFXChange);
        homeButton.onClick.AddListener(HomeButton);
        //language_dr.onValueChanged.AddListener(OnDropdownValueChanged);
    }

 
    private void OnDisable()
    {
        musicEvent.RemoveListener(MusicChange);
        sfxEvent.RemoveListener(SFXChange);

    }
    public override void Setup(DialogParam dialogParam)
    {
        SettingParam param = (SettingParam)dialogParam ;
        int userGold = param.totalGold;
        bool isMainScreen = param.isMainScreen;
        SetupButton(isMainScreen);
        SetupGold(userGold);
        //isMainScreen = param.isMainScreen;
        //        below.gameObject.SetActive(!param.isMainScreen);
    }
    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        ZenSDK.instance.ShowFullScreen();

    }
    public override void OnEndHideDialog()
    {
        base.OnEndHideDialog();

    }
    public void PlayButton()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        DialogManager.Instance.HideDialog(dialogIndex, () =>
        {
        });

    }

    public void SetupGold(int userGold)
    {
        goldLb.text = userGold.ToString();
    }
    public void HomeButton()
    {
       // SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX_2);
        DialogManager.Instance.HideDialog(dialogIndex, () =>
        {
            LoadSceneManager.instance.LoadSceneByName("Buffer", () =>
            {
                MainScreenViewParam param = new();
                param.totalGold = DataAPIController.instance.GetGold();
                ViewManager.Instance.SwitchView(ViewIndex.MainScreenView, param);
                /*  
                DialogManager.Instance.ShowDialog(DialogIndex.LableChooseDialog, null, () =>
                {
                });*/
            });
        });
    }
    public void MusicChange(bool isOn)
    {
        //SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        //Debug.Log("MUSIC CHANGED" + isOn);
        SoundManager.instance.musicSetting = isOn;
        SoundManager.instance.SettingMusicVolume(isOn);
        if (isOn)
        {

            musicOn.gameObject.SetActive(true);
            musicOff.gameObject.SetActive(false);
        }
        else
        {
            musicOn.gameObject.SetActive(false);
            musicOff.gameObject.SetActive(true);
        }
    }
    public void SFXChange(bool isOn)
    {
        //SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        //Debug.Log("SFX CHANGED" + isOn);
        SoundManager.instance.sfxSetting = isOn;
        //SoundManager.instance.SettingSFXVolume(isOn);
        if (isOn)
        {   
            sfxOn.gameObject.SetActive(true);
            sfxOff.gameObject.SetActive(false);
        }
        else
        {
            sfxOn.gameObject.SetActive(false);
            sfxOff.gameObject.SetActive(true);
        }
    }
    public void OnMusicChanged()
    {
        isMusicOn = !isMusicOn;
        //Debug.Log("OnMusicChanged" + isMusicOn);
        musicEvent?.Invoke(isMusicOn);
    }
    public void OnSFXChanged()
    {
        isSFXOn = !isSFXOn;
        sfxEvent?.Invoke(isSFXOn);
    }
    public void CloseBtn()
    {
       // SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        //Debug.Log("Close button on " + this.dialogIndex);
        DialogManager.Instance.HideDialog(dialogIndex, () =>
        {
            
        });
    }

    private void SetupButton(bool isMainScreen)
    {
        language_dr.gameObject.SetActive(isMainScreen);
        homeButton.gameObject.SetActive(!isMainScreen);
    }
    private void OnDropdownValueChanged(int index)
    {
        // Get the selected option text
        string selectedOption = language_dr.options[index].text;

        // Display the selected option
        //Debug.Log("Selected Option: " + selectedOption + " with index " + language_dr.options[index]);
    }

}
