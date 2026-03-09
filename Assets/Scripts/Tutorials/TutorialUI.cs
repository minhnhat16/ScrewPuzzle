using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI ins;

    public CanvasGroup mainGroup;

    [Header("Refs")]
    public CanvasGroup blocker;
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

    /// <summary>
    /// Highlight target với config từ TutorialStep — không hardcode tên object.
    /// </summary>
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
    }

    public IEnumerator HideAllAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        HideAll();
    }
}