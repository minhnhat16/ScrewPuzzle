using UnityEngine;
using System;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;

public class AdmobMediation : MonoBehaviour, AdsManager.AdsMediation
{

    public string AndroidAdmobAppId = "ca-app-pub-3940256099942544~3347511713";
    public string AndroidAdmobBannerAdsId = "ca-app-pub-3940256099942544/6300978111";
    public string AndroidAdmobFullscreenAdsId = "ca-app-pub-3940256099942544/1033173712";
    public string AndroidAdmobVideoAdsId = "ca-app-pub-3940256099942544/5224354917";


    public string IOSAdmobAppId = "";
    public string IOSAdmobBannerAdsId = "";
    public string IOSAdmobFullscreenAdsId = "";
    public string IOSAdmobVideoAdsId = "";

    string appIDAdmob = "";
    string bannerAdmobAdsId = "";
    string fullscreenAdmobAdsId = "";
    string videoRewardAdmonAdsId = "";

    BannerView bannerAds;
    InterstitialAd interstitial;
    RewardedAd rewardedAd;

    bool firstShowFulscreen = true;

    static bool bannerVisible = false;

    static bool videoRewardReady = false;
    static float lastShowInterstitial = 0;

    static Action<bool> lastVideoRewardCallback;

    AdSize adaptiveSize;

    AdmobOpenAdsManager appopenManager;

    string fullscreen_placement;
    string fullscreen_level;
    string video_placement;
    string video_level;

    public void initMediation(AdmobOpenAdsManager admobOpenAdsManager)
    {
#if UNITY_IOS
        appIDAdmob = IOSAdmobAppId;
	    bannerAdmobAdsId = IOSAdmobBannerAdsId;
	    fullscreenAdmobAdsId = IOSAdmobFullscreenAdsId;
	    videoRewardAdmonAdsId = IOSAdmobVideoAdsId;
#else
        appIDAdmob = AndroidAdmobAppId;
        bannerAdmobAdsId = AndroidAdmobBannerAdsId;
        fullscreenAdmobAdsId = AndroidAdmobFullscreenAdsId;
        videoRewardAdmonAdsId = AndroidAdmobVideoAdsId;
#endif
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        // RequestConfiguration requestConfiguration = new RequestConfiguration();
        // requestConfiguration.TestDeviceIds.Add("5F95AD9EF1C219AF48B2B748C752054D");
        // MobileAds.SetRequestConfiguration(requestConfiguration);

        try
        {
            MobileAds.Initialize(initStatus =>
            {
                if (initStatus == null)
                {
                    Debug.LogError("Google Mobile Ads initialization failed.");
                    return;
                }
                initBanner();
                initInterstitial();
                initVideoReward();
                appopenManager = admobOpenAdsManager;
                appopenManager.initAppOpen();
                Debug.Log("Admob Inited");
            });

        }
        catch (Exception e)
        {
            Debug.LogError("Google Mobile Ads initialization exception: " + e);
        }
    }

    void initBanner()
    {

        //Clean old Banner(
        if (this.bannerAds != null)
        {
            this.bannerAds.Destroy();
        }
        //Get AdBanner Size
        if (Screen.orientation == ScreenOrientation.Portrait)
        {
            adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        }
        else
        {
            adaptiveSize = AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        }

        bannerAds = new BannerView(bannerAdmobAdsId, adaptiveSize, AdPosition.Bottom);

        RegisterEventHandlers(bannerAds);

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        bannerAds.LoadAd(adRequest);
    }

    void initInterstitial()
    {
        //Clean old Interstitial
        if (this.interstitial != null)
        {
            this.interstitial.Destroy();
        }
        var adRequest = new AdRequest();
        // send the request to load the ad.
        InterstitialAd.Load(fullscreenAdmobAdsId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " + "with error : " + error);
                    return;
                }
                Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
                interstitial = ad;
                RegisterEventHandlers(ad);
            });
    }

    void initVideoReward()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
        }
        // create our request used to load the ad.
        var adRequest = new AdRequest();
        // send the request to load the ad.
        RewardedAd.Load(videoRewardAdmonAdsId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                    videoRewardReady = false;
                    if (lastVideoRewardCallback != null)
                        lastVideoRewardCallback(false);
                    initVideoReward();
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                videoRewardReady = true;
                rewardedAd = ad;
                RegisterEventHandlers(ad);
            });
    }

    public void showInterstitial(string placement, string level)
    {
        if (firstShowFulscreen)
        {
            showFullscreen(placement, level);
            return;
        }

        float s = Time.realtimeSinceStartup - lastShowInterstitial;
        if (s < ZenSDK.instance.GetConfigInt("fullscreenTime", 30))
        {
            //PromoUI.Instance.OpenPromoUI();
            Debug.Log("Not enought wait time, show crosspromo");
            return;
        }

        showFullscreen(placement, level);
    }
    void showFullscreen(string placement, string level)
    {
        fullscreen_placement = placement;
        fullscreen_level = level;

        if (interstitial != null && interstitial.CanShowAd())
        {
            ZenSDK.instance.isResumeFromAds = true;
            interstitial.Show();
            lastShowInterstitial = Time.realtimeSinceStartup;
            Debug.Log("showInterstitial");
            firstShowFulscreen = false;
        }
        else
        {
            //PromoUI.Instance.OpenPromoUI();
            Debug.Log("Interstitial not ready, open crosspromo");
            initInterstitial();
        }
    }

    void showGameBanner()
    {
        if (bannerVisible == true)
        {
            bannerAds.Show();
        }
        else
        {
            bannerAds.Hide();
        }
    }
    public void showBanner(bool visible)
    {
        if (bannerAds != null)
        {
            Debug.Log("ShowBanner " + visible + " banner" + bannerAds);
            bannerVisible = visible;
            showGameBanner();
        }
        else
        {
            initBanner();
        }
    }

    public void showVideoReward(Action<bool> callback, string placement, string level)
    {
        video_placement = placement;
        video_level = level;

        lastVideoRewardCallback = callback;

        Debug.Log("showVideoReward " + videoRewardReady);
        if (videoRewardReady == false)
        {
            lastVideoRewardCallback(false);
            return;
        }
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            ZenSDK.instance.isResumeFromAds = true;

            rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
            {
                if (lastVideoRewardCallback != null)
                {
                    lastVideoRewardCallback(true);
                    lastVideoRewardCallback = null;
                }
            });
        }

    }

    public bool isVideoRewardReady()
    {
        Debug.Log("isVideoRewardReady " + videoRewardReady);
        return videoRewardReady;
    }
    public bool isFullScreenReady()
    {
        return interstitial.CanShowAd() && (Time.realtimeSinceStartup - lastShowInterstitial) < ZenSDK.instance.GetConfigInt("fullscreenTime", 30);
    }

    public void showAppOpen(Action<bool> callback)
    {
        if (appopenManager != null)
            appopenManager.showAppOpen(callback);
    }


    public bool isAppOpenReady()
    {
        if (appopenManager != null)
            return appopenManager.isAppOpenAvailable;
        else return false;
    }



    //Handle Banner
    void RegisterEventHandlers(BannerView ad)
    {
        // Raised when an ad is loaded into the banner view.
        ad.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : " + bannerAds.GetResponseInfo());
            showGameBanner();
        };
        // Raised when an ad fails to load into the banner view.
        ad.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : " + error);
        };
        // bannerAds when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    //Handle Interstitial
    void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            initInterstitial();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);

            initInterstitial();
        };
    }

    //Handle Video
    void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            videoRewardReady = false;
            initVideoReward();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " + "with error : " + error);
            if (lastVideoRewardCallback != null)
            {
                lastVideoRewardCallback(false);
                lastVideoRewardCallback = null;
            }
            initVideoReward();
        };
    }

}
