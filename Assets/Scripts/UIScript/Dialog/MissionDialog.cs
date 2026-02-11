using Coffee.UIExtensions;
using DG.Tweening;
using JetBrains.Annotations;
using Managers;
using System;
using System.DataBase;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MissionDialog : BaseDialog
{
    [SerializeField]
    private ProgressBar progressBar;
    [SerializeField]
    private Text TextSpecial;
    [SerializeField]
    private Button _BtnConfirm;
    [SerializeField]
    private RectTransform supItem;
    [SerializeField]
    private Button _BtnSup;
    [SerializeField]
    private RectTransform rewardItem;
    [SerializeField]
    private GoldDisplay ticketDisplay;
    [SerializeField]
    private GoldDisplay goldDisplay;
    private MissionParam param;


    Vector3 rewardStartPos;
    Vector3 rewardStartScale;
    CanvasGroup rewardCg;
    private UnityEvent ticketClaim;

    private void Awake()
    {
        BaseDialogAnim = GetComponentInChildren<BaseDialogAnimation>(true);
        progressBar = GetComponentInChildren<ProgressBar>(true);

        rewardStartPos = rewardItem.position;
        rewardStartScale = rewardItem.localScale;

        rewardCg = rewardItem.GetComponent<CanvasGroup>();
        if (rewardCg == null)
            rewardCg = rewardItem.gameObject.AddComponent<CanvasGroup>();
    }
    private void OnEnable()
    {
        _BtnSup.onClick.AddListener(() =>
        {
            ClaimItem();
        });
        _BtnConfirm.onClick.AddListener(() =>
        {
            HideDialog();
        });

        DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, (s) =>
        {
         
            var gold = DataAPIController.instance.GetGold();
            goldDisplay.SetGoldToLable(gold);

        });
        DataTrigger.RegisterValueChange(DataPath.TICKET, (s) =>
        {
            var ticket = DataAPIController.instance.GetTicket();
            ticketDisplay.SetGoldToLable(ticket);
        });

        ticketClaim = GameManager.instance.specialClaim;
    }


    private void OnDisable()
    {   
        _BtnConfirm?.onClick.RemoveAllListeners();

        DataTrigger.UnRegisterValueChange(DataPath.GOLDINVENT, (s) =>
        {

            var gold = DataAPIController.instance.GetGold();
            goldDisplay.SetGoldToLable(gold);

        });
        DataTrigger.UnRegisterValueChange(DataPath.TICKET, (s) =>
        {
            var ticket = DataAPIController.instance.GetTicket();
            ticketDisplay.SetGoldToLable(ticket);
        });
    }
    public override void HideDialog()
    {
        DialogManager.ins.HideDialog(this.dialogIndex, () =>
        {
            IngameController.ins.ResumeGame();
        });
    }
    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
         param = dialogParam as MissionParam;

        float progress = param.current / param.target;
        progressBar.SetProgress(progress);

        TextSpecial.text  = $"{param.current}/{param.target}";
        supItem.gameObject.SetActive(false);
        ticketDisplay.SetGoldToLable(param.totalTicket);
        goldDisplay.SetGoldToLable(param.totalGold);
        
    }
    public override void OnStartShowDialog()
    {
        Debug.Log("Show Mission Dialog");
        progressBar.UpdateProgress();
        CalculateSpecial();
    }
    public override void OnEndHideDialog()
    {
        ResetRewardItem();
    }

    private void ShowSupItem()
    {
        supItem.gameObject.SetActive(true);
        _BtnSup.interactable = false;
        var canvasGroup = supItem.GetComponent<CanvasGroup>();
        canvasGroup.DOFade(1, 0.5f).SetEase(Ease.InBack).OnComplete(()=> _BtnSup.interactable = true );
    }


    private void ClaimItem()
    {
        CanvasGroup cg = rewardItem.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = rewardItem.gameObject.AddComponent<CanvasGroup>();

        rewardItem.localScale = Vector3.one;

        DG.Tweening.Sequence seq = DOTween.Sequence();

        seq.Append(
            rewardItem.DOMove(ticketDisplay.transform.position, 0.6f)
                .SetEase(Ease.InOutCubic)
        );

        seq.Join(
            rewardItem.DOScale(0.6f, 0.6f)
        );

        seq.AppendCallback(() =>
        {
            ticketDisplay.transform
                .DOPunchScale(Vector3.one * 0.3f, 0.25f, 10, 0.9f);

            cg.DOFade(0f, 0.15f);

            supItem.gameObject.SetActive(false);



            ticketClaim?.Invoke();

        });
    }
    public void CalculateSpecial()
    {
        
        TextSpecial.text = $"{param.current}/{param.target}";

        float progress = param.target > 0
            ? (float) param.current/ param.target
            : 0f;

        progress = Mathf.Clamp01(progress);
        progressBar.SetProgress(progress);



        Debug.Log("Progress: " + progress);
        if (progress >= 1f)
        {
            ShowSupItem();
        }
    }

    private void ResetRewardItem()
    {
        // Kill tween cũ
        rewardItem.DOKill();
        rewardCg.DOKill();

        // Reset transform
        rewardItem.position = rewardStartPos;
        rewardItem.localScale = rewardStartScale;

        // Reset alpha
        rewardCg.alpha = 1f;
        ticketDisplay.transform.localScale = Vector3.one;

        // Hiện lại object
        supItem.gameObject.SetActive(true);
    }

}