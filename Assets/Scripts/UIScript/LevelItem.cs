using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour
{
    [SerializeField] private int idLevel;
    [SerializeField] private bool isCompleted;
    [SerializeField] private int levelStars;

    //UI COMPONENT
    [SerializeField] private Image imageIcon;
    [SerializeField] private Text textLevel;
    [SerializeField] private Button button;

    public int IDLevel
    {
        get => idLevel;
        set => idLevel = value;
    }

    public bool IsCompleted
    {
        get => isCompleted;
        set => isCompleted = value;
    }

    public int LevelStart
    {
        get => levelStars;
        set => levelStars = value;
    }

    public void Setup(int idLevel, bool isCompleted, int levelStars)
    {
        this.idLevel = idLevel;
        this.isCompleted = isCompleted;
        this.levelStars = levelStars;
        
        button.onClick.AddListener(() => OnLevelButtonClick(idLevel));
    }
    private void OnLevelButtonClick(int id)
    {
        Debug.Log("Level " + id + " clicked!");
        HandleLevelClicked(id);
    }

    // Custom logic when level is clicked
    private void HandleLevelClicked(int id)
    {
        // Perform actions when the level button is clicked (e.g., load the level)
        Debug.Log("Handle logic for level: " + id);
    }
}