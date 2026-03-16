using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI ins;

    public CanvasGroup mainGroup;

    [Header("Refs")]
    [Tooltip("Block input theo từng step (blockInput = true)")]
    public CanvasGroup blocker;

    [Tooltip("Block toàn bộ raycast trong suốt tutorial — chừa lỗ spotlight")]
    public CanvasGroup tutorialBlocker;   // ← assign trong Inspector, alpha=0 raycast=false mặc định

    public TutorialSpotlight spotlight;
    public TutorialHand hand;
    public TutorialMessage message;

    private void Awake()
    {
        ins = this;
        HideAll();
    }

    // ─────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────

    public void ShowMessage(string msg)
    {
        Show();
        message.Show(msg);
        Debug.Log($"[TutorialUI] ShowMessage: {msg}");
    }

    public void HighlightTarget(
        Transform target,
        float spotlightSize = 200f,
        int spotlightLayer = 1,
        bool showHand = true,
        TutorialHandDirection handDir = TutorialHandDirection.PointDown)
    {
        if (target == null)
        {
            Debug.LogWarning("[TutorialUI] HighlightTarget: target null.");
            return;
        }

        Show();
        spotlight.Show(target, spotlightSize, spotlightLayer, showHand);

        if (showHand)
            hand.ShowAtScreenPos(target, handDir);
        else
            hand.Hide();
    }

    public void BlockInput(bool block)
    {
        Show();
        blocker.alpha = block ? 1f : 0f;
        blocker.blocksRaycasts = block;
        Debug.Log($"[TutorialUI] BlockInput: {block}");
    }

    /// <summary>
    /// Bật permanent blocker — chặn mọi raycast ngoài vùng spotlight
    /// trong suốt tutorial. Gọi một lần khi tutorial bắt đầu.
    /// </summary>
    public void EnableTutorialBlocker()
    {
        if (tutorialBlocker == null) return;
        tutorialBlocker.alpha = 1f;
        tutorialBlocker.blocksRaycasts = true;
        tutorialBlocker.interactable = false; // không cần tương tác, chỉ chặn
        Debug.Log("[TutorialUI] TutorialBlocker ENABLED");
    }

    /// <summary>
    /// Tắt permanent blocker — gọi khi tutorial kết thúc hoàn toàn.
    /// </summary>
    public void DisableTutorialBlocker()
    {
        if (tutorialBlocker == null) return;
        tutorialBlocker.alpha = 0f;
        tutorialBlocker.blocksRaycasts = false;
        Debug.Log("[TutorialUI] TutorialBlocker DISABLED");
    }

    public void Show()
    {
        mainGroup.alpha = 1f;
        mainGroup.interactable = true;
        mainGroup.blocksRaycasts = false;
    }

    public void HideAll()
    {
        mainGroup.alpha = 0f;
        mainGroup.interactable = false;
        mainGroup.blocksRaycasts = false;
        message.Hide();
        spotlight.Hide();
        hand.Hide();
        blocker.alpha = 0f;
        blocker.blocksRaycasts = false;
        DisableTutorialBlocker();
    }

    public IEnumerator HideAllAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        HideAll();
    }
}