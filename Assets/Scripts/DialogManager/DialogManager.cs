using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Linq;
using UnityEngine;

public class DialogManager : SingletonMono<DialogManager>, IDialogService
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
            yield return dialog.Init();
            dicDialog.Add(dialogList[i].dialogIndex, dialogList[i]);
        }
    }

    // ─────────────────────────────────────────
    // Core show / hide
    // ─────────────────────────────────────────

    public void ShowDialog(DialogIndex newDialog, DialogParam dialogParam = null, Action callback = null)
    {
        BaseDialog dialog = dicDialog[newDialog];
        if (!dialogShowed.Contains(dialog))
            dialogShowed.Add(dialog);

        dialog.gameObject.SetActive(true);
        dialog.Setup(dialogParam);
        dialog.ShowDialogAnimation(callback);
    }

    public void HideDialog(DialogIndex newDialog, Action callback = null)
    {
        BaseDialog dialog = dicDialog[newDialog];
        if (dialogShowed.Contains(dialog))
            dialogShowed.Remove(dialog);
        else return;

        dialog.HideDialogAnimation(() =>
        {
            callback?.Invoke();
            dialog.gameObject.SetActive(false);
        });
    }

    public void HideAllDialog() 
    {

        Debug.Log($"[DialogManager] HideAllDialog: {dialogShowed.Count} dialog(s) will be hidden.");
        foreach (BaseDialog dialog in dialogShowed)
        {
            dialog.HideDialogAnimation(null);
            dialog.gameObject.SetActive(false);
        }
        dialogShowed.Clear();
    }

    // ─────────────────────────────────────────
    // IDialogService — kết thúc level
    // ─────────────────────────────────────────

    void IDialogService.ShowWinDialog(WinParam param)
    {
        ShowDialog(DialogIndex.WinDialog, param);
    }

    void IDialogService.ShowLoseDialog(LoseParam param)
    {
        ShowDialog(DialogIndex.LoseDialog, param);
    }

    // ─────────────────────────────────────────
    // IDialogService — interrupt gameplay
    // ─────────────────────────────────────────

    void IDialogService.ShowReviveDialog(ReviveParam param, Action onDeclined)
    {
        // Gắn onDeclined vào param để ReviveDialog gọi khi player từ chối
        if (param != null)
            param.onDeclined = onDeclined;

        ShowDialog(DialogIndex.ReviveDialog, param);
    }

    void IDialogService.ShowItemDialog(AddItemDialogParam param, Action onClosed)
    {
        ShowDialog(DialogIndex.ItemDialog, param, onClosed);
    }

    void IDialogService.ShowPause(SettingParam param, Action onResumed)
    {
        var settingParam = param ?? new SettingParam
        {
            isMainScreen = false,
            totalGold = WalletManager.ins.Get(Currency.Gold),
            totalTicket = WalletManager.ins.Get(Currency.Ticket),
            music_enable = SoundHelper.IsMusicEnabled(),
            sfx_enable = SoundHelper.IsSFXEnabled(),
        };

        ShowDialog(DialogIndex.SettingDialog, settingParam, onResumed);
    }

    // ─────────────────────────────────────────
    // IDialogService — navigation
    // ─────────────────────────────────────────

    void IDialogService.ReturnToMainMenu()
    {
        HideAllDialog();
        LoadSceneManager.ins.LoadSceneByName("Buffer", () =>
        {
            MainScreenViewParam param = new()
            {
                level = DataAPIController.instance.GetPlayerLevel(),
                totalGold = WalletManager.ins.Get(Currency.Gold),
                ticket = WalletManager.ins.Get(Currency.Ticket),
            };
            ViewManager.Instance.SwitchView(ViewIndex.MainScreenView, param);
        });
    }

    // ─────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────

    internal bool IsShowingDialog(DialogIndex settingDialog)
    {
        var dialog = dicDialog[settingDialog];
        return dialogShowed.Contains(dialog);
    }

    private void OnPaymentDone(PaymentResult result) { }
}