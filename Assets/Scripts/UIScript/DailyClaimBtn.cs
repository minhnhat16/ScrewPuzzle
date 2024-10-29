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
    [HideInInspector] private UnityEvent<bool> _onClickClaim = new();
    [HideInInspector]  private UnityEvent<bool> _onClickAds = new();

    private void OnEnable()
    {
        claim.onClick?.AddListener(ClaimBtn);
        /*ads.onClick?.AddListener(AdsBtn);*/

    }
    private void OnDisable()
    {
        claim.onClick.RemoveListener(ClaimBtn);
        /*ads.onClick.RemoveListener(AdsBtn);*/
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
        this._onClickClaim = claimEvent;
        this._onClickAds = adsEvent;
    }

    public void ClaimBtn()
    {
        Debug.Log("Claim reward");
        _onClickClaim?.Invoke(true);
    }
    public void AdsBtn()
    {
        //Debug.Log("Ads Btn");
        _onClickAds?.Invoke(true);
    }
}
