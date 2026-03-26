using Managers;
using System;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

namespace UIScript.Dialog
{
    public class ReviveDialog : BaseDialog
    {
        [SerializeField] private Button ticketPayButton;
        [SerializeField] private Button watchButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button closeDialogButton;
        [SerializeField] private GoldDisplay goldDisplay;
        [SerializeField] private Text txt_Title;
        [SerializeField] private Text txt_watchText;

        private ReviveParam _param;

        // ─────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────

        private void Awake()
        {
            txt_watchText = watchButton.GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            ticketPayButton.onClick.AddListener(OnTicketAccept);
            watchButton.onClick.AddListener(OnWatchAccept);
            retryButton.onClick.AddListener(OnRetryButton);
            closeDialogButton.onClick.AddListener(OnCloseButton);
            DataTrigger.RegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
            DataTrigger.RegisterValueChange(DataPath.TICKET, OnTicketChanged);
            RefreshCurrencyUI();
        }

        private void OnDisable()
        {
            ticketPayButton.onClick.RemoveListener(OnTicketAccept);
            watchButton.onClick.RemoveListener(OnWatchAccept);
            retryButton.onClick.RemoveListener(OnRetryButton);
            closeDialogButton.onClick.RemoveListener(OnCloseButton);
            DataTrigger.UnRegisterValueChange(DataPath.GOLDINVENT, OnGoldChanged);
            DataTrigger.UnRegisterValueChange(DataPath.TICKET, OnTicketChanged);
        }

        // ─────────────────────────────────────────
        // Setup
        // ─────────────────────────────────────────

        public override void Setup(DialogParam param)
        {
            _param = param as ReviveParam;
            if (_param == null) return;

            ticketPayButton.interactable = _param.currentTicket > 0;
            goldDisplay.SetGoldToLable(_param.totalGold);
            SetRevive(_param.isRevive);
        }

        // ─────────────────────────────────────────
        // UI helpers
        // ─────────────────────────────────────────

        private void SetRevive(bool isRevive)
        {
            retryButton.gameObject.SetActive(isRevive);
            txt_Title.text = isRevive ? "So close" : "Add One Box";
            txt_watchText.text = isRevive ? "Continue" : "Free";
        }

        // ─────────────────────────────────────────
        // Button handlers
        // ─────────────────────────────────────────

        /// <summary>Watch ads / free → chấp nhận revive → tiếp tục Playing.</summary>
        private void OnWatchAccept()
        {
            var callback = _param?.onWatchAccepted;
            HideDialog();                  // ← BaseDialog.HideDialog() → animation → OnEndHideDialog
            callback?.Invoke();            // GameFlowService: UnlockInput + UnlockNext + TransitionTo(Playing)
        }

        /// <summary>Dùng ticket → tương tự Watch.</summary>
        private void OnTicketAccept()
        {
            if (_param == null || _param.currentTicket <= 0) return;
            var callback = _param?.onWatchAccepted;
            HideDialog();
            callback?.Invoke();
            WalletManager.ins.Spend(Currency.Ticket, 1);
        }

        /// <summary>Retry → từ chối revive → về Lose.</summary>
        private void OnRetryButton()
        {
            var callback = _param?.onDeclined;
            HideDialog();
            callback?.Invoke();            // GameFlowService: HideAllDialog + TransitionTo(Lose)
        }

        /// <summary>Close (X) → từ chối revive → về Lose.</summary>
        private void OnCloseButton()
        {
            var callback = _param?.onDeclined;
            HideDialog();
            callback?.Invoke();
        }

        public override void HideDialog()
        {
            base.HideDialog();
            DialogManager.ins.HideDialog(dialogIndex);  
        }

        // ─────────────────────────────────────────
        // BaseDialog overrides
        // ─────────────────────────────────────────

        public override void OnEndShowDialog()
        {
            base.OnEndShowDialog();
            SoundHelper.PlaySFX(SoundManager.SFX.Lose);
        }

        private void RefreshCurrencyUI()
        {
            goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
            ticketPayButton.interactable = DataAPIController.instance.GetTicket() > 0;
        }

        private void OnGoldChanged(object _)
        {
            goldDisplay.SetGoldToLable(DataAPIController.instance.GetGold());
        }

        private void OnTicketChanged(object _)
        {
            ticketPayButton.interactable = DataAPIController.instance.GetTicket() > 0;
        }
    }   
}
