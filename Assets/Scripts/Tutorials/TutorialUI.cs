using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

    public void ShowMessage(string msg)
    {
        Show();
        Debug.Log($"[TutorialUI] Show message: {msg}");
        message.Show(msg);

    }

    public void HighlightTarget(Transform target)
    {
        if (target == null)
        {
            Debug.Log("Target null hide alll");
            StartCoroutine(HideAllAfter(4f));
            return;
        }
        Show();
        if (target.name.Contains("Box"))
        {
            Debug.Log("Show target on box");
            spotlight.Show(target, 300, 1, true);
            DOVirtual.DelayedCall(2f, () =>
            {
                TutorialEventBus.Emit(
                 "Screw.InsertedToBox",
                 "red");
            });
            return;
        }
        if (target.name.Contains("Hold"))
        {
            Debug.Log("Show target on array");
            spotlight.Show(target, 300, 1, false);
            DOVirtual.DelayedCall(2f, () =>
            {
                TutorialEventBus.Emit(
                 "Screw.InsertedToQueue",
                 "array");
                hand.Hide();
            });
            return;
        }
        spotlight.Show(target);
        hand.ShowAtScreenPos(target);
    }

    public IEnumerator HideAllAfter(float sec)
    {
        yield return new WaitForSeconds(sec);
        Debug.Log("Hide all after " + sec);
        HideAll();
    }
    public void BlockInput(bool block)
    {
        Show();
        Debug.Log($"[TutorialUI] Block input: {block}");
        blocker.alpha = block ? 1f : 0f;
        blocker.blocksRaycasts = block;
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
        message.Hide();
        spotlight.Hide();
        hand.Hide();
        BlockInput(false);
    }
}

