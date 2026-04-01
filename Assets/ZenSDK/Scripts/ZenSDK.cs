using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.MiniJSON;

public class ZenSDK : MonoBehaviour {
	public static ZenSDK instance;

	public GameObject zenPrefab;
	IZenSDK zenObj = null;

	public InAppReviewManager inAppReviewManager;
	public InAppUpdateManager inAppUpdateManager;

	void Awake () 
	{
		if (instance != null) 
		{
			Destroy (this.gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad (this.gameObject);
		if (zenPrefab != null) 
		{
			GameObject zengo = GameObject.Instantiate (zenPrefab);
			zenObj = zengo.GetComponent<IZenSDK> ();
			DontDestroyOnLoad (zengo);
		}
		Init ();
	}


	public void Init()
	{
		Debug.Log ("ZenSDK: Init");

		if (zenObj!=null)
			zenObj.Init ();
		#if UNITY_ANDROID
			if (GetConfigInt("hasNewVersion", 0) == 1)
			{
				StartCoroutine(inAppUpdateManager.CheckForUpdate());
			}
		#endif
	}


	//for leaderboard
	public void ReportScore(string leaderboardId, long score)
	{
		Debug.Log ("ZenSDK: ReportScore");
		if (zenObj != null) 
		{
            String id = "";
			zenObj.ReportScore (id, score);
		}
	}
	public void ShowLeaderboard()
	{
		Debug.Log ("ZenSDK: ShowLeaderboard");
		if (zenObj!=null)
			zenObj.ShowLeaderboard ();
	}
		
	//for tracking
	public void OnGameStart()
	{
		Debug.Log ("ZenSDK: OnGameStart");
		if (zenObj!=null)
			zenObj.OnGameStart ();
	}
	public void OnGameOver(string overValue){
		Debug.Log ("ZenSDK: OnGameOver");
		if (zenObj!=null)
			zenObj.OnGameOver (overValue);
	}
	public void OnGameResume()
	{ //resume last game
		Debug.Log ("ZenSDK: OnGameResume");
		if (zenObj!=null)
			zenObj.OnGameResume ();
	}
	public void OnGamePause()
	{ //resume last game
		Debug.Log ("ZenSDK: OnGamePause");
		if (zenObj!=null)
			zenObj.OnGamePause ();
	}
		
	//for ads
	public void ShowFullScreen(string placement, string level)
	{ 
		Debug.Log ("ZenSDK: ShowFullScreen");
		if (zenObj!=null)
			zenObj.ShowFullScreen (placement,level);
	}
	public void ShowBanner(bool visible)
	{ 
		Debug.Log ("ZenSDK: ShowBanner = " + visible);
		if (zenObj!=null)
			zenObj.ShowBanner (visible);
	}
	public void ShowVideoReward(Action<bool> callback,string placement, string level)
	{
		Debug.Log ("ZenSDK: ShowVideoReward");

		if (zenObj!=null)
			zenObj.ShowVideoReward (callback,placement,level);
	}

	public bool IsVideoRewardReady()
	{
		if (zenObj!=null)
		{
			Debug.Log("ZenSDK: IsVideoRewardReady " + zenObj.IsVideoRewardReady());
			return zenObj.IsVideoRewardReady ();
		}
		return false;
	}

    public void ShowAppOpen(Action<bool> callback)
    {
        Debug.Log("ZenSDK: ShowAppOpen");

        if (zenObj != null)
            zenObj.ShowAppOpen(callback);
    }
	

	public bool IsAppOpenReady()
	{
		if (zenObj != null)
		{
			Debug.Log("ZenSDK: IsAppOpenReady " + zenObj.IsAppOpenReady());
			return zenObj.IsAppOpenReady();
		}
		return false;
	}

    public bool IsFullScreenReady()
    {
        if (zenObj != null)
        {
            Debug.Log("ZenSDK: IsFullScreenReady " + zenObj.IsFullScreenReady());
            return zenObj.IsFullScreenReady();
        }
        return false;
    }

    //for notification
    //public void pushNotification(string title, string messsage, long time){}


    //rate
    public void Rate()
	{
		Debug.Log("ZenSDK: Rate");
		if (zenObj != null)
			zenObj.Rate();
		else Debug.Log("ZenSDK: Rate Error");

	}

	//rate
	public void RateInApp()
	{
		Debug.Log("ZenSDK: RateInApp");
		StartCoroutine(inAppReviewManager.CheckForReview());
	}

	//share
	public void Share()
	{
		Debug.Log ("ZenSDK: Share");
		if (zenObj != null)
			zenObj.Share ();
	}

	public void Like()
	{
		Debug.Log ("ZenSDK: Like");
		if (zenObj != null)
			zenObj.Like ();
	}

	public void TrackLevelCompleted(string level, string mode, float duration)
	{
		Debug.Log ("ZenSDK: TrackLevelCompleted");
        if (zenObj != null)
            zenObj.TrackLevelCompleted(level, mode, duration);
	}

    public void TrackLevelStart(string level, string mode)
    {
        Debug.Log("ZenSDK: TrackLevelStart");
        if (zenObj != null)
        zenObj.TrackLevelStart(level, mode);
    }
	public void TrackLevelFailed(string level, string mode, string failed_reason, float duration)
	{
		Debug.Log("ZenSDK: TrackLevelFailed");
		if (zenObj != null)
			zenObj.TrackLevelFailed(level, mode, failed_reason, duration);
	}

	public void TrackRewardOffer(string placement, string level, string is_online)
	{
 		Debug.Log("ZenSDK: TrackRewardOffer");
        if (zenObj != null)
            zenObj.TrackRewardOffer(placement, level,is_online);
	}
	
	public void TrackRewardOfferAccept(string placement, string level, string is_online){
       	Debug.Log("ZenSDK: TrackRewardOfferAccept");
        if (zenObj != null)
            zenObj.TrackRewardOfferAccept(placement, level,is_online);  
    }
	public void TrackPurchaseOffer(string sku, string placement, string level)
    {
        Debug.Log("ZenSDK: TrackPurchaseOffer");
        if (zenObj != null)
            zenObj.TrackPurchaseOffer(sku,placement,level);
    }

	public void TrackPurchaseAccept(string sku, string placement, string level)
    {
    	Debug.Log("ZenSDK: TrackPurchaseAccept");
        if (zenObj != null)
            zenObj.TrackPurchaseAccept(sku,placement,level);
    }
	public void TrackPurchaseSuccess(string sku, string placement, string level)
    {
       	Debug.Log("ZenSDK: TrackPurchaseSuccess");
        if (zenObj != null)
            zenObj.TrackPurchaseSuccess(sku,placement,level);
    }

	public void TrackPurchaseFail(string sku, string placement, string level, string failed_reason)
    {
        Debug.Log("ZenSDK: TrackPurchaseFail");
        if (zenObj != null)
            zenObj.TrackPurchaseFail(sku,placement,level,failed_reason);
    }
	public void TrackSpendCurrency(string virtual_currency_name, int value, string item_name, string level)
    {
        Debug.Log("ZenSDK: TrackSpendCurrency");
        if (zenObj != null)
            zenObj.TrackSpendCurrency(virtual_currency_name, value,item_name,level);
    }
	public void TrackEarnCurrency(string virtual_currency_name, int value, string item_name, string level)
    {
        Debug.Log("ZenSDK: TrackEarnCurrency");
        if (zenObj != null)
            zenObj.TrackEarnCurrency(virtual_currency_name, value,item_name,level);
    }
	public void TrackPromoOffer(string name)
    {
         Debug.Log("ZenSDK: TrackPromoOffer");
        if (zenObj != null)
            zenObj.TrackPromoOffer(name);
    }
	public void TrackPromoClick(string name, string promo)
    {
        Debug.Log("ZenSDK: TrackPromoClick");
        if (zenObj != null)
            zenObj.TrackPromoClick(name, promo);
    }

	public void TrackRateSelect(string placement, string rateValue)
	{
		Debug.Log("ZenSDK: TrackRateSelect");
		if (zenObj != null)
			zenObj.TrackRateSelect(placement, rateValue);
	}

	public void TrackCustomEvent(string eventString)
	{
		Debug.Log("ZenSDK: CustomEvent: " + eventString);
		if (zenObj != null)
			zenObj.TrackCustomEvent(eventString);
	}

    public void TrackRewardNotReady(string placement, string level, string failed_reason){
        Debug.Log("ZenSDK: TrackRewardNotReady");
		if (zenObj != null)
			zenObj.TrackRewardNotReady(placement,level,failed_reason);
    }

    public void TrackRewardStartShow(string placement, string level){
        Debug.Log("ZenSDK: TrackRewardStartShow");
		if (zenObj != null)
			zenObj.TrackRewardStartShow(placement,level);
    }

    public void TrackRewardEndShow(string placement, string level){
        Debug.Log("ZenSDK: TrackRewardEndShow");
		if (zenObj != null)
			zenObj.TrackRewardEndShow(placement,level);
    }
    public void TrackFullscreenStartShow(string placement, string level){
       	Debug.Log("ZenSDK: TrackFullscreenStartShow");
		if (zenObj != null)
			zenObj.TrackFullscreenStartShow(placement,level);
    }

    public void TrackFullscreenEndShow(string placement, string level){
        Debug.Log("ZenSDK: TrackFullscreenEndShow");
		if (zenObj != null)
			zenObj.TrackFullscreenEndShow(placement,level);
    }

    public void TrackFullscreenNotReady(string placement, string failed_reason){
       Debug.Log("ZenSDK: TrackFullscreenNotReady");
		if (zenObj != null)
			zenObj.TrackFullscreenNotReady(placement,failed_reason);
    }

	public int GetConfigInt(string name, int defaultValue)
	{
		Debug.Log("ZenSDK: GetConfigInt");
		if (zenObj != null)
			return zenObj.GetConfigInt(name, defaultValue);
		return defaultValue;
	}

	public string GetConfigString(string name, string defaultValue)
	{
		Debug.Log("ZenSDK: GetConfigString");
		if (zenObj != null)
			return zenObj.GetConfigString(name, defaultValue);
		return defaultValue;
	}

	float pauseTime;
	public Boolean isResumeFromAds = false;

	public void OnApplicationPause(bool pause)
    {
		if (pause)
			pauseTime = Time.realtimeSinceStartup;
	}

	public void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			float pausedTime = Time.realtimeSinceStartup - pauseTime;
			if (GetConfigInt("showAppOpenResume", 1) == 1 && pausedTime >= 10 && isResumeFromAds == false)
			{
				ShowAppOpen(succes => { });
				if (isResumeFromAds) isResumeFromAds = false;
			}
		}
	}

	public bool IsNetworkConnected()
    {
		return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public interface IZenSDK
	{
		void Init ();
	//game service
		void ReportScore (string leaderboardId, long score);
		void ShowLeaderboard ();
	//for tracking
		void OnGameStart();
		void OnGameOver (string overValue);
		void OnGameResume ();
		void OnGamePause ();
		void TrackLevelStart(string level, string mode);
		void TrackLevelFailed(string level, string mode, string failed_reason, float duration);
		void TrackLevelCompleted(string level, string mode, float duration);

		void TrackRewardOffer(string placement, string level, string is_online);
		void TrackRewardOfferAccept(string placement, string level, string is_online);
		void TrackPurchaseOffer(string sku, string placement, string level);
		void TrackPurchaseAccept(string sku, string placement, string level);
		void TrackPurchaseSuccess(string sku, string placement, string level);
		void TrackPurchaseFail(string sku, string placement, string level, string failed_reason);
		void TrackSpendCurrency(string virtual_currency_name, int value, string item_name, string level);
		void TrackEarnCurrency(string virtual_currency_name, int value, string item_name, string level); 
		void TrackPromoOffer(string name);
		void TrackPromoClick(string name, string promo); 
		void TrackRateSelect(string placement, string rateValue);
		void TrackCustomEvent(string eventName);

		void TrackRewardNotReady(string placement, string level, string failed_reason); 
		void TrackRewardStartShow(string placement, string level);
		void TrackRewardEndShow(string placement, string level); 
		void TrackFullscreenStartShow(string placement, string level); 
		void TrackFullscreenEndShow(string placement, string level); 
		void TrackFullscreenNotReady(string placement, string failed_reason);

		int GetConfigInt(string name, int defaultValue);
	
        String GetConfigString(string name,string defaultValue);

        //for ads
        void ShowFullScreen (string placement, string level);
		void ShowBanner (bool visible);
		void ShowVideoReward (Action<bool> callback,string placement, string level);
		bool IsVideoRewardReady();
		bool IsAppOpenReady();
		bool IsFullScreenReady();
        void ShowAppOpen(Action<bool> callback);

        void Share();
		void Rate();
		void Like();
	}
}
