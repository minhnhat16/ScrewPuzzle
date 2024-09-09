using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class OutOffCardDialog : BaseDialog
{
    [SerializeField] private OutOffCardParam param;
    [SerializeField] private DateTime target;
    [SerializeField] private Button watchBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Text timeCouter_lb;
    [SerializeField] private Text totalCardAdd_lb;

    [SerializeField] private int bonusCard;
    private bool isCountingTime;
    private void OnEnable()
    {
        watchBtn.onClick.AddListener(AccpetedWatch);
        exitBtn.onClick.AddListener(OnExit);
    }
    private void OnDisable()
    {
        watchBtn.onClick.RemoveListener(AccpetedWatch);
        exitBtn.onClick.RemoveListener(OnExit);
    }

    private void Start()
    {
        param = new OutOffCardParam();
        bonusCard = ZenSDK.instance.GetConfigInt("bonusCard", 500);
    }
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        param = (OutOffCardParam)dialogParam;
        target = param.targetTime;
    }
    public override void OnStartShowDialog()
    {
        base.OnStartShowDialog();
        TimeCounterLBUpdate();
        TotalAddCard(totalCardAdd_lb);
    }
    public override void OnEndHideDialog()
    {
        base.OnEndHideDialog();
        Player.Instance.isAnimPlaying = false;
        Player.Instance.isDealBtnActive = false;
        ZenSDK.instance.ShowFullScreen();
    }
    private void TotalAddCard(Text lb)
    {
        lb.text = $"+{bonusCard}";
    }
    private void TimeCounterLBUpdate()
    {
        StartCoroutine(UpdateTime(target, timeCouter_lb));
    }
    public void OnExit()
    {
        Debug.Log("Exit button on click");
        //SoundManager.instance.PlaySFX(SoundManager.SFX.UIPopSFX);
        DialogManager.Instance.HideDialog(this.dialogIndex);
        Debug.Log("Exit button done");
        GamePlayView gameplay = ViewManager.Instance.currentView as GamePlayView;
        gameplay.DealBtn.TapBtn.interactable = true;
        Player.Instance.isAnimPlaying = false;
    }
    private void AccpetedWatch()
    {
        //ZenSDK.instance.ShowVideoReward((onWatched) =>
        //{
        bool onWatched = true;
        if (onWatched) DataAPIController.instance.SetCurrrentCardPool(bonusCard, () =>
        {
            GamePlayView view = ViewManager.Instance.currentView as GamePlayView;
            view.DealBtn.CurrentCardCounter = bonusCard;
            view.DealBtn.TapBtn.interactable = true;
            DialogManager.Instance.HideDialog(dialogIndex, () =>
            {
                Player.Instance.isDealBtnActive = true;
            });
        });
        else return;
        //});
    }
    IEnumerator UpdateTime(DateTime targetTime, Text label)
    {
        //Debug.Log("UPDATE TIME" + targetTime);
        while (true)
        {
            // Tính toán thời gian còn lại
            TimeSpan timeRemaining = targetTime - DateTime.Now;
            //lb_timeCounter.text = $"500 cards in {minutes}:{seconds}";
            label.text = string.Format("{0:00}:{1:00}:{2:00}", timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds);
            yield return null;
        }
    }

}
