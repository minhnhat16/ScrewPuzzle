using System;
using System.Threading.Tasks;
using UnityEngine;

using Firebase;
using Firebase.Analytics;
using Firebase.RemoteConfig;
using Firebase.Extensions;
using Google.MiniJSON;
using UnityEngine.Tilemaps;


public class FirebaseAnalysticManager : MonoBehaviour
{
    static public FirebaseAnalysticManager instance;
    int gameCount = 0;
    public bool firebaseInitialized = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        FirebaseAnalytics.SetUserProperty(FirebaseAnalytics.UserPropertySignUpMethod,"Google");
      
        FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

        initRemoteConfig();

        firebaseInitialized = true;

        gameCount = PlayerPrefs.GetInt("openApp", 0);
        gameCount++;
        PlayerPrefs.SetInt("openApp", gameCount);

        FetchDataAsync();
    }

    public void sendTrackEvent(string eventName)
    {
        if (firebaseInitialized)
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
    }

    public void sendLevelStart(string level, string mode)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendLevelStart");
            FirebaseAnalytics.LogEvent("LevelStart", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "level", level),
                new Firebase.Analytics.Parameter(
                    "mode", mode)
            });
        }
    }
    public void sendLevelFail(string level, string mode, string failedReason, float duration)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendLevelFail");
            FirebaseAnalytics.LogEvent("LevelFail", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "level", level),
                new Firebase.Analytics.Parameter(
                    "mode", mode),
                new Firebase.Analytics.Parameter(
                    "failed_reason", failedReason),
                new Firebase.Analytics.Parameter(
                    "duration", duration)
            });
        }
    }
    public void sendLevelComplete(string level, string mode, float duration)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendLevelComplete");
            FirebaseAnalytics.LogEvent("LevelComplete", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "level", level),
                new Firebase.Analytics.Parameter(
                    "mode", mode),
                new Firebase.Analytics.Parameter(
                    "duration", duration)

            });
        }
    }
    public void sendRewardOffer(string placement, string level, string is_online)
	{
 		 if (firebaseInitialized)
        {
            Debug.Log("Send event sendRewardOffer");
            FirebaseAnalytics.LogEvent("reward_offer", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level),
                new Firebase.Analytics.Parameter(
                    "is_online", is_online)
            });
        }
	}

    public void sendRewardOfferAccept(string placement, string level, string is_online)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendRewardOfferAccept");
            FirebaseAnalytics.LogEvent("reward_offeraccept", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level),
                new Firebase.Analytics.Parameter(
                    "is_online", is_online)
            });
        }
    }
	public void sendPurchaseOffer(string sku, string placement, string level)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendPurchaseOffer");
            FirebaseAnalytics.LogEvent("purchase_offer", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "sku", sku),
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }

	public void sendPurchaseAccept(string sku, string placement, string level)
    {
    	if (firebaseInitialized)
        {
            Debug.Log("Send event sendPurchaseAccept");
            FirebaseAnalytics.LogEvent("purchase_accept", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "sku", sku),
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }
	public void sendPurchaseSuccess(string sku, string placement, string level)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendPurchaseSuccess");
            FirebaseAnalytics.LogEvent("purchase_success", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "sku", sku),
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }

	public void sendPurchaseFail(string sku, string placement, string level, string failed_reason)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendPurchaseFail");
            FirebaseAnalytics.LogEvent("purchase_failure", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "sku", sku),
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level),
                new Firebase.Analytics.Parameter(
                    "failed_reason", failed_reason),
            });
        }
    }
	public void sendSpendCurrency(string virtual_currency_name, int value, string item_name, string level)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendSpendCurrency");
            FirebaseAnalytics.LogEvent("spend_virtual_currency", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "virtual_currency_name", virtual_currency_name),
                new Firebase.Analytics.Parameter(
                    "value", value),
                new Firebase.Analytics.Parameter(
                    "item_name", item_name),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }
	public void sendEarnCurrency(string virtual_currency_name, int value, string item_name, string level)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendEarnCurrency");
            FirebaseAnalytics.LogEvent("earn_virtual_currency", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "virtual_currency_name", virtual_currency_name),
                new Firebase.Analytics.Parameter(
                    "value", value),
                new Firebase.Analytics.Parameter(
                    "source", item_name),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }
	public void sendPromoOffer(string name)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendPromoOffer");
            FirebaseAnalytics.LogEvent("cross_promotion_offer", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "name", name)
            });
        }
    }
	public void sendPromoClick(string name, string promo)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendPromoClick");
            FirebaseAnalytics.LogEvent("cross_promotion_click", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "name", name),
                new Firebase.Analytics.Parameter(
                    "promo", promo)
            });
        }
    }

    public void sendRateSelect(string placement, string rateValue)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendRateSelect");
            FirebaseAnalytics.LogEvent("rate_select", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "rateValue", rateValue)
            });
        }
    }

    public void sendRewardNotReady(string placement, string level, string failed_reason){
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendRewardNotReady");
            FirebaseAnalytics.LogEvent("reward_notready", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level),
                new Firebase.Analytics.Parameter(
                    "failed_reason", failed_reason)
            });
        }
    }

    public void sendRewardStartShow(string placement, string level){
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendRewardStartShow");
            FirebaseAnalytics.LogEvent("reward_startshow", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }

    public void sendRewardEndShow(string placement, string level){
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendRewardEndShow");
            FirebaseAnalytics.LogEvent("reward_endshow", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }
    public void sendFullscreenStartShow(string placement, string level){
       	 if (firebaseInitialized)
        {
            Debug.Log("Send event sendFullscreenStartShow");
            FirebaseAnalytics.LogEvent("fullscreen_startshow", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }

    public void sendFullscreenEndShow(string placement, string level)
    {
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendFullscreenEndShow");
            FirebaseAnalytics.LogEvent("fullscreen_endshow", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "level", level)
            });
        }
    }

    public void sendFullscreenNotReady(string placement, string failed_reason){
        if (firebaseInitialized)
        {
            Debug.Log("Send event sendFullscreenNotReady");
            FirebaseAnalytics.LogEvent("fullscreen_notready", new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(
                    "placement", placement),
                new Firebase.Analytics.Parameter(
                    "failed_reason", failed_reason)
            });
        }
    }

    public int GetConfigInt(string name, int defaultValue)
    {

        if (firebaseInitialized)
        {
            String v = FirebaseRemoteConfig.DefaultInstance.GetValue(name).StringValue;
            if (v.Equals(""))
                return defaultValue;

            int r = int.Parse(v);
            return r;
        }
        return defaultValue;
    }
    
    public string GetConfigString(string name, string defaultValue)
    {
        if (firebaseInitialized)
        {
            String v = FirebaseRemoteConfig.DefaultInstance.GetValue(name).StringValue;
            if (v.Equals(""))
                return defaultValue;
            return v;
        }
        return defaultValue;
    }
    
    public void initRemoteConfig() {
        System.Collections.Generic.Dictionary<string, object> defaults = new System.Collections.Generic.Dictionary<string, object>();

        defaults.Add("test", "test");

        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);

        Debug.Log("RemoteConfig configured and ready!");
    }

    public Task FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        System.Threading.Tasks.Task fetchTask =
        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
            TimeSpan.Zero);
        FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    void FetchComplete(Task fetchTask)
    {
        if (!fetchTask.IsCompleted)
        {
            Debug.LogError("Retrieval hasn't finished.");
            return;
        }

        var remoteConfig = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;
        if (info.LastFetchStatus != Firebase.RemoteConfig.LastFetchStatus.Success)
        {
            Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
            return;
        }

        remoteConfig.ActivateAsync()
          .ContinueWithOnMainThread(
            task => {
                Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");
            });
    }
}
