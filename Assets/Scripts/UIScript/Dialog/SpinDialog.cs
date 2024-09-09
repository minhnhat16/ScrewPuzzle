using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpinDialog : BaseDialog
{
    [SerializeField] private Button spinBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Text countDown_lb;
    [SerializeField] private SpinCircle circle;
    [SerializeField] public UnityEvent<bool> onSpinDone = new();
    private bool isCoutingTime;

    private void OnEnable()
    {
        onSpinDone = circle.spinnedEvent;
        onSpinDone.AddListener(OnSpinDone);
    }
    public override void OnInit()
    {
        //Debug.Log("ON INIT  DIALOG");
        circle.Init();
        circle.RadialLayout.CalculateRadial();
    }
 
    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        circle.SpawnObjectsInCircle();
        spinBtn.onClick?.AddListener(SpinCircle);
        exitBtn.onClick?.AddListener(() =>
        {
            //Debug.Log("ONCLICK EXIT");
            DialogManager.Instance.HideDialog(dialogIndex);
            DialogManager.Instance.ShowDialog(DialogIndex.LableChooseDialog);

        });

        if (IsNewDay())
        {
            spinBtn.interactable = true;
            isCoutingTime = false;
            countDown_lb.gameObject.SetActive(false);   
        }
        else
        {
            isCoutingTime = true;
            spinBtn.interactable = false;
            circle.SecondBG.gameObject.SetActive(true);
            ShowTimeCounter();
        }
    }

    public override void OnStartHideDialog()
    {
        base.OnEndHideDialog();
        if (ViewManager.Instance.currentView.viewIndex != ViewIndex.MainScreenView) return;
        var main = (MainScreenView)ViewManager.Instance.currentView;
        main.SetLevelPanelIs(true);
    }
    public void SpinCircle()
    {
        circle.SpinningCircle();
    }
    private bool IsNewDay()
    {
        DateTime lastSpinData = DataAPIController.instance.GetSpinTimeData();
        TimeSpan timeDifference = DateTime.Now - lastSpinData;
        bool isNewDay = timeDifference.TotalHours > 24;
        //Debug.Log($"IS NEW DAY {isNewDay}");
        return isNewDay;
    }
    public void OnSpinDone(bool onDoneSpin)
    {
        if (onDoneSpin)
        {
            try 
            {
                var anim = GetComponentInChildren<SpinAnim>();
                if (anim != null)
                {
                    // Assuming anim.OnSpinDone takes a bool parameter
                    anim.OnSpinDone(null); // Pass true or appropriate value
                    isCoutingTime = true;
                    spinBtn.interactable = false;
                    circle.SecondBG.gameObject.SetActive(true);
                    ShowTimeCounter();
                }
            }
            catch (Exception e)
            {
                throw e;
                ///Debug.LogError($"Exception in OnSpinDone: {e}");
            }
        }
    }
    private void ShowTimeCounter()
    {
        if (!isCoutingTime) return;

        DateTime lastSpinData = DataAPIController.instance.GetSpinTimeData();
        TimeSpan remaining = DayTimeController.instance.GetRemainingTime(lastSpinData);

        if (remaining > TimeSpan.Zero)
        {
            countDown_lb.gameObject.SetActive(true);
            StartCoroutine(Counter(remaining));
        }
        else
        {
            countDown_lb.gameObject.SetActive(false);
        }
    }

    IEnumerator Counter(TimeSpan remaining)
    {
        while (remaining > TimeSpan.Zero)
        {
            countDown_lb.text = DayTimeController.instance.GetCountdownString(remaining);
            countDown_lb.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            remaining = remaining.Subtract(TimeSpan.FromSeconds(1));
        }

        countDown_lb.gameObject.SetActive(false);
    }
}
