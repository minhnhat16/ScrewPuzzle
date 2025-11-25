using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class ReviveDialog : BaseDialog
    {

        ReviveDialogParam param;
        [SerializeField] Button ticketPayButton;
        [SerializeField] Button watchButton;
        [SerializeField] Button retryButton;
        [SerializeField] Button closeDialogButton;
        [SerializeField] GoldDisplay goldDisplay;
        [SerializeField] Text txt_Title;
        [SerializeField] Text txt_watchText;
        private void OnEnable()
        {
            ticketPayButton.onClick.AddListener(TickeAccept);
            watchButton.onClick.AddListener(WatchAccept);
            retryButton.onClick.AddListener(ReplayButton);

        }
        private void OnDisable()
        {
            ticketPayButton.onClick.RemoveListener(TickeAccept);
            watchButton.onClick.RemoveListener(WatchAccept);
            retryButton.onClick.RemoveListener(ReplayButton);
        }

        private void Awake()
        {
            txt_watchText = watchButton.GetComponentInChildren<Text>();
        }
        public override void Setup(DialogParam param)
        {
            ReviveDialogParam newParam = (ReviveDialogParam)param;
            this.param = newParam;
            if (newParam == null) return;
            long userGold = newParam.totalGold;


            SetRevive(newParam.isRevive);
            goldDisplay.SetGoldToLable(userGold);
            IngameController.ins.PauseGame();
        }

        public override void OnEndHideDialog()
        {
            IngameController.ins.ResumeGame();

        }


        public void SetRevive(bool isRevive)
        {
            param.isRevive = isRevive;
            retryButton.gameObject.SetActive(isRevive);
            if (isRevive)
            {
                txt_Title.text = "So close";
                txt_watchText.text = "Continue";
            }
            else
            {
                txt_Title.text = "Add One Box";
                txt_watchText.text = "Free";
            }
        }
        private void WatchAccept()
        {

            //ZenSDK.instance.ShowVideoReward(onWatch =>
            //{
            //    if (onWatch) DialogManager.Instance.HideDialog(dialogIndex, IngameController.Instance.OnRevive);
            //    else IngameController.Instance.OnGameOver();
            //});

            DialogManager.ins.HideDialog(dialogIndex, () =>
            {
                //Debug.Log($"Hide this dialog {dialogIndex}");
                IngameController.ins.OnRevive();

                //if (!param.isRevive)
                //{
                //    IngameController.ins.OnGameOver();
                //}
                //else
                //{
                //    IngameController.ins.OnRevive();
                //}
            });
        }

        private void TickeAccept()
        {
            DialogManager.ins.HideDialog(dialogIndex);
            IngameController.ins.OnRevive();

            //ZenSDK.instance.ShowVideoReward(onWatch =>
            //{
            //    if (onWatch) DialogManager.Instance.HideDialog(dialogIndex, IngameController.Instance.OnRevive);
            //    else IngameController.Instance.OnGameOver();
            //});
        }
        private void ReplayButton()
        {
            DialogManager.ins.HideDialog(dialogIndex, () =>
            {
                //Debug.Log($"Hide this dialog {dialogIndex}");
                if (param.isRevive)
                {
                    IngameController.ins.OnGameOver();
                }
            });
        }
    }
}
