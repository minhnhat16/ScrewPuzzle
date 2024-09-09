using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsBannerHolder : MonoBehaviour
{
    public static AdsBannerHolder Instance;
    public Transform anchorAds;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        canvas = GetComponent<Canvas>();
    }
    private void Start()
    {
        bool isEnableBanner = true;
        ZenSDK.instance.ShowBanner(isEnableBanner);
    }
}
