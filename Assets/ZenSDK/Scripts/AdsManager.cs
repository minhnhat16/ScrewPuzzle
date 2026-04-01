using UnityEngine;
using System;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;
using GoogleMobileAds.Api;

public class AdsManager : MonoBehaviour
{
    static public AdsManager instance;
    public GameObject mediationPrefab;
    AdsMediation mediationObj = null;
    public AdmobOpenAdsManager admobOpenAdsManager;

    // Fix: flag để dispatch initMediation về main thread
    private volatile bool _pendingInitMediation = false;

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
            GameObject mediationGO = GameObject.Instantiate(mediationPrefab, this.transform.parent);
            mediationObj = mediationGO.GetComponent<AdsMediation>();
            DontDestroyOnLoad(mediationGO);
        }
    }

    void Start()
    {
        initGDPR();
    }

    void Update()
    {
        // Update() luôn chạy trên Unity main thread
        // Kiểm tra flag và gọi initMediation an toàn tại đây
        if (_pendingInitMediation)
        {
            _pendingInitMediation = false;
            mediationObj.initMediation(admobOpenAdsManager);
        }
    }

    internal void initGDPR()
    {
        // Uncomment để test GDPR trên thiết bị thật:
        // var debugSettings = new ConsentDebugSettings
        // {
        //     DebugGeography = DebugGeography.EEA,
        //     TestDeviceHashedIds = new List<string>
        //     {
        //         "5F95AD9EF1C219AF48B2B748C752054D"
        //     }
        // };
        // ConsentRequestParameters request = new ConsentRequestParameters
        // {
        //     ConsentDebugSettings = debugSettings,
        // };

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
                // Fix: dùng formError thay vì consentError (bug nhỏ trong code gốc)
                Debug.Log("consentError: " + formError);
                return;
            }

            if (ConsentInformation.CanRequestAds())
            {
                // Fix: không gọi thẳng initMediation ở đây vì callback này
                // chạy trên JNI/background thread, sẽ crash MobileAds.Initialize
                // Thay vào đó set flag, Update() sẽ gọi trên main thread
                _pendingInitMediation = true;
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