using Ingame;
using Managers;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

public class LoseDialog : BaseDialog
{
    [SerializeField] private Button btn_retry;
    [SerializeField] private Button btn_Watch;

    private LoseParam _param;

    // ─────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────

    private void OnEnable()
    {
        btn_retry.onClick.AddListener(OnRetryClicked);
        btn_Watch.onClick.AddListener(OnWatchClicked);
    }

    private void OnDisable()
    {
        btn_retry.onClick.RemoveListener(OnRetryClicked);
        btn_Watch.onClick.RemoveListener(OnWatchClicked);
    }

    // ─────────────────────────────────────────
    // Setup
    // ─────────────────────────────────────────

    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        _param = dialogParam as LoseParam;

        // Ẩn/hiện nút Watch tuỳ theo có ads không
        if (btn_Watch != null)
            btn_Watch.gameObject.SetActive(_param?.isAdAvailable ?? true);

        SoundHelper.PlaySFX(SoundManager.SFX.Lose);
    }

    // ─────────────────────────────────────────
    // Button handlers
    // ─────────────────────────────────────────

    /// <summary>
    /// Watch ads → Revive (về Playing) → dùng Magnet xóa screw trong ArrayScrew.
    /// Thứ tự bắt buộc: Revive trước (state = Playing), rồi mới InvokeItem.
    /// </summary>
    private void OnWatchClicked()
    {
        string playerLevel = DataAPIController.instance.GetPlayerLevel().ToString();
        ZenSDK.instance.TrackRewardOfferAccept(GameConstants.REVIVE_VIDEO_KEY, playerLevel, ZenSDK.instance.IsNetworkConnected().ToString());

        ZenSDK.instance.ShowVideoReward((success) =>
        {
            DialogManager.ins.HideDialog(dialogIndex, () =>
            {
                // Callback chạy sau khi animation hide xong
                // 1. Về Playing
                IngameController.ins.ReviveDirectly();

                // 2. Invoke Magnet — state đã là Playing ✅
                Vector3 magnetPos = ArrayScrew.ins.GetLastHoldPosition();
                IngameController.ins.OnItemInvoke.Invoke(ItemType.Magnet, magnetPos);
            });
        }, GameConstants.REVIVE_VIDEO_KEY, playerLevel);

    }

    /// <summary>Retry → restart level hiện tại.</summary>
    private void OnRetryClicked()
    {

        ZenSDK.instance.ShowFullScreen(GameConstants.QUIT_STRING_KEY, DataAPIController.instance.GetPlayerLevel().ToString());

        HideDialog();

        int currentLevel = LevelManager.ins.CurrentLevelId;
        IngameController.ins.RestartLevel(currentLevel);
    }

    // ─────────────────────────────────────────
    // BaseDialog overrides
    // ─────────────────────────────────────────
    public override void HideDialog()
    {
        base.HideDialog();
    }
    public override void OnEndHideDialog()
    {
        // State transition do button handler xử lý — không tự resume ở đây
    }
}