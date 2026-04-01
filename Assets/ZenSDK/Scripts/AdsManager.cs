using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using GoogleMobileAds.Ump.Api;
using GoogleMobileAds.Api;

public class AdsManager : MonoBehaviour
{

    static public AdsManager instance;

    public GameObject mediationPrefab;
    AdsMediation mediationObj = null;

    public AdmobOpenAdsManager admobOpenAdsManager;

    private SynchronizationContext mainThreadContext;

    // Use this for initialization
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        if (mediationPrefab != null)
        {
            GameObject mediationGO = GameObject.Instantiate(mediationPrefab);
            mediationObj = mediationGO.GetComponent<AdsMediation>();
            DontDestroyOnLoad(mediationGO);
        }
    }

    void Start()
    {
        // Capture the main thread context so we can safely return to it later
        mainThreadContext = SynchronizationContext.Current;
        initGDPR();
    }

    void initGDPR()
    {

        // var debugSettings = new ConsentDebugSettings
        //  {
        //      DebugGeography = DebugGeography.EEA,
        //      TestDeviceHashedIds = new List<string>
        //  {
        //      "5F95AD9EF1C219AF48B2B748C752054D"
        //  }
        //  };



        //  ConsentRequestParameters request = new ConsentRequestParameters
        //  {
        //      ConsentDebugSettings = debugSettings,
        //  };

        ConsentRequestParameters request = new ConsentRequestParameters();

        ConsentInformation.Update(request, OnConsentInfoUpdated);

        Debug.Log("AdsManager init");
    }

    void OnConsentInfoUpdated(FormError consentError)
    {
        if (consentError != null)
        {
            Debug.Log("consentError: " + consentError);
            return;
        }

        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if (formError != null)
            {

                Debug.Log("consentError: " + consentError);
                return;
            }
            if (ConsentInformation.CanRequestAds())
            {
                // Send the initialization call back to the main thread securely 
                // without using the Update loop.
                mainThreadContext.Post(_ =>
                {
                    mediationObj.initMediation(admobOpenAdsManager);
                }, null);
            }
        });
    }


    public void showInterstitial(string placement, string level)
    {
        mediationObj.showInterstitial(placement, level);
    }

    public void showBanner(bool visible)
    {
        mediationObj.showBanner(visible);
    }

    public bool isVideoRewardReady()
    {
        return mediationObj.isVideoRewardReady();
    }

    public bool isFullScreenReady()
    {
        return mediationObj.isFullScreenReady();
    }

    public void showVideoReward(Action<bool> callback, string placement, string level)
    {
        mediationObj.showVideoReward(callback, placement, level);
    }

    public void showAppOpen(Action<bool> callback)
    {
        mediationObj.showAppOpen(callback);
    }

    public bool isAppOpenReady()
    {
        return mediationObj.isAppOpenReady();
    }

    public interface AdsMediation
    {
        void initMediation(AdmobOpenAdsManager admobOpenAdsManager);
        void showInterstitial(string placement, string level);
        void showBanner(bool visible);
        bool isVideoRewardReady();
        bool isAppOpenReady();
        bool isFullScreenReady();

        void showVideoReward(Action<bool> callback, string placement, string level);
        void showAppOpen(Action<bool> callback);
    }
}