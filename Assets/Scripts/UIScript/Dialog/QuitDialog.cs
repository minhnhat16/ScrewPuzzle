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
            IngameController.ins.Resume();
        });
        _quitBtn.onClick.AddListener(() => {
            IngameController.ins.ReturnHome();
        });
    }
    public void OnDisable()
    {
        _btnContinue.onClick.RemoveAllListeners();
        _quitBtn.onClick.RemoveAllListeners();
    }
    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        SoundHelper.PlaySFX(SoundManager.SFX.Lose);
    }
}
