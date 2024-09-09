using UnityEngine;

public class FPS_Control : MonoBehaviour
{

    float deltaTime = 0f;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0f, 0f, 0f, 1f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.}) fps", msec, fps);
        GUI.Label(rect, text, style);
    }

}
