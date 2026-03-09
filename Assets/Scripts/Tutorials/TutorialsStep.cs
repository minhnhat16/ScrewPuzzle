using UnityEngine;

public enum TutorialStepType
{
    ShowMessage,        // Chỉ hiện message, không highlight
    HighlightAndClick,  // Highlight target, chờ player click
    WaitForEvent,       // Chờ game event
    AutoAdvance,        // Tự động chuyển sau delay
}

public enum TutorialHandDirection
{
    None,
    PointDown,
    PointUp,
    PointLeft,
    PointRight,
}

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    [Header("Identity")]
    public string stepId;
    public TutorialStepType stepType;

    [Header("Message")]
    [TextArea] public string message;

    [Header("Highlight")]
    public string targetKey;
    public float spotlightSize = 200f;
    public int spotlightLayer = 1;
    public bool showHand = true;
    public TutorialHandDirection handDirection = TutorialHandDirection.PointDown;

    [Header("Input")]
    public bool blockInput = true;

    [Header("Complete Condition")]
    public string completeEventKey;
    public string requiredPayload;

    [Header("Auto Advance")]
    [Tooltip("Tự động chuyển step sau N giây. 0 = không tự động.")]
    public float autoAdvanceDelay = 0f;
}