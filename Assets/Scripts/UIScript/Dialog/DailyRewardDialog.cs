using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using Unity.Profiling;
using UnityEngine.Profiling;

public class DailyRewardDialog : BaseDialog
{
    public DailyClaimBtn claimBtn;
    public DailyGrid dailyGrid;

    [HideInInspector] public UnityEvent<bool> onClickDailyItem = new();
    [HideInInspector] public UnityEvent<bool> onClickClaim = new();
    [HideInInspector] public UnityEvent<bool> onClickAds = new();
    private void OnEnable()
    {
        onClickDailyItem?.AddListener(ClickDailyItem);
        onClickClaim?.AddListener(ClickClaimReward);
        onClickAds?.AddListener(OnClickAdsReward);
    }
    private void OnDisable()
    {
        onClickDailyItem.RemoveListener(ClickDailyItem);
        onClickClaim.RemoveListener(ClickClaimReward);
        onClickAds.RemoveListener(OnClickAdsReward);
    }
    public override void OnInit()
    {
        //Debug.Log("ON DAILY DIALOG INIT");
        claimBtn.SetButtonEvent(onClickClaim, onClickAds);

    }
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        DailyParam param = dialogParam as DailyParam;
        if(param != null)
        {
        dailyGrid.FetchDailyData(param.config);

        }
        
    }
    public override void OnStartShowDialog()
    {
        Profiler.BeginSample("Start dailydialog");
        base.OnStartShowDialog();
        bool isCurrentAvailable = dailyGrid.currentDaily != null;
        //dailyGrid.Content.gameObject.SetActive(true);
        ClickDailyItem(isCurrentAvailable);
        Profiler.EndSample();

    }
    public override void OnStartHideDialog()
    {
        base.OnStartHideDialog();

        var main = ViewManager.Instance.currentView as MainScreenView;
        main.SetLevelPanelIs(true);
    }
    public override void OnEndHideDialog()
    {
        base.OnEndHideDialog();
        //dailyGrid.Content.gameObject.SetActive(false);

    }
    public void ClickDailyItem(bool isEnable)
    {
        //Debug.Log("ClickDailyItem");
        claimBtn.enabled = true;
        if (isEnable)
        {
            //Debug.Log($"Check Button type {isEnable} + claimbtn {claimBtn.gameObject.activeSelf}");
            claimBtn.CheckButtonType();
            claimBtn.gameObject.SetActive(isEnable);
        }
        else if(ZenSDK.instance.IsVideoRewardReady() && !isEnable)
        {
            //claimBtn.enabled = false;
            //Debug.Log($"Check Button type {isEnable}");
            
            claimBtn.claim.gameObject.SetActive(false);
            claimBtn.ads.gameObject.SetActive(true);
        }
        else
        {
            claimBtn.gameObject.SetActive(false);
        }
    }
    public void ClickClaimReward(bool isClaim)
    {
        Debug.Log("ClickClaimReward");

        if (isClaim)
        {
            //Debug.Log("claim reward successful");
            claimBtn.gameObject.SetActive(false);
            dailyGrid.currentDaily.ItemClaim(isClaim);
        }
        Debug.Log("claim reward unsuccessful");

    }
    public void OnClickAdsReward(bool isAds)
    {
        if (isAds && ZenSDK.instance.IsVideoRewardReady())
        {
            //Debug.Log("Ads reward showing");
            ZenSDK.instance.ShowVideoReward((isWatched) =>
            {
                if (isWatched) dailyGrid.currentDaily.ItemClaim(isWatched);
                
            });
        }
    }
    public void QuitButton()
    {
        DialogManager.Instance.HideDialog(dialogIndex, () =>
        {

            //Debug.Log("quit button callback");
        });
    }
}
