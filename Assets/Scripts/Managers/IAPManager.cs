//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Unity.Services.Core;
//using Unity.Services.IAP;
//using UnityEngine;

//public class IAPManager : MonoBehaviour
//{
//    public static IAPManager Instance;

//    private Dictionary<string, Action> onSuccess = new();
//    private Dictionary<string, Action<string>> onFail = new();

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//    }

//    // -------------------------
//    // IAP INIT (Unity 6.2 UGS)
//    // -------------------------
//    public async Task InitializeIAPAsync()
//    {
//        try
//        {
//            Debug.Log("[IAP] Initializing Unity Services...");

//            await UnityServices.InitializeAsync();

//            Debug.Log("[IAP] Initializing In-App Purchasing...");

//            await IAPService.Instance.Initialize();

//            Debug.Log("[IAP] IAP Initialization SUCCESS!");

//            foreach (var p in IAPService.Instance.Catalog)
//                Debug.Log($"[IAP] Product detected: {p.id}");

//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"[IAP] Initialization FAILED: {e}");
//        }
//    }

//    // -------------------------
//    // BUY PRODUCT (Unity 6.2)
//    // -------------------------
//    public async void BuyProduct(string productId, Action onSuccessCallback, Action<string> onFailCallback = null)
//    {
//        try
//        {
//            if (!IAPService.Instance.IsAvailable())
//            {
//                Debug.LogError("[IAP] Service unavailable.");
//                onFailCallback?.Invoke("SERVICE_UNAVAILABLE");
//                return;
//            }

//            Debug.Log("[IAP] Buying product: " + productId);

//            PurchaseResult result = await IAPService.Instance.PurchaseAsync(productId);

//            if (result != null && result.status == PurchaseStatus.Complete)
//            {
//                Debug.Log("[IAP] Purchase SUCCESS: " + productId);
//                onSuccessCallback?.Invoke();
//            }
//            else
//            {
//                Debug.LogError("[IAP] Purchase FAILED: " + productId);
//                onFailCallback?.Invoke(result?.status.ToString());
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.LogError("[IAP] Purchase exception: " + e);
//            onFailCallback?.Invoke(e.Message);
//        }
//    }
//}
