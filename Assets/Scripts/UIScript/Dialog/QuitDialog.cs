using Managers;
using UnityEngine;
using UnityEngine.UI;

public class QuitDialog : BaseDialog
{

    [SerializeField]
    private Button _btnContinue;
    [SerializeField]
    private Button _quitBtn;

    public void OnEnable()
    {
        _btnContinue.onClick.AddListener(() =>
        {
            DialogManager.ins.HideAllDialog();
            IngameController.ins.ResumeGame();
        });
        _quitBtn.onClick.AddListener(() => {
            IngameController.ins.ReturnToHome(this.dialogIndex);
        });
    }
    public void OnDisable()
    {
        _btnContinue.onClick.RemoveAllListeners();
        _quitBtn.onClick.RemoveAllListeners();
    }
}
