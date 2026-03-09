#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        LevelManager levelManager = LevelManager.ins;

        // Check if we're in Play Mode or Edit Mode
        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Load Level in Play Mode"))
            {
                Debug.Log("Button on click");
                // Call your load level function or any other function in Play Mode
                levelManager.LoadLevel(levelManager.CurrentLevelId);
            }

            if (GUILayout.Button("Reset Level in Play Mode"))
            {
                levelManager.OnReset();
            }
        }
        else
        {
            // This button will only work in Edit Mode
            if (GUILayout.Button("Load Level in Edit Mode"))
            {
                // Call your load level function or any other function in Edit Mode
                levelManager.LoadLevel(levelManager.CurrentLevelId);
            }
        }
    }
}
#endif
