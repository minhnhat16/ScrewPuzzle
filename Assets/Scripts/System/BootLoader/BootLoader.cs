using Managers;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Threading.Tasks;
using UnityEngine;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private UIRootControlScale uiRootControl;

    IEnumerator Start()
    {
        DontDestroyOnLoad(this.gameObject);
        ScreenSetup();
        yield return new WaitForSeconds(0.1f);
        TaskManager.ins.AddTask(Task_LoadRemoteAsset);
        // ----- REGISTER BOOT TASKS -----
        TaskManager.ins.AddTask(Task_InitConfig);

        TaskManager.ins.AddTask(Task_InitData);
        TaskManager.ins.AddTask(Task_InitMission);
        TaskManager.ins.AddTask(Task_SetupUI);
        TaskManager.ins.AddTask(Task_FinishBoot);


        LoadSceneManager.ins.LoadSceneByName("Buffer", () =>
        {
            Debug.Log("task run done");
            float progressTime = TaskManager.ins.TotalProgress;
            LoadSceneManager.ins.TimeWait = progressTime;
            MainScreenViewParam param = new()
            {
                totalGold = DataAPIController.instance.GetGold(),
                ticket = (int)DataAPIController.instance.GetTicket(),
                level  = DataAPIController.instance.GetPlayerLevel(),
            };

            ViewManager.Instance.SwitchView(ViewIndex.MainScreenView, param, () =>
            {
                Debug.Log("task run done switch view");

                DayTimeController.instance.CheckNewDay();
            });
        });
        // ----- RUN TASKS -----
     
      
    }

    private IEnumerator Task_InitMission()
    {
        Debug.Log("[BOOT] InitMission...");
        bool done = false;
        yield return MissionManager.ins.Init(() => done = true);
        yield return new WaitUntil(() => done);
    }

    private void ScreenSetup()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;

        // Ch? cho phép xoay ngang
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
    }

    IEnumerator Task_InitConfig()
    {
        Debug.Log("[BOOT] InitConfig...");
        bool done = false;
        ConfigFileManager.Instance.Init(() => done = true);
        LevelManager.ins.Init();
        yield return new WaitUntil(() => done);
    }
    IEnumerator Task_InitData()
    {
        Debug.Log("[BOOT] InitData...");
        bool done = false;

        DataAPIController.instance.InitData(() => done = true);
        yield return new WaitUntil(() => done);
    }
    IEnumerator Task_SetupUI()
    {
        Debug.Log("[BOOT] Setup UI...");
        uiRoot.SetActive(true);
        yield return ViewManager.Instance.Init();
        Debug.Log("[BOOT] VIEW LOAD DONE ");
        yield return DialogManager.ins.Init();
        Debug.Log("[BOOT] DIALOG LOAD DONE ");
    }

    IEnumerator Task_LoadRemoteAsset()
    {
        bool done = false;

        yield return ResourceManager.ins.Init(
            new List<string>
            {
            "Image_Level",
            "Config_Level",
            "UI"
            },
            () => done = true
        );

        while (!done)
        {
            yield return null;

        }
        SpriteLibControl.Instance.LoadAllPartSprites(true);

    }


    IEnumerator Task_FinishBoot()
    {
        Debug.Log("[BOOT] Finalizing...");

        gameManager = GetComponentInChildren<GameManager>();
        gameManager.SetUpIngame();
        gameManager.TrackLevelStart = 0;

        ZenSDK.instance.TrackLevelStart(gameManager.TrackLevelStart);

        yield return null;
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

