using UnityEngine;

public enum TutorialStepType
{
    ShowMessage,
    HighlightAndClick,
    WaitForEvent
}
[CreateAssetMenu(menuName = "Tutorial/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    public string stepId;
    public TutorialStepType stepType;

    [TextArea] public string message;

    public string targetKey;
    public bool blockInput = true;

    // Complete condition
    public string completeEventKey;
    public string requiredPayload;
}