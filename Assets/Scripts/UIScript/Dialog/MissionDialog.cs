using Managers;
using UnityEngine;
using UnityEngine.UI;

public class MissionDialog : BaseDialog
{
    [SerializeField]
    private ProgressBar progressBar;

    [SerializeField]
    private Button _BtnConfirm;
    private void Awake()
    {
        BaseDialogAnim = GetComponentInChildren<BaseDialogAnimation>(true);
        progressBar = GetComponentInChildren<ProgressBar>(true);
    }
    private void OnEnable()
    {
        _BtnConfirm.onClick.AddListener(() =>
        {
            HideDialog();
        });
    }
    private void OnDisable()
    {
        _BtnConfirm?.onClick.RemoveAllListeners();  
    }
    public override void HideDialog()
    {
        DialogManager.Instance.HideDialog(this.dialogIndex, () =>
        {
            IngameController.Instance.ResumeGame();
        });
    }
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        var param = dialogParam as MissionParam;

        float progress = param.current / param.target;
        progressBar.SetProgress(progress);
    }
    public override void ShowDialog()
    {
        progressBar.UpdateProgress();
    }
}