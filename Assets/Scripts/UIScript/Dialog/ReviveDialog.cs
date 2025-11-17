using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class ReviveDialog : BaseDialog
    {

        ReviveDialogParam param;
        [SerializeField] Button acceptButton;
        [SerializeField] Button denyButton;
        [SerializeField] Button closeDialogButton;
        [SerializeField] GoldDisplay goldDisplay;
        private void OnEnable()
        {
            acceptButton.onClick.AddListener(AcceptedWatch);
            denyButton.onClick.AddListener(DenyWatch);
        }
        private void OnDisable()
        {
            acceptButton.onClick.RemoveListener(AcceptedWatch);
            denyButton.onClick.RemoveListener(DenyWatch);
        }
        public override void Setup(DialogParam param)
        {
            ReviveDialogParam newParam = (ReviveDialogParam)param;
            this.param = newParam;
            if (newParam == null) return;
            int userGold = newParam.totalGold;

            goldDisplay.SetGoldToLable(userGold);
            IngameController.Instance.PauseGame();
        }

        public override void OnEndHideDialog()
        {
            IngameController.Instance.ResumeGame();
            acceptButton.onClick.RemoveListener(AcceptedWatch);
            denyButton.onClick.RemoveListener(DenyWatch);
        }
        private void DenyWatch()
        {
            DialogManager.Instance.HideDialog(dialogIndex, () =>
            {
                //Debug.Log($"Hide this dialog {dialogIndex}");
                if (param.isRevive)
                {
                    IngameController.Instance.OnGameOver();
                }
            });
        }

        private void AcceptedWatch()
        {
            DialogManager.Instance.HideDialog(dialogIndex);
            IngameController.Instance.OnRevive();

            //ZenSDK.instance.ShowVideoReward(onWatch =>
            //{
            //    if (onWatch) DialogManager.Instance.HideDialog(dialogIndex, IngameController.Instance.OnRevive);
            //    else IngameController.Instance.OnGameOver();
            //});
        }
    }
}
