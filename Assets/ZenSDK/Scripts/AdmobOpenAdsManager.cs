using UnityEngine;
using System;
using GoogleMobileAds.Api;

public class AdmobOpenAdsManager : MonoBehaviour
{
    public string AndroidAdmobAppOpenAdsId = "ca-app-pub-1015023790649631/8533834163";

    public string IOSAdmobAppOpenAdsId = "";

    string appOpenAdmobAdsId = "";

    AppOpenAd appOpen;

    static Action<bool> lastAppOpenCallback;

    DateTime expireTime;
    bool IsAppOpenLoaded = false;
    bool IsShowingAppOpen = false;

    public void initAppOpen()
    {
#if UNITY_IOS
        appOpenAdmobAdsId = IOSAdmobAppOpenAdsId;

#else
        appOpenAdmobAdsId = AndroidAdmobAppOpenAdsId;
#endif

        if (appOpen != null)
        {
            appOpen.Destroy();
            appOpen = null;
        }
        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load(appOpenAdmobAdsId, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " + "with error : " + error);
                    IsAppOpenLoaded = false;
                    return;
                }

                Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());

                // App open ads can be preloaded for up to 4 hours.
                expireTime = DateTime.Now + TimeSpan.FromHours(4);
                IsAppOpenLoaded = true;

                appOpen = ad;
                RegisterEventHandlers(ad);
            });
    }

    public void showAppOpen(Action<bool> callback)
    {
        Debug.Log("IsShowingAppOpen " + IsShowingAppOpen);
        if (!IsShowingAppOpen)
        {
            lastAppOpenCallback = callback;
            if (isAppOpenAvailable)
            {
                Debug.Log("Showing app open ad.");
                appOpen.Show();
                IsShowingAppOpen = true;
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
                return;
            }
        }
        else
        {
            Debug.LogError("App open ad is showing."); 
            return;
        }
     }

    public bool isAppOpenAvailable
    {
        get
        {
            return appOpen != null && IsAppOpenLoaded == true && DateTime.Now < expireTime;
        }
    }

    //Handle AppOpen
    void RegisterEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
            if (lastAppOpenCallback != null)
            {
                lastAppOpenCallback(true);
                lastAppOpenCallback = null;
            }
            IsShowingAppOpen = false;
            initAppOpen();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " + "with error : " + error);
            if (lastAppOpenCallback != null)
            {
                lastAppOpenCallback(true);
                lastAppOpenCallback = null;
            }
            initAppOpen();
        };
    }
}
