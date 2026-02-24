using ConfigFile;
using Ingame.Screw;
using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager ins;

    [SerializeField] private TutorialConfig config;
    private int currentIndex;

    private TutorialStep Current
    {
        get
        {
            var list = config.GetAllRecord();
            return currentIndex >= 0 && currentIndex < list.Count
                ? list[currentIndex]
                : null;
        }
    }


    private void Awake()
    {
        ins = this;
    }

    private void Start()
    {
        currentIndex = 0;
    }

    void PlayStep()
    {
        TutorialUI.ins.HideAll();

        if (Current == null)
        {
            Debug.Log("[Tutorial] DONE");
            return;
        }

        Debug.Log($"[Tutorial] Play step {Current.stepId}");

        // 1. MESSAGE
        if (!string.IsNullOrEmpty(Current.message))
        {
            TutorialUI.ins.ShowMessage(Current.message);
            //StartCoroutine(TutorialUI.ins.HideAllAfter(5f));

        }
        if(Current.completeEventKey == "Level.Completed")
        {
            StartCoroutine(TutorialUI.ins.HideAllAfter(5f));

        }
        // 2. UI theo StepType
        switch (Current.stepType)
        {
            case TutorialStepType.ShowMessage:
                TutorialUI.ins.BlockInput(false);
                break;

            case TutorialStepType.HighlightAndClick:
                TutorialUI.ins.BlockInput(Current.blockInput);
                var target = TutorialTargetRegistry.Get(Current.targetKey);
                TutorialUI.ins.HighlightTarget(target);

                target.TryGetComponent<ScrewController>(out var s);
                if (s != null) s.IsClicked = false;
                break;

            case TutorialStepType.WaitForEvent:
                TutorialUI.ins.BlockInput(true);
                target = TutorialTargetRegistry.Get(Current.targetKey);
                TutorialUI.ins.HighlightTarget(target);
                break;
        }

        // 3. SUBSCRIBE EVENT
        if (!string.IsNullOrEmpty(Current.completeEventKey))
        {
            TutorialEventBus.Subscribe(Current.completeEventKey, OnStepEvent);
        }
    }
    public void OnStepEvent(object payload)
    {

        Debug.Log($"[Tutorial] Event received for step {Current.stepId} with payload: {payload}");
        if (!string.IsNullOrEmpty(Current.requiredPayload))
        {
            Debug.Log($" OnStepEvent  {payload == null || payload.ToString() != Current.requiredPayload}");
            if (payload == null || payload.ToString() != Current.requiredPayload)
                return;
        }

        CompleteStep();
    }

    void CompleteStep()
    {
        if (!string.IsNullOrEmpty(Current.completeEventKey))
            TutorialEventBus.Unsubscribe(Current.completeEventKey, OnStepEvent);

        currentIndex++;

        if (currentIndex >= config.GetAllRecord().Count)
        {
            OnTutorialCompleted();
            return;
        }

        PlayStep();
    }

    public void StartTutorial()
    {
        Debug.Log("[Tutorial] START");
        currentIndex = 0;
        PlayStep();
    }

    void OnTutorialCompleted()
    {
        Debug.Log("[Tutorial] COMPLETED");

        PlayerPrefs.SetInt("FIRST_TIME", 0);
        PlayerPrefs.Save();

    }
}
