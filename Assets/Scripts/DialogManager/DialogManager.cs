
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogManager : SingletonMono<DialogManager>
{
    public Transform anchorDialog;
    public Dictionary<DialogIndex, BaseDialog> dicDialog = new Dictionary<DialogIndex, BaseDialog>();
    private List<BaseDialog> dialogShowed = new List<BaseDialog>();
    public List<BaseDialog> dialogList = new List<BaseDialog>();

    private void Start()
    {
        PaymentManager.ins.OnPaymentCompleted += OnPaymentDone;
    }


    public override void Awake()
    {
        base.Awake();
        dialogList = GetComponentsInChildren<BaseDialog>(true).ToList();
    }

    public IEnumerator Init()
    {
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < dialogList.Count; i++)
        {
            BaseDialog dialog = dialogList[i].GetComponent<BaseDialog>();

            // ⭐ chạy init async
            yield return dialog.Init();

            // ⭐ chỉ add khi init xong 100%
            dicDialog.Add(dialogList[i].dialogIndex, dialogList[i]);
        }
    }

    public void ShowDialog(DialogIndex newDialog, DialogParam dialogParam = null, Action callback = null)
    {
        BaseDialog dialog = dicDialog[newDialog];
        //Debug.Log("ShowDialog");
        if (!dialogShowed.Contains(dialog))
        {
            dialogShowed.Add(dialog);
        }

        dialog.gameObject.SetActive(true);
        dialog.Setup(dialogParam);
        dialog.ShowDialogAnimation(callback);
    }

    public void HideDialog(DialogIndex newDialog, Action callback = null)
    {
        BaseDialog dialog = dicDialog[newDialog];
        //Debug.Log("Hidedialog" + callback);
        if (dialogShowed.Contains(dialog))
        {
            dialogShowed.Remove(dialog);
        }
        else return;
        dialog.HideDialogAnimation(() =>
        {
            callback?.Invoke();
            //Debug.Log(callback);
            dialog.gameObject.SetActive(false);
        });

    }

    public void HideAllDialog()
    {
        foreach (BaseDialog dialog in dialogShowed)
        {
            dialog.HideDialogAnimation(null);
            dialog.gameObject.SetActive(false);
        }
        dialogShowed.Clear();
    }



    private void OnPaymentDone(PaymentResult result)
    {
        if (result.success)
        {
            //DialogManager.ins.ShowDialog(DialogIndex.NotifyDialog, new NotifyDialog()
            //{
            //    header = "Completed",
            //    message = result.message
            //});
        }
        else
        {
            //DialogManager.ins.ShowDialog(DialogIndex.NotifyDialog, new NotifyDialog()
            //{
            //    header = "Failed",
            //    message = result.message
            //});
        }
    }

    internal bool IsShowingDialog(DialogIndex settingDialog)
    {
        var dialog = dicDialog[settingDialog];
        return dialogShowed.Contains(dialog);
    }
}
