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
        // Quick init - defer individual dialog initialization
        yield return new WaitForSeconds(0.1f);

        // Pre-initialize only critical dialogs (those needed at game start)
        // Other dialogs will be lazy-initialized on first use
        foreach (var dialog in dialogList)
        {
            if (IsCriticalDialog(dialog.dialogIndex))
            {
                BaseDialog baseDialog = dialog.GetComponent<BaseDialog>();
                if (baseDialog != null)
                {
                    yield return baseDialog.Init();
                    dicDialog.Add(dialog.dialogIndex, dialog);
                }
            }
        }

        Debug.Log($"[DialogManager] Init completed. {dicDialog.Count} critical dialogs pre-initialized. Others will lazy-load.");
    }

    /// <summary>
    /// Determine if a dialog should be pre-loaded at boot or lazy-loaded on demand.
    /// </summary>
    private bool IsCriticalDialog(DialogIndex dialogIndex)
    {
        // Only pre-load dialogs that might be shown during initial gameplay
        // Adjust this list based on your game's flow
        return false; // For now, all dialogs are lazy-loaded. Modify if needed.
    }

    /// <summary>
    /// Lazy-initialize a dialog if not already initialized.
    /// </summary>
    private IEnumerator EnsureDialogInitialized(DialogIndex dialogIndex)
    {
        if (dicDialog.ContainsKey(dialogIndex))
            yield break; // Already initialized

        // Find the dialog in dialogList
        var dialog = dialogList.FirstOrDefault(d => d.dialogIndex == dialogIndex);
        if (dialog != null && !dicDialog.ContainsKey(dialogIndex))
        {
            Debug.Log($"[DialogManager] Lazy-initializing dialog: {dialogIndex}");
            BaseDialog baseDialog = dialog.GetComponent<BaseDialog>();
            if (baseDialog != null)
            {
                yield return baseDialog.Init();
                dicDialog.Add(dialogIndex, dialog);
            }
        }
    }

    // ─────────────────────────────────────────
    // Core show / hide
    // ─────────────────────────────────────────

    public void ShowDialog(DialogIndex newDialog, DialogParam dialogParam = null, Action callback = null)
    {
        StartCoroutine(ShowDialogAsync(newDialog, dialogParam, callback));
    }

    private IEnumerator ShowDialogAsync(DialogIndex newDialog, DialogParam dialogParam = null, Action callback = null)
    {
        // Ensure dialog is initialized before showing
        yield return StartCoroutine(EnsureDialogInitialized(newDialog));

        if (!dicDialog.TryGetValue(newDialog, out var dialog))
        {
            Debug.LogError($"[DialogManager] Dialog {newDialog} not found after lazy-init");
            callback?.Invoke();
            yield break;
        }

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
        LoadSceneManager.ins.LoadSceneByName("BootScene", () =>
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