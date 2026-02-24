public interface IDialogService
{
    void ShowWinDialog(int levelId);
    void ShowLoseDialog();
    void ShowReviveDialog();
    void ShowItemDialogd(ItemType item);
    void ReturnToMainMenu();
}