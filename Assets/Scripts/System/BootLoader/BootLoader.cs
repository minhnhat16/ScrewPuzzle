using System;
using System.Collections;
using UnityEngine;
public class BootLoader : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private UIRootControlScale uiRootControl;
    IEnumerator Start()
    {
        DontDestroyOnLoad(this.gameObject);
        yield return new WaitForSeconds(1f);
        
        InitConfig(() =>
        {
            InitDataDone(() =>
            {

            });
        });
       
        yield return new WaitUntil(()=> ConfigFileManager.Instance.isDone);
        StartCoroutine(SetUpUI(() =>
        {
            SetupAfterInitConfig();

            gameManager.SetUpIngame();
            gameManager = GetComponentInChildren<GameManager>();
            gameManager.TrackLevelStart = 0;
            ZenSDK.instance.TrackLevelStart(gameManager.TrackLevelStart);
        }));
    }
    private void InitDataDone(Action callback)
    {
        DataAPIController.instance.InitData(() =>
        {
            callback?.Invoke();
        });
    }
    IEnumerator SetUpUI(Action callback)
    {
        uiRoot.SetActive(true);
        yield return new WaitForSeconds(1f);
        LoadSceneManager.instance.LoadSceneByName("Buffer", () =>
        {
            MainScreenViewParam param = new MainScreenViewParam();
            param.totalGold = 0;
            //Debug.Log("LoadSenceCallback");
            ViewManager.Instance.SwitchView(ViewIndex.MainScreenView, param, () =>
            {
                DayTimeController.instance.CheckNewDay();
                ZenSDK.instance.ShowAppOpen((isDone) =>
                {
                    DialogManager.Instance.ShowDialog(DialogIndex.LableChooseDialog);
                    SoundManager.instance.PlayMusic(SoundManager.Music.GamplayMusic);
                    //Debug.LogWarning("SHOW APP OPEN ON END LOADING");
                    if (DayTimeController.instance.isNewDay)
                    {
                        //Debug.Log("isnewday now go to claim spin reward");
                        DataAPIController.instance.SetSpinData(false);
                    }
                    else
                    {
                        //Debug.Log("still in last day can't claim spin reward");
                        //DialogManager.Instance.ShowDialog(DialogIndex.LabelChooseDialog);
                    }
                });
            });
           

        });
        yield return null;
        callback?.Invoke();
    }
    private void SetupAfterInitConfig()
    {
        MainScreenViewParam param = new();
        param.totalGold = DataAPIController.instance.GetGold();

    }
    private void InitConfig(Action callback)
    {
        ConfigFileManager.Instance.Init(() =>
        {
            callback?.Invoke();
        }); ;
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Time.timeScale = 0;
        }
        else
            Time.timeScale = 1;
    }
    private void OnApplicationFocus(bool focus)
    {
        Time.timeScale = 1;
    }
}

