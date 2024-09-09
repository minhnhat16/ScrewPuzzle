using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Security;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuyDialog : BaseDialog
{
    [SerializeField] private int cost;
    [SerializeField] private  Text total_lb;
    [SerializeField] private Text amount_lb;
    [SerializeField] private Text cost_lb;
    [SerializeField] Button buyButton;
    [SerializeField] Button AdsButton;

    [SerializeField] private Text explain_lb;
    private Action onConfirm;
    private Action onCancel;
    private void OnEnable()
    {
        //buyButton.onClick.AddListener(()=>ConfirmBuy());
        //AdsButton.onClick.AddListener(() => ConfirmWatch());
    }
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        if (dialogParam != null)
        {
            BuyConfirmDialogParam param = (BuyConfirmDialogParam)dialogParam;
            onConfirm = param.onConfirmAction;
            onCancel = param.onCancleAction;
            cost = param.cost;
            explain_lb.text = param.plaintext;
        }
    }
    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        if(cost > 0)
        {
            buyButton.gameObject.SetActive(true);
            AdsButton.gameObject.SetActive(false);
        }
        else
        {
            AdsButton.gameObject.SetActive(true);
            buyButton.gameObject.SetActive(false);
        }
    }
    public void HideConfirmDialog()
    {
        DialogManager.Instance.HideDialog(dialogIndex);
    }
    public void ConfirmWatch()
    {
        ZenSDK.instance.ShowVideoReward((isWatched) =>
        {
            if (isWatched) ConfirmBuy();
            else
            {
                //Debug.Log("NO ADS TO WATCH");
                HideConfirmDialog();
            }
        });
    }
    public void ConfirmBuy()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        onConfirm?.Invoke();
        HideConfirmDialog();
    }

    public void CancleBuy()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.UIClickSFX);
        onCancel?.Invoke();
        HideConfirmDialog();
    }
}
