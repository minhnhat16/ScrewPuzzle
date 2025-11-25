using UnityEngine;
using UnityEngine.UI;

public class MissionItemUI : MonoBehaviour
{
    public Image icon;
    public Text titleText;
    public Text progressText;
    public Image progressFill;
    public Button actionButton;
    public GameObject doneIcon;

    private MissionConfigRecord missionData;

    public void Setup(MissionConfigRecord data, int currentProgress)
    {
        missionData = data;

        titleText.text = data.Description;
        progressText.text = $"{currentProgress}/{data.Target}";
        progressFill.fillAmount = (float)currentProgress / data.Target;

        icon.sprite = SpriteLibControl.Instance.GetSprite(data.IconName);

        if (currentProgress >= data.Target)
        {
            actionButton.gameObject.SetActive(false);
            doneIcon.SetActive(true);
        }
        else
        {
            actionButton.gameObject.SetActive(true);
            doneIcon.SetActive(false);
            actionButton.GetComponentInChildren<Text>().text = "CHƠI";
        }
    }
}
