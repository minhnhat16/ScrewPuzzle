using System.Collections;
using UnityEngine;

public class GamePlayCanvas : MonoBehaviour
{
    public static GamePlayCanvas Instance;
    private Canvas canvas;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        canvas = GetComponent<Canvas>();

    }
    // Start is called before the first frame update
    public IEnumerator Start()
    {
        yield return new WaitUntil(() => CameraMain.instance.main != null);
        canvas.worldCamera = CameraMain.instance.main;
    }

}
