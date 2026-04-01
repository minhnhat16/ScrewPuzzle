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
    }

    public void ShowBanner(bool isShow)
    {
        ZenSDK.instance.ShowBanner(isShow);
        canvas.enabled = isShow;
    }
}
