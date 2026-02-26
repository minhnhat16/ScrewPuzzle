using UIScript.Dialog;

public interface IDialogService
{
    void ShowWinDialog(WinParam param);
    void ShowLoseDialog(LoseParam param = null);
    void ShowReviveDialog(ReviveParam param = null );
    void ShowItemDialog(AddItemDialogParam param = null);
    void ReturnToMainMenu();
    void ShowPause();
    void HideAllDialog();
}