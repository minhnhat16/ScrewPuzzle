using UnityEngine;
using System;

public class ZenSDKV1 :MonoBehaviour, ZenSDK.IZenSDK {

	public void Init()
	{
		Application.targetFrameRate = 60;

		//init leaderboard;
		#if UNITY_ANDROID

        #elif UNITY_IOS

		#else

		#endif

		Debug.Log("ZenSDKV1:Init");

	}

	//tracking
	public void OnGameStart(){
	}
	public void OnGameOver(string overValue){
	}
	public void OnGameResume(){
	}
	public void OnGamePause(){		

	}

    public void TrackLevelStart(string level, string mode)
    {
        FirebaseAnalysticManager.instance.sendLevelStart(level, mode);
    }

    public void TrackLevelFailed(string level, string mode, string failedReason, float duration)
    {
        FirebaseAnalysticManager.instance.sendLevelFail(level, mode, failedReason, duration);
    }

    public void TrackLevelCompleted(string level, string mode, float duration)
    {
        FirebaseAnalysticManager.instance.sendLevelComplete(level, mode, duration);
    }

    public void TrackRewardOffer(string placement, string level, string is_online)
	{
        FirebaseAnalysticManager.instance.sendRewardOffer(placement,level,is_online);
	}
	
	public void TrackRewardOfferAccept(string placement, string level, string is_online){
        FirebaseAnalysticManager.instance.sendRewardOfferAccept(placement,level,is_online);
    }
	public void TrackPurchaseOffer(string sku, string placement, string level)
    {
        FirebaseAnalysticManager.instance.sendPurchaseOffer(sku, placement, level);
    }

	public void TrackPurchaseAccept(string sku, string placement, string level)
    {
    	 FirebaseAnalysticManager.instance.sendPurchaseAccept(sku, placement, level);
    }
	public void TrackPurchaseSuccess(string sku, string placement, string level)
    {
        FirebaseAnalysticManager.instance.sendPurchaseSuccess(sku, placement, level);
    }

	public void TrackPurchaseFail(string sku, string placement, string level, string failed_reason)
    {
         FirebaseAnalysticManager.instance.sendPurchaseFail(sku, placement, level,failed_reason);
    }
	public void TrackSpendCurrency(string virtual_currency_name, int value, string item_name, string level)
    {
        FirebaseAnalysticManager.instance.sendSpendCurrency(virtual_currency_name,value,item_name,level);
    }
	public void TrackEarnCurrency(string virtual_currency_name, int value, string item_name, string level)
    {
         FirebaseAnalysticManager.instance.sendEarnCurrency(virtual_currency_name,value,item_name,level);
    }
	public void TrackPromoOffer(string name)
    {
        FirebaseAnalysticManager.instance.sendPromoOffer(name);
    }
	public void TrackPromoClick(string name, string promo)
    {
        FirebaseAnalysticManager.instance.sendPromoClick(name,promo);
    }

	public void TrackRateSelect(string placement, string rateValue)
	{
        FirebaseAnalysticManager.instance.sendRateSelect(placement,rateValue);
    }

    public void TrackCustomEvent(string eventName)
    {
        FirebaseAnalysticManager.instance.sendTrackEvent(eventName);
    }

    public void TrackRewardNotReady(string placement, string level, string failed_reason){
        FirebaseAnalysticManager.instance.sendRewardNotReady(placement,level,failed_reason);
    }

    public void TrackRewardStartShow(string placement, string level){
        FirebaseAnalysticManager.instance.sendRewardStartShow(placement,level);
    }

    public void TrackRewardEndShow(string placement, string level){
        FirebaseAnalysticManager.instance.sendRewardEndShow(placement,level);
    }
    public void TrackFullscreenStartShow(string placement, string level){
       	FirebaseAnalysticManager.instance.sendFullscreenStartShow(placement,level);
    }

    public void TrackFullscreenEndShow(string placement, string level){
        FirebaseAnalysticManager.instance.sendFullscreenEndShow(placement,level);
    }

    public void TrackFullscreenNotReady(string placement, string failed_reason){
       FirebaseAnalysticManager.instance.sendFullscreenNotReady(placement,failed_reason);
    }


    public int GetConfigInt(string name, int defaultValue)
    {
        return FirebaseAnalysticManager.instance.GetConfigInt(name,defaultValue);
    }

    public string GetConfigString(string name, string defaultValue)
    {
        return FirebaseAnalysticManager.instance.GetConfigString(name,defaultValue);
    }

    //leaderboard
    public void ReportScore(string leaderboardID, long score)
    {

    }

	public void ShowLeaderboard()
	{
		
	}



	//ads
	public void ShowFullScreen (string placement, string level){
		AdsManager.instance.showInterstitial (placement,level);
	}
	public void ShowBanner (bool visible){
		AdsManager.instance.showBanner (visible);
	}
	public void ShowVideoReward (Action<bool> callback,string placement, string level){
		AdsManager.instance.showVideoReward (callback,placement,level);

	}
	public bool IsVideoRewardReady(){
        return AdsManager.instance.isVideoRewardReady();
	}

    public bool IsFullScreenReady()
    {
        return AdsManager.instance.isFullScreenReady();
    }
    public void ShowAppOpen(Action<bool> callback)
    {
        int appOpenCount = PlayerPrefs.GetInt("appOpenCount", 0);
        if (appOpenCount > GetConfigInt("appOpenCount", 0) && GetConfigInt("isShowAppOpen", 1) == 1)
        {
            Debug.Log("show app open");
            AdsManager.instance.showAppOpen(callback);
    }
        else
        {
            callback(true);
            appOpenCount++;
            PlayerPrefs.SetInt("appOpenCount", appOpenCount);
            Debug.Log("appOpenCount "+ appOpenCount);
        }
    }
    
    public bool IsAppOpenReady(){
        return AdsManager.instance.isAppOpenReady();
	}

    //rate
    public void Rate()
    {
#if UNITY_ANDROID
        Application.OpenURL("market://details?id=com.geda.jigsolitaire");
#elif UNITY_IOS
        Application.OpenURL("itms-apps://itunes.apple.com/app/id6499258826");
#endif

    }

	//share
	public void Share(){

	}

	public void Like(){


	}

}
