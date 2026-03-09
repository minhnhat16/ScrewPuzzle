using System;

public interface IDialogService
{
    // ─── Kết thúc level ───────────────────────────────────────────
    // Không auto-resume — flow do button trong dialog quyết định
    void ShowWinDialog(WinParam param);
    void ShowLoseDialog(LoseParam param = null);

    // ─── Interrupt gameplay (auto-resume khi đóng) ────────────────
    // onResumed được gọi khi dialog đóng → IngameController tự resume state
    void ShowReviveDialog(ReviveParam param = null, Action onDeclined = null);
    void ShowItemDialog(AddItemDialogParam param = null, Action onClosed = null);
    void ShowPause(SettingParam param = null, Action onResumed = null);

    // ─── Navigation ───────────────────────────────────────────────
    void ReturnToMainMenu();
    void HideAllDialog();
}