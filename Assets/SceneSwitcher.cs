using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Array of scene names to switch between
    [SerializeField] private string[] sceneNames;

    private int currentSceneIndex = 0;

    private void OnEnable()
    {
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        // Check if Ctrl is being held down and Tab is pressed
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchScene();
            }
        }
    }

    private void SwitchScene()
    {
        // Get the name of the next scene to load
        string nextSceneName = sceneNames[currentSceneIndex];

        // Load the next scene by name
        SceneManager.LoadScene(nextSceneName);

        // Update the current scene index, looping back to the first scene
        currentSceneIndex = (currentSceneIndex + 1) % sceneNames.Length;

        Debug.Log($"Switched to scene: {nextSceneName}");
    }
}