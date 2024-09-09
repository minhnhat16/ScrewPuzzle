using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RateDialog : BaseDialog
{
    [SerializeField] Button rateBtn;
    [SerializeField] Button remindBtn;

    [HideInInspector]
    public UnityEvent<bool> rateEvent = new UnityEvent<bool>();

    public void CloseButton()
    {
        DialogManager.Instance.HideDialog(dialogIndex, () =>
        {
            //Debug.Log("CloseButton");
            DialogManager.Instance.ShowDialog(DialogIndex.LableChooseDialog);
        });
    }
    public override void OnStartHideDialog()
    {
        base.OnStartHideDialog();
        if (ViewManager.Instance.currentView.viewIndex != ViewIndex.MainScreenView) return;
        var main = (MainScreenView)ViewManager.Instance.currentView;
        main.SetLevelPanelIs(false);
    }
    public void ReMindLaterBtn()
    {
        DialogManager.Instance.HideDialog(this.dialogIndex, () =>
        {
            //Debug.Log("ReMindLaterBtn");
        });
    }
    public void OnRateEvents(bool isRate)
    {
        rateBtn.gameObject.SetActive(isRate);
        remindBtn.gameObject.SetActive(!isRate);
    }
    public void RateButton()
    {
        //Debug.Log("RateButton");
    }
}
