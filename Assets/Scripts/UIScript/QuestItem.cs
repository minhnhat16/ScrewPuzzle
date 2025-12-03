using Managers;
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


    public void OnEnable()
    {
        actionButton.onClick.AddListener(() =>
        {
            // Start mission gameplay
            LoadLevelConclick();


        });
    }
    private void OnDisable()
    {
        actionButton.onClick.RemoveListener(() =>
        {
            LoadLevelConclick();
        });
    }


    public void LoadLevelConclick()
    {
        Debug.Log($"Starting mission: {missionData.Description}");

        DialogManager.ins.HideDialog(DialogIndex.QuestDialog, () =>
        {
            // Here you would add the logic to start the mission gameplay
            int lv = LevelManager.ins.currentLevelID;
            LevelManager.ins.LoadLevel(lv);
        });
    }
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
