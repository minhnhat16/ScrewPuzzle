using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DailyClaimBtn : MonoBehaviour
{
    public Button claim;
    public Button ads;
    private bool isClaimed;
    [HideInInspector] UnityEvent<bool> onClickClaim = new();
    [HideInInspector] UnityEvent<bool> onClickAds = new();

    private void OnEnable()
    {
        claim.onClick?.AddListener(ClaimBtn);
        ads.onClick?.AddListener(AdsBtn);

    }
    private void OnDisable()
    {
        claim.onClick.RemoveListener(ClaimBtn);
        ads.onClick.RemoveListener(AdsBtn);
    }
    public void CheckButtonType()
    {
        if(isClaimed )
        {
            return;
        }
        else
        {
           claim.gameObject.SetActive(true);
        }
    }
    public void SetButtonEvent(UnityEvent<bool> claimEvent, UnityEvent<bool> adsEvent) 
     {
        //Debug.Log($"sett btn event {claimEvent} +{adsEvent} ");
        this.onClickClaim = claimEvent;
        this.onClickAds = adsEvent;
    }

    public void ClaimBtn()
    {
        Debug.Log("Claim reward");
        onClickClaim?.Invoke(true);
    }
    public void AdsBtn()
    {
        //Debug.Log("Ads Btn");
        onClickAds?.Invoke(true);
    }
}
