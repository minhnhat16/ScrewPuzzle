using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using System;


public class ZenSDKV1 : MonoBehaviour, ZenSDK.IZenSDK
{

	public void Init()
	{
		//Application.targetFrameRate = 60;

		//init leaderboard;
#if UNITY_ANDROID

#elif UNITY_IOS

#else

#endif

		Debug.Log("ZenSDKV1:Init");

	}

	//tracking
	public void OnGameStart()
	{
	}
	public void OnGameOver(string overValue)
	{
	}
	public void OnGameResume()
	{
	}
	public void OnGamePause()
	{

	}
	public void TrackLevelStart(int level)
	{
		//Debug.Log($"ZenSDK1:TRACKLEVELSTART{level}");
	}
	public void TrackLevelFailed(int level)
	{
	}
    public void TrackLevelCompleted(int level,int scores)
    {

    }
    public int GetConfigInt(String name,int defaultValue)
	{
		return defaultValue;
	}
	public string GetConfigString(String name, string defaultValue)
    {
		return defaultValue;
    }
	public string SpecialTileTutorial(String name)
	{
		return "";
	}

	//leaderboard
	public void ReportScore(string leaderboardID, long score)
	{

	}

	public void ShowLeaderboard()
	{

	}

	//ads
	public void ShowFullScreen()
	{
		Debug.Log("ShowFullScreen");
	}
	public void ShowBanner(bool visible)
	{

	}
	public void ShowVideoReward(Action<bool> callback)
	{


	}
	public bool IsVideoRewardReady()
	{
		return false;
	}

	public void ShowAppOpen(Action<bool> callback)
	{
		//Debug.Log($"SHOW APP OPEN {callback}");
		callback?.Invoke(true);
	}

	//rate
	public void Rate()
	{
#if UNITY_ANDROID
				Application.OpenURL("market://details?id=com.geda.candymatch3mania");
#elif UNITY_IPHONE
				Application.OpenURL("itms-apps://itunes.apple.com/app/id1438933213");
#endif

	}

	//share
	public void Share()
	{

	}

	public void Like()
	{


	}
}
