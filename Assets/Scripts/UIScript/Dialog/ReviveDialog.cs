using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class ReviveDialog : BaseDialog
    {
        [SerializeField] Button acceptButton;
        [SerializeField] Button denyButton;
        [SerializeField] Button closeDialogButton;
        [SerializeField] GoldDisplay goldDisplay;



        public override void Setup(DialogParam param)
        {
            ReviveDialogParam newParam = (ReviveDialogParam)param;
            if (newParam == null) return;
            int userGold = newParam.totalGold;
            acceptButton.onClick.AddListener(AcceptedWatch);
            denyButton.onClick.AddListener(DenyWatch);

            goldDisplay.SetGoldToLable(userGold);
        }

        public override void OnEndHideDialog()
        {
            acceptButton.onClick.RemoveListener(AcceptedWatch);
            denyButton.onClick.RemoveListener(DenyWatch);
        }
        private void DenyWatch()
        {
            DialogManager.Instance.HideDialog(dialogIndex, () =>
            {
                Debug.Log($"Hide this dialog {dialogIndex}");
                IngameController.Instance.OnGameOver();
            });
        }

        private void AcceptedWatch()
        {
            DialogManager.Instance.HideDialog(dialogIndex);
            
            ZenSDK.instance.ShowVideoReward(onWatch =>
            {
                if (onWatch) DialogManager.Instance.HideDialog(dialogIndex, IngameController.Instance.OnRevive);
                else IngameController.Instance.OnGameOver();
            });
        }
    }
}
