using UnityEngine;
using UnityEngine.UI;

public class TutorialMessage : MonoBehaviour
{
    [SerializeField] private Text text;

    public void Show(string msg)
    {
        gameObject.SetActive(true);
        text.text = msg;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
