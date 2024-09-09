using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ZenSDK : MonoBehaviour
{
	public static ZenSDK instance;

	public GameObject zenPrefab;
	IZenSDK zenObj = null;
	void Start()
	{
		if (instance != null)
		{
			Destroy(this.gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this.gameObject);
		if (zenPrefab != null)
		{
			GameObject zengo = GameObject.Instantiate(zenPrefab);
			zenObj = zengo.GetComponent<IZenSDK>();
			DontDestroyOnLoad(zengo);
		}
		//zenObj = FindObjectOfType<ZenSDKV1> ();
		Init();
	}


	public void Init()
	{
		Debug.Log("ZenSDK: Init");

		if (zenObj != null)
			zenObj.Init();
	}


	//for leaderboard
	public void ReportScore(string leaderboardId, long score)
	{
		Debug.Log("ZenSDK: ReportScore");
		if (zenObj != null)
		{
			//String id = GamePlatform.GetLeaderboardId(leaderboardId);
			//zenObj.ReportScore(id, score);
		}
	}
	public void ShowLeaderboard()
	{
		Debug.Log("ZenSDK: ShowLeaderboard");
		if (zenObj != null)
			zenObj.ShowLeaderboard();
	}

	//for tracking
	public void OnGameStart()
	{
		Debug.Log("ZenSDK: OnGameStart");
		if (zenObj != null)
			zenObj.OnGameStart();
	}
	public void OnGameOver(string overValue)
	{
		Debug.Log("ZenSDK: OnGameOver");
		if (zenObj != null)
			zenObj.OnGameOver(overValue);
	}
	public void OnGameResume()
	{ //resume last game
		Debug.Log("ZenSDK: OnGameResume");
		if (zenObj != null)
			zenObj.OnGameResume();
	}
	public void OnGamePause()
	{ //resume last game
		Debug.Log("ZenSDK: OnGamePause");
		if (zenObj != null)
			zenObj.OnGameResume();
	}

	//for ads
	public void ShowFullScreen()
	{
		Debug.Log("ZenSDK: ShowFullScreen");
		if (zenObj != null)
			zenObj.ShowFullScreen();
	}
	public void ShowBanner(bool visible)
	{
		Debug.Log("ZenSDK: ShowBanner = " + visible);
		if (zenObj != null)
			zenObj.ShowBanner(visible);
	}
	public void ShowVideoReward(Action<bool> callback)
	{
		Debug.Log("ZenSDK: ShowVideoReward"); 

		if (zenObj != null)
			zenObj.ShowVideoReward(callback);
	}

	public bool IsVideoRewardReady()
	{
		Debug.Log("ZenSDK: IsVideoRewardReady");

		if (zenObj != null)
			return zenObj.IsVideoRewardReady();

		return false;
	}

	public void ShowAppOpen(Action<bool> callback)
	{
		Debug.Log("ZenSDK: ShowAppOpen");
		if (zenObj != null)
			zenObj.ShowAppOpen(callback);
	}

	//rate
	public void Rate()
	{
		Debug.Log("ZenSDK: Rate");
		if (zenObj != null)
			zenObj.Rate();
		else Debug.Log("ZenSDK: Rate Error");

	}
	public void TrackLevelStart(int level)
	{
		Debug.Log("ZenSDK: TrackLevelStart");
		if (zenObj != null)
			zenObj.TrackLevelStart(level);
	}
	public void TrackLevelFailed(int level)
	{
		Debug.Log("ZenSDK: TrackLevelFailed");
		if (zenObj != null)
			zenObj.TrackLevelFailed(level);
	}
	public void TrackLevelCompleted(int level,int score)
	{
		Debug.Log("ZenSDK: TrackLevelCompleted");
		if (zenObj != null)
			zenObj.TrackLevelCompleted(level, score);
	}

	public int GetConfigInt(String name, int defaultValue)
	{
		Debug.Log("ZenSDK: GetConfigInt");
			return zenObj.GetConfigInt(name, defaultValue);
	}

	public string GetConfigString(String name, string defaultValue)
	{
		Debug.Log("ZenSDK: GetConfigString");
		return zenObj.GetConfigString(name, defaultValue);
	}
	float pauseTime;
	public Boolean isResumeFromAds = false;

	public void OnApplicationPause(bool pause)
	{
		if (pause)
			pauseTime = Time.realtimeSinceStartup;
	}

	public interface IZenSDK
	{
		void Init();
		//game service
		void ReportScore(string leaderboardId, long score);
		void ShowLeaderboard();
		//for tracking
		void OnGameStart();
		void OnGameOver(string overValue);
		void OnGameResume();
		void OnGamePause();
		void TrackLevelFailed(int n);
		void TrackLevelStart(int n);
		void TrackLevelCompleted(int n,int scores);
		int GetConfigInt(string name,int defaultValue);
		string GetConfigString(string name,string defaultValue);	
		//for ads
		void ShowFullScreen();
		void ShowBanner(bool visible);
		void ShowVideoReward(Action<bool> callback);
		bool IsVideoRewardReady();
		void ShowAppOpen(Action<bool> callback);
		void Rate();
	}


}
