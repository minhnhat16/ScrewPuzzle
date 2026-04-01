using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

internal class InitAdsBanner : IBootTask
{
    public string Name => "InitAdsBanner";

    public IEnumerator Execute()
    {
        yield return new WaitUntil(() => AdsManager.instance.admobOpenAdsManager != null);
    }
}