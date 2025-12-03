using Managers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class LoseDialog : BaseDialog
{

    private readonly LoseParam param;
    [SerializeField] Button btn_retry;
    [SerializeField] Button btn_Watch;


    private void OnEnable()
    {
        btn_retry.onClick.AddListener(OnRetryButtonClicked);
        btn_Watch.onClick.AddListener(OnWatchButtonClicked);
    }

    private void OnWatchButtonClicked()
    {

        //ZenSDK.instance.ShowVideoReward( (isWatched) =>
        //{
        //    if(isWatched)
        //    {
     

        DialogManager.ins.HideDialog(DialogIndex.LoseDialog, () =>
        {
            IngameController.ins.OnRevive();
            IngameController.ins.onItemInvoke.Invoke(ItemType.Magnet);
        });
        //    }   
        //    DialogManager.ins.HideDialog(DialogIndex.LoseDialog);
        //});
    }

    private void OnRetryButtonClicked()
    {
        ZenSDK.instance.ShowFullScreen();

        DialogManager.ins.HideDialog(DialogIndex.LoseDialog, () =>
        {
            IngameController.ins.OnGameOver();
        });

    }
}
