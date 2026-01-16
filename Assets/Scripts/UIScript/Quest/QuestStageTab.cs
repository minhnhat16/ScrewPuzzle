using System;
using UnityEngine;
using UnityEngine.UI;

public class QuestStageTab : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text label;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private GameObject unlockedUI;
    [SerializeField] private Button button;

    public int StageIndex { get; private set; }
    private Action<int> onClick;

    private bool unlocked;
    private bool selected;

    // =====================================================
    // LIFECYCLE
    // =====================================================
    private void OnEnable()
    {
        StageEvents.OnStageUnlocked += OnStageUnlocked;
    }

    private void OnDisable()
    {
        StageEvents.OnStageUnlocked -= OnStageUnlocked;
    }

    // =====================================================
    // SETUP
    // =====================================================
    public void Setup(int index, bool isSelected, bool isUnlocked, Action<int> onClickCallback)
    {
        StageIndex = index;
        onClick = onClickCallback;

        label.text = $"{index + 1}";

        unlocked = isUnlocked;
        selected = isSelected;

        ApplyState();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);
    }

    // =====================================================
    // EVENTS
    // =====================================================
    private void OnStageUnlocked(int stageId)
    {
        if (stageId != StageIndex)
            return;

        unlocked = true;
        ApplyState();
    }

    // =====================================================
    // UI STATE
    // =====================================================
    public void SetSelected(bool isOn)
    {
        selected = isOn;
        ApplyState();
    }

    private void ApplyState()
    {
        lockedIcon.SetActive(!unlocked);
        unlockedUI.SetActive(unlocked);
        // selected tab không cho click
        button.interactable = unlocked ;
    }

    // =====================================================
    // CLICK
    // =====================================================
    private void OnButtonClicked()
    {
        if (!unlocked || selected)
            return;

        onClick?.Invoke(StageIndex);
    }
}
