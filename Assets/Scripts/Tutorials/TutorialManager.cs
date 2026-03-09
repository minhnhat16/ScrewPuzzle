using ConfigFile;
using Ingame.Screw;
using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager ins;

    [SerializeField] private TutorialConfig config;

    private int _currentIndex;
    private Coroutine _autoAdvanceCoroutine;

    private TutorialStep Current
    {
        get
        {
            var list = config.GetAllRecord();
            return _currentIndex >= 0 && _currentIndex < list.Count
                ? list[_currentIndex]
                : null;
        }
    }

    private void Awake() => ins = this;

    // ─────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────

    public void StartTutorial()
    {
        Debug.Log("[Tutorial] START");
        _currentIndex = 0;
        PlayStep();
    }

    /// <summary>
    /// Force start từ step cụ thể — dùng để debug trong Editor.
    /// </summary>
    public void StartFromStep(int index)
    {
        _currentIndex = Mathf.Clamp(index, 0, config.GetAllRecord().Count - 1);
        Debug.Log($"[Tutorial] Force start from step index {_currentIndex}");
        PlayStep();
    }

    /// <summary>
    /// Skip step hiện tại — dùng để debug.
    /// </summary>
    public void SkipCurrentStep()
    {
        Debug.Log($"[Tutorial] Skip step '{Current?.stepId}'");
        CompleteStep();
    }

    public void OnStepEvent(object payload)
    {
        if (Current == null) return;

        Debug.Log($"[Tutorial] Event for step '{Current.stepId}' payload={payload}");

        if (!string.IsNullOrEmpty(Current.requiredPayload))
        {
            if (payload == null || payload.ToString() != Current.requiredPayload)
                return;
        }

        CompleteStep();
    }

    // ─────────────────────────────────────────
    // Internal
    // ─────────────────────────────────────────

    private void PlayStep()
    {
        TutorialUI.ins.HideAll();
        StopAutoAdvance();

        if (Current == null)
        {
            Debug.Log("[Tutorial] DONE — no more steps");
            return;
        }

        Debug.Log($"[Tutorial] ▶ Step [{_currentIndex}] '{Current.stepId}' type={Current.stepType}");

        if (!string.IsNullOrEmpty(Current.message))
            TutorialUI.ins.ShowMessage(Current.message);

        switch (Current.stepType)
        {
            case TutorialStepType.ShowMessage:
                TutorialUI.ins.BlockInput(false);
                break;

            case TutorialStepType.HighlightAndClick:
                TutorialUI.ins.BlockInput(Current.blockInput);
                HighlightCurrentTarget();
                break;

            case TutorialStepType.WaitForEvent:
                TutorialUI.ins.BlockInput(Current.blockInput);
                HighlightCurrentTarget();
                break;

            case TutorialStepType.AutoAdvance:
                TutorialUI.ins.BlockInput(false);
                break;
        }

        if (!string.IsNullOrEmpty(Current.completeEventKey))
            TutorialEventBus.Subscribe(Current.completeEventKey, OnStepEvent);

        if (Current.autoAdvanceDelay > 0f)
            _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine(Current.autoAdvanceDelay));
    }

    private void HighlightCurrentTarget()
    {
        if (string.IsNullOrEmpty(Current.targetKey)) return;

        var target = TutorialTargetRegistry.Get(Current.targetKey);
        if (target == null)
        {
            Debug.LogWarning($"[Tutorial] Target not found: '{Current.targetKey}' — check TutorialTargetRegistry");
            return;
        }

        if (target.TryGetComponent<ScrewController>(out var screw))
            screw.IsClicked = false;

        TutorialUI.ins.HighlightTarget(
            target,
            Current.spotlightSize,
            Current.spotlightLayer,
            Current.showHand,
            Current.handDirection
        );
    }

    private void CompleteStep()
    {
        if (Current == null) return;

        if (!string.IsNullOrEmpty(Current.completeEventKey))
            TutorialEventBus.Unsubscribe(Current.completeEventKey, OnStepEvent);

        StopAutoAdvance();
        _currentIndex++;

        if (_currentIndex >= config.GetAllRecord().Count)
        {
            OnTutorialCompleted();
            return;
        }

        PlayStep();
    }

    private IEnumerator AutoAdvanceCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteStep();
    }

    private void StopAutoAdvance()
    {
        if (_autoAdvanceCoroutine != null)
        {
            StopCoroutine(_autoAdvanceCoroutine);
            _autoAdvanceCoroutine = null;
        }
    }

    private void OnTutorialCompleted()
    {
        Debug.Log("[Tutorial] ✅ COMPLETED");
        TutorialUI.ins.HideAll();
        PlayerPrefs.SetInt("FIRST_TIME", 0);
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    // ─────────────────────────────────────────
    // Editor debug — hiện trong Inspector
    // ─────────────────────────────────────────

    [Header("── Debug (Editor Only) ──")]
    [SerializeField] private int _debugStartIndex = 0;

    [ContextMenu("▶ Start Tutorial")]
    private void Debug_StartTutorial() => StartTutorial();

    [ContextMenu("▶ Start From Index")]
    private void Debug_StartFromIndex() => StartFromStep(_debugStartIndex);

    [ContextMenu("⏭ Skip Current Step")]
    private void Debug_SkipStep() => SkipCurrentStep();

    [ContextMenu("🔄 Reset FIRST_TIME")]
    private void Debug_ResetFirstTime()
    {
        PlayerPrefs.SetInt("FIRST_TIME", 1);
        PlayerPrefs.Save();
        Debug.Log("[Tutorial] FIRST_TIME reset → 1");
    }
#endif
}