using System.Collections;
using UnityEngine;
using Google.Play.Review;

public class InAppReviewManager : MonoBehaviour
{
    static public InAppReviewManager instance;

    ReviewManager appReviewManager;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        if (Application.platform == RuntimePlatform.Android)
        {
            this.appReviewManager = new ReviewManager();
        }
    }

    void Start()
    {
        
    }

    public IEnumerator CheckForReview()
    {
        var requestFlowOperation = appReviewManager.RequestReviewFlow();

        yield return requestFlowOperation;

        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            print("request review errors");
            yield break;
        }

        if (requestFlowOperation.IsSuccessful) {
            var appReviewInfo = requestFlowOperation.GetResult();
            StartCoroutine(StartAppReview(appReviewInfo));
        }
    }

    IEnumerator StartAppReview(PlayReviewInfo appReviewInfo_i) {
        var launchFlowOperation = appReviewManager.LaunchReviewFlow(appReviewInfo_i);
        yield return launchFlowOperation;
        appReviewInfo_i = null;
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            print("app review errors");
            yield break;
        }
    }
}
