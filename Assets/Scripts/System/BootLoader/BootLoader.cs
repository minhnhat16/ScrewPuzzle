using Managers;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class BootLoader : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private UIRootControlScale uiRootControl;

    private void Awake()
    {
        ScreenSetup();
    }
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        // ----- REGISTER BOOT TASKS -----
        TaskManager.ins.AddDataInitTask(Task_LoadRemoteAsset);
        TaskManager.ins.AddDataInitTask(Task_InitConfig);
        TaskManager.ins.AddDataInitTask(Task_InitData);
        TaskManager.ins.AddTask(Task_InitMission);
        TaskManager.ins.AddTask(Task_SetupUI);
        TaskManager.ins.AddTask(Task_InitSound);

        StartCoroutine(TaskManager.ins.RunDataInitTask(() =>
          {
              Debug.Log("BootLoader: Load Scene after initdata  Done");

              bool isNew = DataAPIController.instance.IsNewPlayer();
              string sceneName = isNew ? "InGame" : "Buffer";
              LoadSceneManager.ins.LoadSceneByName(sceneName, () =>
              {

                  Debug.Log("BootLoader: Load Scene Done");
                  float progressTime = TaskManager.ins.TotalProgress;
                  LoadSceneManager.ins.TimeWait = progressTime;
                  ViewManager.Instance.SwitchViewForNewPlayer(isNew);
              });
          }));

    }


    private IEnumerator Task_InitSound()
    {
        bool done = false;

        bool musicData = DataAPIController.instance.GetMusicSetting();
        bool sfxData = DataAPIController.instance.GetSoundSetting();
        SoundHelper.SetEnabled(musicData, sfxData);
        SoundManager.instance.Init(() => done = true);

        SoundHelper.PlayMusic(SoundManager.Music.MainScreenMusic);
        yield return new WaitUntil(() => done);
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

        Screen.orientation = ScreenOrientation.Portrait;

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

        try
        {
            ConfigFileManager.Instance.Init(() =>
            {
                done = true;
            });

        }
        catch (Exception e)
        {
            Debug.LogError("[BOOT] InitConfig FAILED\n" + e);
            done = true; // tránh treo boot
        }
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


        Debug.Log("Task load remote asset");
        yield return ResourceManager.ins.Init(
            new List<string>
            {
                "level",
            "UI"
            },
            () => done = true
        );

        while (!done)
        {
            yield return null;

        }
        SpriteLibControl.Instance.LoadAllPartSprites(true);
        Shader.WarmupAllShaders();

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

